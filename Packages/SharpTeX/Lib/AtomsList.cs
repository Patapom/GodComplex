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

namespace SharpTeX
{
	/// <summary>
	/// Class holding a LaTeX list of "atoms"
	/// Equivalent of the original MTMathList class in the original library
	/// 
	/// This list can be constructed directly or built with the help of the MTMathListBuilder. It is not required that the mathematics represented make sense
	/// (i.e. this can represent something like "x 2 = +". This list can be used for display using MTLine or can be a list of tokens to be used by a parser after finalizedMathList is called.
	/// </summary>
    public class AtomsList {

		#region NESTED TYPES

		#endregion

		#region FIELDS

		List< Atom >	atoms = new List<Atom>();	// A list of math atoms

		#endregion

		#region PROPERTIES

		public Atom	Last { get { return atoms.Count > 0 ? atoms[atoms.Count-1] : null; } }

		#endregion

		#region METHODS

		public AtomsList() {
		}

		public	AtomsList( params Atom[] _atoms ) {
			if ( _atoms == null )
				return;

			foreach ( Atom atom in _atoms ) {
				if ( atom != null )
					AddAtom( atom );
			}
		}

		/// <summary>
		/// Converts the list into a string form. Note: This is not the LaTeX form.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			string	str = "";
			foreach ( Atom atom in atoms ) {
				str += atom.ToString();
			}
			return str;
		}

		/// <summary>
		/// Add an atom to the end of the list.
		/// </summary>
		/// <param name="_atom">The atom to be inserted. This cannot be `null` and cannot have the type `kMTMathAtomBoundary`.
		/// @throws NSException if the atom is of type `kMTMathAtomBoundary`
		/// @throws NSInvalidArgumentException if the atom is `null`
		/// </param>
		public void		AddAtom( Atom _atom ) {
			if ( _atom == null )
				throw new Exception( "Invalid atom!" );
			if ( _atom.type == Atom.TYPE.kMTMathAtomBoundary )
				throw new Exception( "Forbidden atom type!" );

			atoms.Add( _atom );
		}

		/// <summary>
		///Inserts an atom at the given index. If index is already occupied, the objects at index and beyond are shifted by adding 1 to their indices to make room.
		/// </summary>
		/// <param name="_atom">The atom to be inserted. This cannot be `null` and cannot have the type `kMTMathAtomBoundary`.
		/// @throws NSException if the atom is of type `kMTMathAtomBoundary`
		/// @throws NSInvalidArgumentException if the atom is `null`
		/// </param>
		/// <param name="_index">The index where the atom is to be inserted. The index should be less than or equal to the number of elements in the math list.</param>
		public void		InsertAtom( Atom _atom, uint _index ) {
			if ( _atom == null )
				throw new Exception( "Invalid atom!" );
			if ( _atom.type == Atom.TYPE.kMTMathAtomBoundary )
				throw new Exception( "Forbidden atom type!" );

			atoms.Insert( (int) _index, _atom );
		}

		/// <summary>
		/// Append the given list to the end of the current list.
		/// </summary>
		/// <param name="list">The list to append</param>
		public void	Append( AtomsList list ) {
			atoms.AddRange( list.atoms );
		}

