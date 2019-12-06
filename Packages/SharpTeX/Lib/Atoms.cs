//////////////////////////////////////////////////////////////////////////
/// This is an adaptation of the iOS LaTeX library by Kostub Deshmukh
/// Initial GitHub project: https://github.com/kostub/iosMath
///
//////////////////////////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTeX
{
	public class	NSRange {
		public int	location = 0, length = 0;
		public NSRange( int _location, int _length ) {
			location = _location;
			length = _length;
		}

		public static bool	operator==( NSRange a, NSRange b ) {
			return a.location == b.location && a.length == b.length;
		}
		public static bool	operator!=( NSRange a, NSRange b ) {
			return a.location != b.location || a.length != b.length;
		}
		public override bool Equals(object obj) {
			NSRange	other = obj as NSRange;
			return other != null && other == this;
		}
		public override int GetHashCode() {
			return location.GetHashCode() ^ length.GetHashCode();
		}
	}

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

			NOT_FOUND = -1,
		}

		#endregion

		#region FIELDS

		public TYPE			type;				// The type of the atom.
		public string		nucleus = "";		// The nucleus of the atom
		protected AtomsList	superScript = null;	// An optional superscript.
		protected AtomsList	subScript = null;	// An optional subscript.
		public FontStyle	fontStyle;			// The font style to be used for the atom.

		// The index range in the MTMathList this MTMathAtom tracks. This is used by the finalizing and preprocessing steps which fuse MTMathAtoms to track the position of the current MTMathAtom in the original list.
		public NSRange		indexRange;

		// If this atom was formed by fusion of multiple atoms, then this stores the list of atoms that were fused to create this one.
		// This is used in the finalizing and preprocessing steps.
		List< Atom >		_fusedAtoms = null;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Returns true if this atom allows scripts (sub or super).
		/// </summary>
		public bool			ScriptsAllowed { get { return type < TYPE.kMTMathAtomBoundary; } }

		public string		Description { get { return ": " + ToString(); } }

		public AtomsList	SubScript {
			get { return subScript; }
			set {
				if ( value != null && !ScriptsAllowed ) {
					throw new Exception( @"Subscripts not allowed for atom of type " + AtomFactory.TypeToText( type ) );
				}

				subScript = value;
			}
		}

		public AtomsList	SuperScript {
			get { return superScript; }
			set {
				if ( value != null && !ScriptsAllowed ) {
					throw new Exception( @"Superscripts not allowed for atom of type " + AtomFactory.TypeToText( type ) );
				}

				superScript = value;
			}
		}

		/// <summary>
		/// Returns a Finalized copy of the atom
		/// </summary>
		public virtual Atom	Finalized {
			get {
// 				Atom	newNode = Atom.CreateAtomWithType( type, nucleus );
// 				if ( newNode.superScript != null ) {
// 					 newNode.superScript = newNode.superScript.Finalized;
// 				}
// 				if ( newNode.subScript != null ) {
// 					newNode.subScript = newNode.subScript.Finalized;
// 				}
				Atom	newNode = Copy( ( FieldInfo _fieldInfo, object _fieldValue ) => {
					if ( _fieldInfo.FieldType == typeof(Atom) ) {
						Atom	source = _fieldValue as Atom;
						return source != null ? source.Finalized : null;
					} else if ( _fieldInfo.FieldType == typeof(AtomsList) ) {
						AtomsList	source = _fieldValue as AtomsList;
						return source != null ? source.Finalized : null;
// 					} else if ( _fieldInfo.FieldType == typeof(string) ) {
// 						return new string( _fieldValue as string );
					}

					return _fieldValue;
				} );
				return newNode;
			}
		}

		#endregion

		#region METHODS

 		public Atom( TYPE _type, string _nucleus ) {
 			type = _type;
 			nucleus = _nucleus;
 		}
 		public Atom( TYPE _type ) {
 			type = _type;
 		}

		/// <summary>
		/// Returns a string representation of the MTMathAtom
		/// </summary>
		public override string ToString() {
			string	str = nucleus;
			str += AppendSubSuper();
			return str;
		}

		public static Atom	Create( TYPE _type, string _nucleus ) {
			switch ( _type ) {
				case TYPE.kMTMathAtomFraction:		return new AtomFraction( true );
				case TYPE.kMTMathAtomPlaceholder:	return new Atom( TYPE.kMTMathAtomPlaceholder, @"\u25A1" );		// A placeholder is created with a white square.
				case TYPE.kMTMathAtomRadical:		return new AtomRadical();
				case TYPE.kMTMathAtomLargeOperator:	return new AtomLargeOperator( _nucleus, true );					// Default setting of limits is true
				case TYPE.kMTMathAtomInner:			return new AtomInner();
				case TYPE.kMTMathAtomOverline:		return new AtomOverLine();
				case TYPE.kMTMathAtomUnderline:		return new AtomUnderLine();
				case TYPE.kMTMathAtomAccent:		return new AtomAccent( _nucleus );
				case TYPE.kMTMathAtomSpace:			return new AtomSpace( 0.0f );
				case TYPE.kMTMathAtomColor:			return new AtomColor();
				case TYPE.kMTMathAtomColorbox:		return new AtomColorBox();
				default:							return new Atom( _type, _nucleus );
			}
		}

		/// <summary>
		/// This is used everywhere to add super/subscripts to the ToString() representation
		/// </summary>
		/// <returns></returns>
		protected string	AppendSubSuper() {
			string	str = "";
			if ( superScript != null ) {
				str += @"^{" + superScript + "}";
			}
			if ( subScript != null ) {
				str += @"_{" + subScript + "}";
			}
			return str;
		}

		/// <summary>
		/// Fuse the given atom with this one by combining their nucleii.
		/// </summary>
		/// <param name="atom"></param>
		public void	Fuse( Atom atom ) {
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
			indexRange = new NSRange( indexRange.location, indexRange.length + atom.indexRange.length );

			// Update super/sub scripts
			subScript = atom.subScript;
			superScript = atom.superScript;
		}

		#region Helpers

		// Returns true if the current binary operator is not really binary.
		public bool	IsNotBinaryOperator() {
			if ( type == TYPE.kMTMathAtomBinaryOperator || type == TYPE.kMTMathAtomRelation || type == TYPE.kMTMathAtomOpen || type == TYPE.kMTMathAtomPunctuation || type == TYPE.kMTMathAtomLargeOperator ) {
				return true;
			}

			return false;
		}

		protected delegate object	DelegateCopyField( FieldInfo _fieldInfo, object _sourceField );
		protected Atom		Copy( DelegateCopyField _fieldCopier ) {
			Atom	result = Create( type, nucleus );

			// Perform a deep copy
			FieldInfo[]	fields = GetType().GetFields( BindingFlags.NonPublic | BindingFlags.Public );
			foreach ( FieldInfo field in fields ) {
				field.SetValue( result, _fieldCopier( field, field.GetValue( this ) ) );
			}

			return result;
		}

		public Atom		Copy() {
			return Copy( ( FieldInfo _fieldInfo, object _fieldValue ) => {
									if ( _fieldInfo.FieldType == typeof(Atom) ) {
										Atom	source = _fieldValue as Atom;
										return source != null ? source.Copy() : null;
									} else if ( _fieldInfo.FieldType == typeof(AtomsList) ) {
										AtomsList	source = _fieldValue as AtomsList;
										return source != null ? source.Copy() : null;
									}

									return _fieldValue;
								} );
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

// 		public override Atom	Finalized { get
// 			{
// 				AtomFraction	newFrac = base.Finalized;
// 						newFrac.numerator = newFrac.numerator.Finalized;
// 						newFrac.denominator = newFrac.denominator.Finalized;
// 				return newFrac;
// 			}
// 		}

		public AtomFraction( bool _hasRule ) : base( TYPE.kMTMathAtomFraction ) {
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

			str += AppendSubSuper();

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

		public AtomsList	radicand = null;	// Denotes the term under the square root sign
		public AtomsList	degree = null;		// Denotes the degree of the radical, i.e. the value to the top left of the radical sign
												// This can be null if there is no degree.

		public	AtomRadical() : base( TYPE.kMTMathAtomRadical ) {
		}

		public override string ToString() {
			string	str = @"\\sqrt";
			if ( degree != null ) {
				str += "[" + degree + "]";
			}
			str += "{" + radicand + "}";
			str += AppendSubSuper();
			return str;
		}

// - (id)copyWithZone:(NSZone *)zone
// {
//     MTRadical* rad = [super copyWithZone:zone];
//     rad.radicand = [self.radicand copyWithZone:zone];
//     rad.degree = [self.degree copyWithZone:zone];
//     return rad;
// }
// 
// - (instancetype)Finalized
// {
//     MTRadical* newRad = [super Finalized];
//     newRad.radicand = newRad.radicand.Finalized;
//     newRad.degree = newRad.degree.Finalized;
//     return newRad;
// }
	}

	/// <summary>
	/// A `MTMathAtom` of type `kMTMathAtomLargeOperator`.
	/// </summary>
	public class AtomLargeOperator : Atom {

		public bool	limits = true;		// Indicates whether the limits (if present) should be displayed above and below the operator in display mode.
										// If limits is false then the limits (if present) and displayed like a regular subscript/superscript.

		public AtomLargeOperator( string _nucleus, bool _limits ) : base( TYPE.kMTMathAtomLargeOperator, _nucleus ) {
			limits = _limits;
		}
		public AtomLargeOperator( bool _limits ) : this( "", _limits ) {
		}

	}

	/// <summary>
	/// An inner atom. This denotes an atom which contains a math list inside it. An inner atom has optional boundaries. Note: Only one boundary may be present, it is not required to have both. 
	/// </summary>
	public class	AtomInner : Atom {

		public AtomsList	innerList = null;		// The inner math list
		Atom				leftBoundary = null;	// The left boundary atom. This must be a node of type kMTMathAtomBoundary
		Atom				rightBoundary = null;	// The right boundary atom. This must be a node of type kMTMathAtomBoundary

		public Atom	LeftBoundary {
			get { return leftBoundary; }
			set {
				if ( value != null && value.type != TYPE.kMTMathAtomBoundary ) throw new Exception( "Left boundary must be of type kMTMathAtomBoundary" );
				leftBoundary = value;
			}
		}

		public Atom	RightBoundary {
			get { return rightBoundary; }
			set {
				if ( value != null && value.type != TYPE.kMTMathAtomBoundary ) throw new Exception( "Right boundary must be of type kMTMathAtomBoundary" );
				rightBoundary = value;
			}
		}

		public AtomInner() : base( TYPE.kMTMathAtomInner ) {

		}

		public override string ToString() {
			string	str = @"\\inner";
			if ( leftBoundary != null ) {
				str += "[" + leftBoundary.nucleus + "]";
			}
			str += "{" + innerList + "}";
			if ( rightBoundary != null ) {
				str += "[" + rightBoundary.nucleus + "]";
			}
    
			str += AppendSubSuper();

			return str;
		}

// 		- (id)copyWithZone:(NSZone *)zone
// 		{
// 			MTInner* inner = [super copyWithZone:zone];
// 			inner.innerList = [self.innerList copyWithZone:zone];
// 			inner.leftBoundary = [self.leftBoundary copyWithZone:zone];
// 			inner.rightBoundary = [self.rightBoundary copyWithZone:zone];
// 			return inner;
// 		}
// 
// 		- (instancetype)Finalized
// 		{
// 			MTInner *newInner = [super Finalized];
// 			newInner.innerList = newInner.innerList.Finalized;
// 			return newInner;
// 		}
	}

	/// <summary>
	/// An atom with a line over the contained math list.
	/// </summary>
	public class AtomOverLine : Atom {

		public AtomsList	innerList = null;

		public AtomOverLine() : base( TYPE.kMTMathAtomOverline ) {}
	}

	/// <summary>
	/// An atom with a line under the contained math list.
	/// </summary>
	public class AtomUnderLine : Atom {

		public AtomsList	innerList = null;

		public AtomUnderLine() : base( TYPE.kMTMathAtomUnderline ) {}
	}

	/// <summary>
	/// An atom with an accent.
	/// </summary>
	public class AtomAccent : Atom {

		public AtomsList	innerList = null;

		public AtomAccent( string _nucleus ) : base( TYPE.kMTMathAtomAccent, _nucleus ) {}
		public AtomAccent() : base( TYPE.kMTMathAtomAccent ) {}
	}

	/// <summary>
	/// An atom representing space.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this
	/// class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomSpace : Atom {

		public float	space = 0.0f;

		public AtomSpace( float _space ) : base( TYPE.kMTMathAtomSpace ) {
			space = _space;
		}
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

		public LineStyle	style;

		public AtomStyle( LineStyle _style ) : base( TYPE.kMTMathAtomStyle ) {
			style = _style;
		}
	}

	/// <summary>
	/// An atom representing an color element.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomColor : Atom {

		public string		colorString = "";
		public AtomsList	innerList = null;

		public	AtomColor( ) : base( TYPE.kMTMathAtomColor ) {
		}

		public override string ToString() {
			string	str = @"\\color";
			str += "{" + colorString + "}{" + innerList + "}";
			return str;
		}
	}

	/// <summary>
	/// An atom representing an colorbox element.
	/// @note None of the usual fields of the `MTMathAtom` apply even though this class inherits from `MTMathAtom`. i.e. it is meaningless to have a value in the nucleus, subscript or superscript fields.
	/// </summary>
	public class AtomColorBox : AtomColor {

		public	AtomColorBox( ) : base() {
			type = TYPE.kMTMathAtomColorbox;
		}

		public override string ToString() {
			string	str = @"\\colorbox";
			str += "{" + colorString + "}{" + innerList + "}";
			return str;
		}
	}

	/// <summary>
	/// An atom representing an table element. This atom is not like other atoms and is not present in TeX. We use it to represent the `\halign` command in TeX with some simplifications. This is used for matrices, equation alignments and other uses of multiline environments.
	/// The cells in the table are represented as a two dimensional array of `MTMathList` objects. The `MTMathList`s could be empty to denote a missing value in the cell. Additionally an array of alignments indicates how each column will be aligned.
	/// </summary>
	public class AtomTable : Atom {

		public enum ColumnAlignment {
			kMTColumnAlignmentLeft,			// Align left.
			kMTColumnAlignmentCenter,		// Align center.
			kMTColumnAlignmentRight,		// Align right.
		}

		public ColumnAlignment[]	alignments = new ColumnAlignment[0];	// The alignment for each column (left, right, center). The default alignment for a column (if not set) is center.
		public AtomsList[,]			cells = new AtomsList[0,0];				// The cells in the table as a two dimensional array.
		public string				environment = "";						// The name of the environment that this table denotes.
		public float				interColumnSpacing;						// Spacing between each column in mu units.
		public float				interRowAdditionalSpacing;				// Additional spacing between rows in jots (one jot is 0.3 times font size).
																			// If the additional spacing is 0, then normal row spacing is used are used.

		public uint					RowsCount		{ get { return (uint) cells.GetLength(0); } }
		public uint					ColumnsCount	{ get { return (uint) cells.GetLength(1); } }
		public AtomsList			this[uint row, uint column] {
			get { return cells[row,column]; }
			set { cells[row,column] = value; }
		}

		public override Atom	Finalized {
			get {
				AtomTable	result = new AtomTable( environment );

				result.alignments = new ColumnAlignment[alignments.Length];
				alignments.CopyTo( result.alignments, 0 );

				result.cells = new AtomsList[cells.GetLength(0), cells.GetLength(1)];
				for ( uint rowIndex=0; rowIndex < RowsCount; rowIndex++ ) {
					for ( uint columnIndex=0; columnIndex < ColumnsCount; columnIndex++ ) {
						AtomsList	sourceList = cells[rowIndex, columnIndex];
						result.cells[rowIndex, columnIndex] = sourceList != null ? sourceList.Finalized : null;
					}
				}

				result.interColumnSpacing = interRowAdditionalSpacing;
				result.interRowAdditionalSpacing = interRowAdditionalSpacing;

				return result;
			}
		}

		public	AtomTable( string _environment ) : base( TYPE.kMTMathAtomTable ) {
			environment = _environment;
		}

		/// <summary>
		/// Set the value of a given cell. The table is automatically resized to contain this cell.
		/// </summary>
		/// <param name="_row"></param>
		/// <param name="_colum"></param>
		/// <param name="_value"></param>
		public void		SetCell( uint _row, uint _colum, AtomsList _value ) {
			ResizeAtLeast( _row, _colum );
			cells[_row,_colum] = _value;
		}

		/// <summary>
		/// Set the alignment of a particular column. The table is automatically resized to contain this column and any new columns added have their alignment set to center.
		/// </summary>
		/// <param name="_column"></param>
		/// <param name="_alignment"></param>
		public void		SetAlignment( uint _column, ColumnAlignment _alignment ) {
			ResizeAtLeast( RowsCount, _column );
			alignments[_column] = _alignment;
		}

		/// <summary>
		/// Gets the alignment for a given column. If the alignment is not specified it defaults to center.
		/// </summary>
		/// <param name="_column"></param>
		/// <returns></returns>
		public ColumnAlignment	GetAlignment( uint _column ) {
			return _column < ColumnsCount ? alignments[_column] : ColumnAlignment.kMTColumnAlignmentCenter;
		}

		/// <summary>
		/// Inserts a "style atom" for every cell of the table
		/// </summary>
		/// <param name="_atom"></param>
		public void		InsertStyleAtom( Atom _atom ) {
			for ( uint i = 0; i < RowsCount; i++ ) {
				for ( uint j = 0; j < ColumnsCount; j++ ) {
					this[i,j].InsertAtom( 0, _atom );
				}
			}
		}

		void	ResizeAtLeast( uint _rows, uint _colums ) {
			if ( _rows <= RowsCount && _colums <= ColumnsCount )
				return;	// Already to size

			if ( _colums > ColumnsCount ) {
				// Resize columns' alignments and initialize new alignments to "center"
				ColumnAlignment[]	oldAlignments = alignments;
				alignments = new ColumnAlignment[_colums];
				oldAlignments.CopyTo( alignments, 0 );
				for ( uint columnIndex=(uint) oldAlignments.Length; columnIndex < _colums; columnIndex++ ) {
					alignments[columnIndex] = ColumnAlignment.kMTColumnAlignmentCenter;
				}
			}

			// Resize table
			uint			oldRowsCount = RowsCount;
			uint			oldColumnsCount = RowsCount;
			AtomsList[,]	oldCells = cells;

			cells = new AtomsList[_rows,_colums];
			Array.Clear( cells, 0, (int) (_rows*_colums) );
			for ( uint rowIndex=0; rowIndex < oldRowsCount; rowIndex++ ) {
				for ( uint columnIndex=0; columnIndex < oldColumnsCount; columnIndex++ ) {
					cells[rowIndex,columnIndex] = oldCells[rowIndex,columnIndex];
				}
			}
		}
	}
}
