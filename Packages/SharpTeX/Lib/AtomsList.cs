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

		public string	stringValue {
			get {
				return null;
			}
		}

		#endregion

		#region METHODS

		public AtomsList() {
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
		/// <param name="_atom">The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
		/// @throws NSException if the atom is of type `kMTMathAtomBoundary`
		/// @throws NSInvalidArgumentException if the atom is `nil`
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
		/// <param name="_atom">The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
		/// @throws NSException if the atom is of type `kMTMathAtomBoundary`
		/// @throws NSInvalidArgumentException if the atom is `nil`
		/// </param>
		/// <param name="_index">The index where the atom is to be inserted. The index should be less than or equal to the number of elements in the math list.</param>
		public void		InsertAtom( Atom _atom, uint _index ) {
			if ( _atom == null )
				throw new Exception( "Invalid atom!" );
			if ( _atom.type == Atom.TYPE.kMTMathAtomBoundary )
				throw new Exception( "Forbidden atom type!" );

			atoms.Insert( (int) _index, _atom );
		}


/*

// /** Create a `MTMathList` given a list of atoms. The list of atoms should be terminated by `nil`.
+ (instancetype)mathListWithAtoms:(MTMathAtom *)firstAtom, ...
{
    MTMathList* list = [[MTMathList alloc] init];
    va_list args;
    va_start(args, firstAtom);
    for (MTMathAtom* atom = firstAtom; atom != nil; atom = va_arg(args, MTMathAtom*))
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
//  @param atom The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
//  @throws NSException if the atom is of type `kMTMathAtomBoundary`
//  @throws NSInvalidArgumentException if the atom is `nil`
- (void)addAtom:(MTMathAtom *)atom
{
    NSParameterAssert(atom);
    if (![self isAtomAllowed:atom]) {
        @throw [[NSException alloc] initWithName:@"Error"
                                          reason:[NSString stringWithFormat:@"Cannot add atom of type %@ in a mathlist", typeToText(atom.type)]
                                        userInfo:nil];
    }
    [_atoms addObject:atom];
}

// Inserts an atom at the given index. If index is already occupied, the objects at index and beyond are 
//  shifted by adding 1 to their indices to make room.
//  
//  @param atom The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
//  @param index The index where the atom is to be inserted. The index should be less than or equal to the
//  number of elements in the math list.
//  @throws NSException if the atom is of type kMTMathAtomBoundary
//  @throws NSInvalidArgumentException if the atom is nil
//  @throws NSRangeException if the index is greater than the number of atoms in the math list.
- (void)insertAtom:(MTMathAtom *)atom atIndex:(NSUInteger) index
{
    if (![self isAtomAllowed:atom]) {
        @throw [[NSException alloc] initWithName:@"Error"
                                          reason:[NSString stringWithFormat:@"Cannot add atom of type %@ in a mathlist", typeToText(atom.type)]
                                        userInfo:nil];
    }
    [_atoms insertObject:atom atIndex:index];
}

// Append the given list to the end of the current list.
//  @param list The list to append.
- (void)append:(MTMathList *)list
{
    [_atoms addObjectsFromArray:list.atoms];
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

// Create a new math list as a final expression and update atoms
// by combining like atoms that occur together and converting unary operators to binary operators.
// This function does not modify the current MTMathList
- (MTMathList *)finalized
{
    MTMathList* finalized = [MTMathList new];
    NSRange zeroRange = NSMakeRange(0, 0);
    
    MTMathAtom* prevNode = nil;
    for (MTMathAtom* atom in self.atoms) {
        MTMathAtom* newNode = [atom finalized];
        // Each character is given a separate index.
        if (NSEqualRanges(zeroRange, atom.indexRange)) {
            NSUInteger index = (prevNode == nil) ? 0 : prevNode.indexRange.location + prevNode.indexRange.length;
            newNode.indexRange = NSMakeRange(index, 1);
        }

        switch (newNode.type) {
            case kMTMathAtomBinaryOperator: {
                if (isNotBinaryOperator(prevNode)) {
                    newNode.type = kMTMathAtomUnaryOperator;
                }
                break;
            }
            case kMTMathAtomRelation:
            case kMTMathAtomPunctuation:
            case kMTMathAtomClose:
                if (prevNode && prevNode.type == kMTMathAtomBinaryOperator) {
                    prevNode.type = kMTMathAtomUnaryOperator;
                }
                break;
                
            case kMTMathAtomNumber:
                // combine numbers together
                if (prevNode && prevNode.type == kMTMathAtomNumber && !prevNode.subScript && !prevNode.superScript) {
                    [prevNode fuse:newNode];
                    // skip the current node, we are done here.
                    continue;
                }
                break;
                
            default:
                break;
        }
        [finalized addAtom:newNode];
        prevNode = newNode;
    }
    if (prevNode && prevNode.type == kMTMathAtomBinaryOperator) {
        // it isn't a binary since there is noting after it. Make it a unary
        prevNode.type = kMTMathAtomUnaryOperator;
    }
    return finalized;
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