		// Create a new math list as a final expression and update atoms by combining like atoms that occur together and converting unary operators to binary operators.
		// This function does not modify the current list
		public AtomsList	Finalized {
			get {
				AtomsList	result = new AtomsList();
				NSRange		zeroRange = new NSRange( 0, 0 );
    
				Atom		prevNode = null;
				foreach ( Atom atom in atoms ) {
					Atom	newNode = atom.Finalized;

					// Each character is given a separate index.
					if ( atom.indexRange == zeroRange ) {
						int	index = prevNode == null ? 0 : prevNode.indexRange.location + prevNode.indexRange.length;
						newNode.indexRange = new NSRange( index, 1 );
					}

					switch ( newNode.type ) {
						case Atom.TYPE.kMTMathAtomBinaryOperator: {
							if ( prevNode == null || prevNode.IsNotBinaryOperator() ) {
								newNode.type = Atom.TYPE.kMTMathAtomUnaryOperator;
							}
							break;
						}
						case Atom.TYPE.kMTMathAtomRelation:
						case Atom.TYPE.kMTMathAtomPunctuation:
						case Atom.TYPE.kMTMathAtomClose:
							if ( prevNode != null && prevNode.type == Atom.TYPE.kMTMathAtomBinaryOperator ) {
								prevNode.type = Atom.TYPE.kMTMathAtomUnaryOperator;
							}
							break;
                
						case Atom.TYPE.kMTMathAtomNumber:
							// Combine numbers together
							if ( prevNode != null && prevNode.type == Atom.TYPE.kMTMathAtomNumber && prevNode.SubScript != null && prevNode.SuperScript != null ) {
								prevNode.Fuse( newNode );
								newNode = prevNode;		// skip the current node, we are done here.
							}
							break;
                
						default:
							break;
					}

					if ( newNode != prevNode ) {
						result.AddAtom( newNode );
						prevNode = newNode;
					}
				}

				if ( prevNode != null && prevNode.type == Atom.TYPE.kMTMathAtomBinaryOperator ) {
					prevNode.type = Atom.TYPE.kMTMathAtomUnaryOperator;		// it isn't a binary since there is noting after it. Make it a unary
				}

				return result;
			}
		}


/*

// /** Create a `MTMathList` given a list of atoms. The list of atoms should be terminated by `null`.
+ (instancetype)mathListWithAtoms:(MTMathAtom *)firstAtom, ...
{
    MTMathList* list = [[MTMathList alloc] init];
    va_list args;
    va_start(args, firstAtom);
    for (MTMathAtom* atom = firstAtom; atom != null; atom = va_arg(args, MTMathAtom*))
    {
        [list addAtom:atom];
    }
    va_end(args);
    return list;
}

// Create a `MTMathList` given a list of atoms.
// + (instancetype) mathListWithAtomsArray:(NSArray<MTMathAtom*>*) atoms;
+ (instancetype)mathListWithAtomsArray:(NSArray<MTMathAtom *> *)atoms
{
    MTMathList* list = [[MTMathList alloc] init];
    [list->_atoms addObjectsFromArray:atoms];
    return list;
}

// Initializes an empty math list.
- (instancetype)init
{
    self = [super init];
    if (self) {
        _atoms = [NSMutableArray array];
    }
    return self;
}

- (bool) isAtomAllowed:(MTMathAtom*) atom
{
    return atom.type != kMTMathAtomBoundary;
}

// Add an atom to the end of the list.
//  @param atom The atom to be inserted. This cannot be `null` and cannot have the type `kMTMathAtomBoundary`.
//  @throws NSException if the atom is of type `kMTMathAtomBoundary`
//  @throws NSInvalidArgumentException if the atom is `null`
- (void)addAtom:(MTMathAtom *)atom
{
    NSParameterAssert(atom);
    if (![self isAtomAllowed:atom]) {
        @throw [[NSException alloc] initWithName:@"Error"
                                          reason:[NSString stringWithFormat:@"Cannot add atom of type %@ in a mathlist", typeToText(atom.type)]
                                        userInfo:null];
    }
    [_atoms addObject:atom];
}

// Inserts an atom at the given index. If index is already occupied, the objects at index and beyond are 
//  shifted by adding 1 to their indices to make room.
//  
//  @param atom The atom to be inserted. This cannot be `null` and cannot have the type `kMTMathAtomBoundary`.
//  @param index The index where the atom is to be inserted. The index should be less than or equal to the
//  number of elements in the math list.
//  @throws NSException if the atom is of type kMTMathAtomBoundary
//  @throws NSInvalidArgumentException if the atom is null
//  @throws NSRangeException if the index is greater than the number of atoms in the math list.
- (void)insertAtom:(MTMathAtom *)atom atIndex:(NSUInteger) index
{
    if (![self isAtomAllowed:atom]) {
        @throw [[NSException alloc] initWithName:@"Error"
                                          reason:[NSString stringWithFormat:@"Cannot add atom of type %@ in a mathlist", typeToText(atom.type)]
                                        userInfo:null];
    }
    [_atoms insertObject:atom atIndex:index];
}


			// Removes the last atom from the math list. If there are no atoms in the list this does nothing.
- (void)removeLastAtom
{
    if (_atoms.count > 0) {
        [_atoms removeLastObject];
    }
}

			// Removes the atom at the given index.
			//  @param index The index at which to remove the atom. Must be less than the number of atoms in the list.
- (void) removeAtomAtIndex:(NSUInteger)index
{
    [_atoms removeObjectAtIndex:index];
}

			// Removes all the atoms within the given range.
- (void) removeAtomsInRange:(NSRange) range
{
    [_atoms removeObjectsInRange:range];
}


#pragma mark NSCopying

// Makes a deep copy of the list
- (id)copyWithZone:(NSZone *)zone
{
    MTMathList* list = [[[self class] allocWithZone:zone] init];
    list->_atoms = [[NSMutableArray alloc] initWithArray:self.atoms copyItems:YES];
    return list;
}
*/


		#endregion
    }
}
