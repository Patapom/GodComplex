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

		List< Atom >	m_atoms = new List<Atom>();

		#endregion

		#region PROPERTIES

		public string	stringValue {
			get {
				return null;
			}
		}

		#endregion

		#region METHODS

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

			m_atoms.Add( _atom );
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

			m_atoms.Insert( (int) _index, _atom );
		}

			// /** Create a `MTMathList` given a list of atoms. The list of atoms should be
			//  terminated by `nil`.
			//  */
			// + (instancetype) mathListWithAtoms:(MTMathAtom*) firstAtom, ... NS_REQUIRES_NIL_TERMINATION;
			// 
			// /** Create a `MTMathList` given a list of atoms. */
			// + (instancetype) mathListWithAtomsArray:(NSArray<MTMathAtom*>*) atoms;
			// 
			// /// A list of MathAtoms
			// @property (nonatomic, readonly) NSArray<__kindof MTMathAtom*>* atoms;
			// 
			// /** Initializes an empty math list. */
			// - (instancetype) init NS_DESIGNATED_INITIALIZER;
			// 
			// /** Add an atom to the end of the list.
			//  @param atom The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
			//  @throws NSException if the atom is of type `kMTMathAtomBoundary`
			//  @throws NSInvalidArgumentException if the atom is `nil` */
			// - (void) addAtom:(MTMathAtom*) atom;
			// 
			// /** Inserts an atom at the given index. If index is already occupied, the objects at index and beyond are 
			//  shifted by adding 1 to their indices to make room.
			//  
			//  @param atom The atom to be inserted. This cannot be `nil` and cannot have the type `kMTMathAtomBoundary`.
			//  @param index The index where the atom is to be inserted. The index should be less than or equal to the
			//  number of elements in the math list.
			//  @throws NSException if the atom is of type kMTMathAtomBoundary
			//  @throws NSInvalidArgumentException if the atom is nil
			//  @throws NSRangeException if the index is greater than the number of atoms in the math list. */
			// - (void) insertAtom:(MTMathAtom *)atom atIndex:(NSUInteger) index;
			// 
			// /** Append the given list to the end of the current list.
			//  @param list The list to append.
			//  */
			// - (void) append:(MTMathList*) list;
			// 
			// /** Removes the last atom from the math list. If there are no atoms in the list this does nothing. */
			// - (void) removeLastAtom;
			// 
			// /** Removes the atom at the given index.
			//  @param index The index at which to remove the atom. Must be less than the number of atoms
			//  in the list.
			//  */
			// - (void) removeAtomAtIndex:(NSUInteger) index;
			// 
			// /** Removes all the atoms within the given range. */
			// - (void) removeAtomsInRange:(NSRange) range;
			// 
			// /// converts the MTMathList to a string form. Note: This is not the LaTeX form.
			// @property (nonatomic, readonly) NSString *stringValue;
			// 
			// /// Create a new math list as a final expression and update atoms
			// /// by combining like atoms that occur together and converting unary operators to binary operators.
			// /// This function does not modify the current MTMathList
			// - (MTMathList*) finalized;
			// 
			// /// Makes a deep copy of the list
			// - (id)copyWithZone:(nullable NSZone *)zone;


		#endregion
    }
}
