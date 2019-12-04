//////////////////////////////////////////////////////////////////////////
/// This is an adaptation of the iOS LaTeX library by Kostub Deshmukh
/// Initial GitHub project: https://github.com/kostub/iosMath
///
//////////////////////////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLaTeX
{
	/// <summary>
	/// An Atom is the basic unit of a math list. Each atom represents a single character or mathematical operator in a list.
	/// However certain atoms can represent more complex structures such as fractions and radicals.
	/// Each atom has a type which determines how the atom is rendered and a nucleus.
	/// The nucleus contains the character(s) that need to be rendered. However the nucleus may be empty for certain types of atoms.
	/// An atom has an optional subscript or superscript which represents the subscript or superscript that is to be rendered.
	/// 
	/// Certain types of atoms inherit from `MTMathAtom` and may have additional fields.
	/// </summary>
    public class Atom {

		#region NESTED TYPES

		/// <summary>
		/// The type of atom in a `MTMathList`.
		/// The type of the atom determines how it is rendered, and spacing between the atoms.
		/// </summary>
		/// 
		public enum TYPE {
			/// A number or text in ordinary format - Ord in TeX
			kMTMathAtomOrdinary = 1,
			/// A number - Does not exist in TeX
			kMTMathAtomNumber,
			/// A variable (i.e. text in italic format) - Does not exist in TeX
			kMTMathAtomVariable,
			/// A large operator such as (sin/cos, integral etc.) - Op in TeX
			kMTMathAtomLargeOperator,
			/// A binary operator - Bin in TeX
			kMTMathAtomBinaryOperator,
			/// A unary operator - Does not exist in TeX.
			kMTMathAtomUnaryOperator,
			/// A relation, e.g. = > < etc. - Rel in TeX
			kMTMathAtomRelation,
			/// Open brackets - Open in TeX
			kMTMathAtomOpen,
			/// Close brackets - Close in TeX
			kMTMathAtomClose,
			/// An fraction e.g 1/2 - generalized fraction noad in TeX
			kMTMathAtomFraction,
			/// A radical operator e.g. sqrt(2)
			kMTMathAtomRadical,
			/// Punctuation such as , - Punct in TeX
			kMTMathAtomPunctuation,
			/// A placeholder square for future input. Does not exist in TeX
			kMTMathAtomPlaceholder,
			/// An inner atom, i.e. an embedded math list - Inner in TeX
			kMTMathAtomInner,
			/// An underlined atom - Under in TeX
			kMTMathAtomUnderline,
			/// An overlined atom - Over in TeX
			kMTMathAtomOverline,
			/// An accented atom - Accent in TeX
			kMTMathAtomAccent,
    
			// Atoms after this point do not support subscripts or superscripts
    
			/// A left atom - Left & Right in TeX. We don't need two since we track boundaries separately.
			kMTMathAtomBoundary = 101,
    
			// Atoms after this are non-math TeX nodes that are still useful in math mode. They do not have
			// the usual structure.
    
			/// Spacing between math atoms. This denotes both glue and kern for TeX. We do not
			/// distinguish between glue and kern.
			kMTMathAtomSpace = 201,
			/// Denotes style changes during rendering.
			kMTMathAtomStyle,
			kMTMathAtomColor,
			kMTMathAtomColorbox,
    
			// Atoms after this point are not part of TeX and do not have the usual structure.
    
			/// An table atom. This atom does not exist in TeX. It is equivalent to the TeX command
			/// halign which is handled outside of the TeX math rendering engine. We bring it into our
			/// math typesetting to handle matrices and other tables.
			kMTMathAtomTable = 1001,
		}

		/// <summary>
		/// The font style of a character.
		/// The fontstyle of the atom determines what style the character is rendered in. This only applies to atoms of type kMTMathAtomVariable and kMTMathAtomNumber.
		/// None of the other atom types change their font style.
		/// </summary>
		public enum FontStyle {
			/// The default latex rendering style. i.e. variables are italic and numbers are roman.
			kMTFontStyleDefault = 0,
			/// Roman font style i.e. \mathrm
			kMTFontStyleRoman,
			/// Bold font style i.e. \mathbf
			kMTFontStyleBold,
			/// Caligraphic font style i.e. \mathcal
			kMTFontStyleCaligraphic,
			/// Typewriter (monospace) style i.e. \mathtt
			kMTFontStyleTypewriter,
			/// Italic style i.e. \mathit
			kMTFontStyleItalic,
			/// San-serif font i.e. \mathss
			kMTFontStyleSansSerif,
			/// Fractur font i.e \mathfrak
			kMTFontStyleFraktur,
			/// Blackboard font i.e. \mathbb
			kMTFontStyleBlackboard,
			/// Bold italic
			kMTFontStyleBoldItalic,
		}

		#endregion

		#region FIELDS

		public TYPE			type;				// The type of the atom.
		public string		nucleus = null;		// The nucleus of the atom
		protected AtomsList	superScript = null;	// An optional superscript.
		protected AtomsList	subScript = null;	// An optional subscript.
		public FontStyle	fontStyle;			// The font style to be used for the atom.

		// The index range in the MTMathList this MTMathAtom tracks. This is used by the finalizing and preprocessing steps which fuse MTMathAtoms to track the position of the current MTMathAtom in the original list.
		Tuple<uint,uint>	indexRange;

		// If this atom was formed by fusion of multiple atoms, then this stores the list of atoms that were fused to create this one.
		// This is used in the finalizing and preprocessing steps.
		List< Atom >		_fusedAtoms = null;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Returns true if this atom allows scripts (sub or super).
		/// </summary>
		public bool		scriptsAllowed { get { return type < TYPE.kMTMathAtomBoundary; } }

		public string	description { get { return ": " + ToString(); } }

		public void	SetSubScript( AtomsList _value ) {
		   if ( _value != null && !scriptsAllowed ) {
				throw new Exception( @"Subscripts not allowed for atom of type %@", typeToText( type ) );
			}

			subScript = _value;
		}

		public void	SetSuperScript( AtomsList _value ) {
		   if ( _value != null && !scriptsAllowed ) {
				throw new Exception( @"Superscripts not allowed for atom of type %@", typeToText( type ) );
			}

			superScript = _value;
		}

///// Makes a deep copy of the atom
//- (id)copyWithZone:(nullable NSZone *)zone;
//

		/// <summary>
		/// Returns a finalized copy of the atom
		/// </summary>
		public virtual Atom	finalized {
			get {
				Atom	newNode = this.Copy();
				if ( newNode.superScript != null ) {
					 newNode.superScript = newNode.superScript.finalized;
				}
				if ( newNode.subScript != null ) {
					newNode.subScript = newNode.subScript.finalized;
				}
				return newNode;
			}
		}

		#endregion

		#region METHODS

 		public Atom( TYPE _type, string _value ) {
 			type = _type;
 			nucleus = _value;
 		}
// 		public Atom( TYPE _type ) {
// 			type = _type;
// 		}
		public Atom( string _nucleus ) {
			nucleus = _nucleus;
		}
		public Atom() {
		}

		/// <summary>
		/// Returns a string representation of the MTMathAtom
		/// </summary>
		public override string ToString() {
			string	result = nucleus;
			if ( superScript != null ) {
				result += @"^{%@}" + superScript.ToString();
			}
			if ( subScript != null ) {
				result += @"_{%@}" + subScript.ToString();
			}
			return result;
		}

		public static Atom	CreateAtomWithType( TYPE _type, string _value ) {
			switch (_type) {
				case TYPE.kMTMathAtomFraction:
					return new AtomFraction( true );
            
				case TYPE.kMTMathAtomPlaceholder:
					return new Atom( TYPE.kMTMathAtomPlaceholder, @"\u25A1" );		// A placeholder is created with a white square.
            
				case TYPE.kMTMathAtomRadical:
					return new AtomRadical();
            
				case TYPE.kMTMathAtomLargeOperator:
					return new AtomLargeOperator( _value, limits:YES );				// Default setting of limits is true
            
				case TYPE.kMTMathAtomInner:
					return new AtomInner();
            
				case TYPE.kMTMathAtomOverline:
					return new AtomOverLine();
            
				case TYPE.kMTMathAtomUnderline:
					return new AtomUnderLine();
            
				case TYPE.kMTMathAtomAccent:
					return new AtomAccent( _value );
            
				case TYPE.kMTMathAtomSpace:
					return new AtomSpace( initWithSpace:0 );
        
				case TYPE.kMTMathAtomColor:
					return new AtomColor();
            
				case TYPE.kMTMathAtomColorbox:
					return new AtomColorBox();
            
				default:
					return new Atom( _type, _value );
			}
		}

		/// <summary>
		/// Fuse the given atom with this one by combining their nucleii.
		/// </summary>
		/// <param name="atom"></param>
		void	fuse( Atom atom ) {
			if ( subScript != null ) throw new Exception( @"Cannot fuse into an atom which has a subscript: " + this );
			if ( superScript != null ) throw new Exception( @"Cannot fuse into an atom which has a superscript: " + this );
			if ( atom.type != type ) throw new Exception( @"Only atoms of the same type can be fused. " + this + ", " + atom );
    
			// Update the fused atoms list
			if ( _fusedAtoms == null ) {
				_fusedAtoms = new List<Atom>();
				_fusedAtoms.Add( this );
			}
			if ( atom._fusedAtoms != null ) {
				_fusedAtoms.AddRange( atom._fusedAtoms );
			} else {
				_fusedAtoms.Add( atom );
			}    
    
			// Update the nucleus
			nucleus += atom.nucleus;
    
			// Update the range
			indexRange = new Tuple<uint, uint>( indexRange.Item1, indexRange.Item2 + atom.indexRange.Item2 );
    
			// Update super/sub scripts
			subScript = atom.subScript;
			superScript = atom.superScript;
		}

		#region Helpers

		// Returns true if the current binary operator is not really binary.
		public static bool	IsNotBinaryOperator( Atom _prevNode ) {
			if ( _prevNode == null ) {
				return true;
			}
			if ( _prevNode.type == TYPE.kMTMathAtomBinaryOperator || _prevNode.type == TYPE.kMTMathAtomRelation || _prevNode.type == TYPE.kMTMathAtomOpen || _prevNode.type == TYPE.kMTMathAtomPunctuation || _prevNode.type == TYPE.kMTMathAtomLargeOperator ) {
				return true;
			}

			return false;
		}

		public static string	 TypeToText( TYPE _type ) {
			switch (_type) {
				case TYPE.kMTMathAtomOrdinary:				return @"Ordinary";
				case TYPE.kMTMathAtomNumber:				return @"Number";
				case TYPE.kMTMathAtomVariable:				return @"Variable";
				case TYPE.kMTMathAtomBinaryOperator:		return @"Binary Operator";
				case TYPE.kMTMathAtomUnaryOperator:			return @"Unary Operator";
				case TYPE.kMTMathAtomRelation:				return @"Relation";
				case TYPE.kMTMathAtomOpen:					return @"Open";
				case TYPE.kMTMathAtomClose:					return @"Close";
				case TYPE.kMTMathAtomFraction:				return @"Fraction";
				case TYPE.kMTMathAtomRadical:				return @"Radical";
				case TYPE.kMTMathAtomPunctuation:			return @"Punctuation";
				case TYPE.kMTMathAtomPlaceholder:			return @"Placeholder";
				case TYPE.kMTMathAtomLargeOperator:			return @"Large Operator";
				case TYPE.kMTMathAtomInner:					return @"Inner";
				case TYPE.kMTMathAtomUnderline:				return @"Underline";
				case TYPE.kMTMathAtomOverline:				return @"Overline";
				case TYPE.kMTMathAtomAccent:				return @"Accent";
				case TYPE.kMTMathAtomBoundary:				return @"Boundary";
				case TYPE.kMTMathAtomSpace:					return @"Space";
				case TYPE.kMTMathAtomStyle:					return @"Style";
				case TYPE.kMTMathAtomColor:					return @"Color";
				case TYPE.kMTMathAtomColorbox:				return @"Colorbox";
				case TYPE.kMTMathAtomTable:					return @"Table";
			}
			return null;
		}

		#endregion

		#endregion
    }

	/// <summary>
	/// An atom of type fraction. This atom has a numerator and denominator.
	/// </summary>
	public class AtomFraction : Atom {

		public bool			hasRule = true;			// If true, the fraction has a rule (i.e. a line) between the numerator and denominator.
		public AtomsList	numerator = null;		// Numerator of the fraction
		public AtomsList	denominator = null;		// Denominator of the fraction
		public string		leftDelimiter = null;	// An optional delimiter for a fraction on the left.
		public string		rightDelimiter = null;	// An optional delimiter for a fraction on the right.

		public override Atom	finalized { get
			{
				AtomFraction	newFrac = base.finalized;
						newFrac.numerator = newFrac.numerator.finalized;
						newFrac.denominator = newFrac.denominator.finalized;
				return newFrac;
			}
		}

		public AtomFraction( bool _hasRule ) : base() {
			type = Atom.TYPE.kMTMathAtomFraction;
			nucleus = @"";	// fractions have no nucleus
			hasRule = _hasRule;
		}

		public override string ToString() {
			string	str = "";
			if (hasRule) {
				str += @"\\atop";
			} else {
				str += @"\\frac";
			}
			if ( leftDelimiter != null || rightDelimiter != null ) {
				str += "[" + leftDelimiter + "][" + rightDelimiter + "]";
			}
    
			str += @"{" + numerator + "}{" + denominator + "}";
			if ( superScript != null ) {
				str += @"^{" + superScript + "}";
			}
			if ( subScript != null ) {
				str += @"_{" + subScript + "}";
			}
			return str;
		}

// 		- (id)copyWithZone:(NSZone *)zone
// 		{
// 			MTFraction* frac = [super copyWithZone:zone];
// 			frac.numerator = [self.numerator copyWithZone:zone];
// 			frac.denominator = [self.denominator copyWithZone:zone];
// 			frac->_hasRule = self.hasRule;
// 			frac.leftDelimiter = [self.leftDelimiter copyWithZone:zone];
// 			frac.rightDelimiter = [self.rightDelimiter copyWithZone:zone];
// 			return frac;
// 		}

	}


	/// <summary>
	/// An atom of type radical (square root).
	/// </summary>
	public class AtomRadical : Atom {

		//// Creates an empty radical
		//- (instancetype)init NS_DESIGNATED_INITIALIZER;
		//
		///// Denotes the term under the square root sign
		//@property (nonatomic, nullable) MTMathList* radicand;
		//
		///// Denotes the degree of the radical, i.e. the value to the top left of the radical sign
		///// This can be null if there is no degree.
		//@property (nonatomic, nullable) MTMathList* degree;

	}

	/// <summary>
	/// A `MTMathAtom` of type `kMTMathAtomLargeOperator`.
	/// </summary>
	public class AtomLargeOperator: Atom {
		// @interface MTLargeOperator : MTMathAtom
		// 
		// /** Designated initializer. Initialize a large operator with the given
		//  value and setting for limits.
		//  */
		// - (instancetype) initWithValue:(NSString*) value limits:(BOOL) limits NS_DESIGNATED_INITIALIZER;
		// 
		// /** Indicates whether the limits (if present) should be displayed
		//  above and below the operator in display mode.  If limits is false
		//  then the limits (if present) and displayed like a regular subscript/superscript.
		//  */
		// @property (nonatomic) BOOL limits;

}

	/// <summary>
	/// An inner atom. This denotes an atom which contains a math list inside it. An inner atom has optional boundaries. Note: Only one boundary may be present, it is not required to have both. 
	/// </summary>
	public class	AtomInner : Atom {

// 	// Creates an empty inner
		// 	- (instancetype)init NS_DESIGNATED_INITIALIZER;
		// 
		// 	/// The inner math list
		// 	@property (nonatomic, nullable) MTMathList* innerList;
		// 	/// The left boundary atom. This must be a node of type kMTMathAtomBoundary
		// 	@property (nonatomic, nullable) MTMathAtom* leftBoundary;
		// 	/// The right boundary atom. This must be a node of type kMTMathAtomBoundary
		// 	@property (nonatomic, nullable) MTMathAtom* rightBoundary;

	}

	/// <summary>
	/// An atom with a line over the contained math list.
	/// </summary>
	public class AtomOverLine : Atom {

	// / Creates an empty over
	// - (instancetype)init NS_DESIGNATED_INITIALIZER;
	// 
	// / The inner math list
	// @property (nonatomic, nullable) MTMathList* innerList;

	}

	/// <summary>
	/// An atom with a line under the contained math list.
	/// </summary>
	public class AtomUnderLine : Atom {

// 		/// Creates an empty under
// 		- (instancetype)init NS_DESIGNATED_INITIALIZER;
// 
// 		/// The inner math list
// 		@property (nonatomic, nullable) MTMathList* innerList;

	}

	/// <summary>
	/// An atom with an accent.
	/// </summary>
	public class AtomAccent : Atom {

// 		/** Creates a new `MTAccent` with the given value as the accent.
// 		 */
// 		- (instancetype)initWithValue:(NSString*) value NS_DESIGNATED_INITIALIZER;
// 
// 		/// The mathlist under the accent.
// 		@property (nonatomic, nullable) MTMathList* innerList;

	}

	/// <summary>
	/// An atom representing space.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this
	/// class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomSpace : Atom {

// 		- (instancetype) initWithSpace:(CGFloat) space NS_DESIGNATED_INITIALIZER;
// 
// 		/** The amount of space represented by this object in mu units. */
// 		@property (nonatomic, readonly) CGFloat space;

	}


	/// <summary>
	/// An atom representing a style change.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this class inherits from `MTMathAtom`. i.e. it is meaningless to have a value	/// in the nucleus, subscript or superscript fields
	/// </summary>
	public class AtomStyle : Atom {

		/// <summary>
		/// Styling of a line of math
		/// </summary>
		public enum LineStyle {
			/// Display style
			kMTLineStyleDisplay,
			/// Text style (inline)
			kMTLineStyleText,
			/// Script style (for sub/super scripts)
			kMTLineStyleScript,
			/// Script script style (for scripts of scripts)
			kMTLineStyleScriptScript
		};


		// 	Creates a new `MTMathStyle` with the given style.
		// 		 @param style The style to be applied to the rest of the list.
		// 		 */
		// 		- (instancetype) initWithStyle:(MTLineStyle) style NS_DESIGNATED_INITIALIZER;
		// 
		// 		/** The style represented by this object.
		// 		@property (nonatomic, readonly) MTLineStyle style;

	}

	/// <summary>
	/// An atom representing an color element.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomColor : Atom {

		// / Creates an empty color with a nil environment
		// - (instancetype) init NS_DESIGNATED_INITIALIZER;
		// 
		// /** The style represented by this object. */
		// @property (nonatomic, nullable) NSString* colorString;
		// 
		// / The inner math list
		// @property (nonatomic, nullable) MTMathList* innerList;

	}

	/// <summary>
	/// An atom representing an colorbox element.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomColorBox : Atom {

		// / Creates an empty color with a nil environment
		// - (instancetype) init NS_DESIGNATED_INITIALIZER;
		// 
		// /** The style represented by this object. */
		// @property (nonatomic, nullable) NSString* colorString;
		// 
		// / The inner math list
		// @property (nonatomic, nullable) MTMathList* innerList;
	}

	/// <summary>
	/// An atom representing an table element. This atom is not like other atoms and is not present in TeX. We use it to represent the `\halign` command in TeX with some simplifications. This is used for matrices, equation alignments and other uses of multiline environments.
	/// The cells in the table are represented as a two dimensional array of `MTMathList` objects. The `MTMathList`s could be empty to denote a missing value in the cell. Additionally an array of alignments indicates how each column will be aligned.
	/// </summary>
	public class AtomMathTable : Atom {

		public enum ColumnAlignment {
			/// Align left.
			kMTColumnAlignmentLeft,
			/// Align center.
			kMTColumnAlignmentCenter,
			/// Align right.
			kMTColumnAlignmentRight,
		}

// / Creates an empty table with a nil environment
// - (instancetype)init;
// 
// / Creates a table with a given environment
// - (instancetype)initWithEnvironment:(nullable NSString*) env NS_DESIGNATED_INITIALIZER;
// 
// / The alignment for each column (left, right, center). The default alignment
// / for a column (if not set) is center.
// @property (nonatomic, nonnull, readonly) NSArray<NSNumber*>* alignments;
// / The cells in the table as a two dimensional array.
// @property (nonatomic, nonnull, readonly) NSArray<NSArray<MTMathList*>*>* cells;
// / The name of the environment that this table denotes.
// @property (nonatomic, nullable) NSString* environment;
// 
// / Spacing between each column in mu units.
// @property (nonatomic) CGFloat interColumnSpacing;
// / Additional spacing between rows in jots (one jot is 0.3 times font size).
// / If the additional spacing is 0, then normal row spacing is used are used.
// @property (nonatomic) CGFloat interRowAdditionalSpacing;
// 
// / Set the value of a given cell. The table is automatically resized to contain this cell.
// - (void) setCell:(MTMathList*) list forRow:(NSInteger) row column:(NSInteger) column;
// 
// / Set the alignment of a particular column. The table is automatically resized to
// / contain this column and any new columns added have their alignment set to center.
// - (void) setAlignment:(MTColumnAlignment) alignment forColumn:(NSInteger) column;
// 
// / Gets the alignment for a given column. If the alignment is not specified it defaults
// / to center.
// - (MTColumnAlignment) getAlignmentForColumn:(NSInteger) column;
// 
// / Number of columns in the table.
// - (NSUInteger) numColumns;
// 
// / Number of rows in the table.
// - (NSUInteger) numRows;

	}
}
