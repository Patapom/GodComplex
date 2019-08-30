//////////////////////////////////////////////////////////////////////////
// Implements the prototype/neuron class
// 
// The prototype/neuron class is the only class needed for the structure to work.
// It's very generic so it can encode any type of relationship and grouping, as well as describe any kind of concept, abstract or real.
//
// Basically, a neuron is a *unique concept*, it can serve to define a notion, refine a notion, group some concepts together, attach a concept to another, etc.
// The concept exists because the neuron exists, there is no notion of value, importance, comparison, metric at this level.
// All these notions are concepts of their own, also defined in terms of prototype neurons.
// For example, there is no need to introduce comparison operators like "smaller" or "larger", they are themselves introduced
//	as neurons because these concepts have been created due to the fact they have been deemed important, these neurons can
//	be used later to characterize the relationships between other concepts.

// A neuron contains:
//	• A name, which is unique in its particular context.
//		The "context" (or namespace) of a neuron is the concatenation of the neuron along the shortest path to the root
//		The important thing is that 2 identically-named neurons must *always* describe 2 different concepts
//			(i.e. each concept must be completely unique).
//		e.g. You can have 2 neurons named "Volume", one in the context of physics where it determines the volume of an object,
//				and one in the context of sound where it means the amplitude of a sound.
//
//	• A list of parents, so a neuron can belong to multiple groups/contexts
//		e.g. A neuron can be a person, but also be a "political person", which is a more specific group
//
//	• A list of children, so a neuron can be the root of many notions
//		e.g. A neuron "color" can have many instances of colors, like "red", "green", "yellow", etc.
//
//	• A list of features, so a neuron is characterized by having some specific traits
//		e.g. A prototype "bird" neuron can have a "beak" neuron, a "wing" neuron, a "feather" neuron, etc.
// 
//	• A value, which is an optional data attached to a particular neuron
// 
//////////////////////////////////////////////////////////////////////////
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoParser
{
	/// <summary>
	/// Base neuron class
	/// </summary>
	[System.Diagnostics.DebuggerDisplay( "{FullName} - P {ParentsCount,d} - C {ChildrenCount,d} - F {FeaturesCount,d}" )]
	public class Neuron {
		public string				m_name = "";
		private List< Neuron >		m_parents = new List<Neuron>();
		private List< Neuron >		m_children = new List<Neuron>();
		private List< Neuron >		m_features = new List<Neuron>();
		public NeuronValueBase		m_value = null;

		private static uint			ms_searchMarker = 0;
		private uint				m_searchMarker = 0;

		public int			ParentsCount	{ get { return m_parents.Count; } }
		public Neuron[]		Parents			{ get {return m_parents.ToArray(); } }
		public int			ChildrenCount	{ get { return m_children.Count; } }
		public Neuron[]		Children		{ get {return m_children.ToArray(); } }
		public int			FeaturesCount	{ get { return m_features.Count; } }
		public Neuron[]		Features		{ get {return m_features.ToArray(); } }

		public int			Distance2Root	{ get {
			if ( m_parents.Count == 0 )
				return 0;

			int	minDistance = int.MaxValue;
			foreach ( Neuron P in m_parents ) {
				if ( P == this )
					return 0;	// We're the root
				minDistance = Math.Min( minDistance, 1+P.Distance2Root );
			}

			return minDistance;
		} }

		internal Neuron() {}
		public Neuron( string _name ) {
			m_name = _name;
		}
		public override string ToString() { return FullName; }

		public void				LinkParent( Neuron _parent ) {
			if ( m_parents.Contains( _parent ) )
				return;

			if ( _parent == this )
				throw new Exception( "Can't link self!" );

			m_parents.Add( _parent );
			_parent.m_children.Add( this );
		}
		public void				LinkChild( Neuron _child ) {
			if ( m_children.Contains( _child ) )
				return;

			if ( _child == this )
				throw new Exception( "Can't link self!" );

			m_children.Add( _child );
			_child.m_parents.Add( this );
		}
		public void				LinkFeature( Neuron _feature ) {
			if ( m_features.Contains( _feature ) )
				return;

			if ( _feature == this )
				throw new Exception( "Can't link self!" );

			m_features.Add( _feature );
			_feature.m_parents.Add( this );
		}

		public void				LinkParents( Neuron[] _parents ) {
			foreach ( Neuron parent in _parents )
				LinkParent( parent );
		}
		public void				LinkChildren( Neuron[] _children ) {
			foreach ( Neuron child in _children )
				LinkChild( child );
		}
		public void				LinkFeatures( Neuron[] _features ) {
			foreach ( Neuron feature in _features )
				LinkFeature( feature );
		}

		public void				RemoveParent( Neuron _parent ) {
			m_parents.Remove( _parent );
			_parent.m_children.Remove( this );
		}
		public void				RemoveChild( Neuron _child ) {
			m_children.Remove( _child );
			_child.m_parents.Remove( this );
		}
		public void				RemoveFeature( Neuron _feature ) {
			m_features.Remove( _feature );
			_feature.m_parents.Remove( this );
		}

		public string	FullName {
			get {
				Neuron	parent = this;
				Neuron	child = null;
				string	result = "";
				int		count = 0;
				while ( parent != null && child != parent && count < 8 ) {
					result = (parent.m_name != null ? (parent.m_name.IndexOf( ' ' ) != -1 ? ("\""+parent.m_name+"\"") : parent.m_name) : "<anon>") + (child != null ? "." + result : "");
					child = parent;
					parent = child.m_parents.Count > 0 ? child.m_parents[0] : null;
					count++;
				}
				if ( count == 8 && parent != null && parent != child ) {
					result = "(...)" + result;
				}
				return result;
			}
		}

		public Neuron			FindParent( string _parentName ) {
			foreach ( Neuron parent in m_parents )
				if ( parent.m_name == _parentName )
					return parent;

			return null;
		}

		public Neuron			FindChild( string _childName ) {
			foreach ( Neuron child in m_children ) {
				if ( child.m_name != null ) {
					if ( child.m_name == _childName )
						return child;
				} else {
					// Anonymous node, search named parent
					Neuron	namedParent = child.FindParent( _childName );
					if ( namedParent != null )
						return child;
				}
			}

			return null;
		}

		public Neuron			FindFeature( string _featureName ) {
			foreach ( Neuron feature in m_features ) {
				if ( feature.m_name != null ) {
					if ( feature.m_name == _featureName )
						return feature;
				} else {
					// Anonymous node, search named parent
					Neuron	namedParent = feature.FindParent( _featureName );
					if ( namedParent != null )
						return feature;
				}
			}

			return null;
		}

		/// <summary>
		/// Tries and find the provided neuron in the child hierarchy of this neuron
		/// </summary>
		/// <param name="_child"></param>
		/// <returns></returns>
		public Neuron			RecursiveFindChild( Neuron _child ) {
			return RecursiveFindChild( _child, ++ms_searchMarker );
		}
		Neuron			RecursiveFindChild( Neuron _child, uint _searchMarker ) {
			if ( m_searchMarker == _searchMarker )
				return null;	// Already visited

			m_searchMarker = _searchMarker;	// Now visited

			// Test all children first
			foreach ( Neuron child in m_children ) {
				if ( child == _child )
					return child;
			}

			// Recurse through children
			foreach ( Neuron child in m_children ) {
				Neuron	result = child.RecursiveFindChild( _child, _searchMarker );
				if ( result != null )
					return result;
			}

			return null;
		}

		/// <summary>
		/// Tries and find the provided neuron name in the parent hierarchy of this neuron
		/// </summary>
		/// <param name="_parent"></param>
		/// <returns></returns>
		public Neuron			RecursiveFindParent( Neuron _parent ) {
			return RecursiveFindParent( _parent, ++ms_searchMarker );
		}
		Neuron			RecursiveFindParent( Neuron _parent, uint _searchMarker ) {
			if ( m_searchMarker == _searchMarker )
				return null;	// Already visited

			m_searchMarker = _searchMarker;	// Now visited

			// Test all parents first
			foreach ( Neuron parent in m_parents ) {
				if ( parent == _parent )
					return parent;
			}

			// Recurse through parents
			foreach ( Neuron parent in m_parents ) {
				Neuron	result = parent.RecursiveFindParent( _parent, _searchMarker );
				if ( result != null )
					return result;
			}

			return null;
		}

		/// <summary>
		/// Tries and find the provided neuron name in the parent hierarchy of this neuron
		/// </summary>
		/// <param name="_parentName"></param>
		/// <returns></returns>
		public Neuron			RecursiveFindParentByName( string _parentName ) {
			return RecursiveFindParentByName( _parentName, ++ms_searchMarker );
		}
		Neuron			RecursiveFindParentByName( string _parentName, uint _searchMarker ) {
			if ( m_searchMarker == _searchMarker )
				return null;	// Already visited

			m_searchMarker = _searchMarker;	// Now visited

			// Test all parents first
			foreach ( Neuron parent in m_parents ) {
				if ( parent.m_name == _parentName )
					return parent;
			}

			// Recurse through parents
			foreach ( Neuron parent in m_parents ) {
				Neuron	result = parent.RecursiveFindParentByName( _parentName, _searchMarker );
				if ( result != null )
					return result;
			}

			return null;
		}

		#region I/O

		public void		Write( BinaryWriter _W, Dictionary< Neuron, int > _neuron2ID ) {
			if ( m_name != null ) {
				_W.Write( true );
				_W.Write( m_name );
			} else {
				_W.Write( false );
			}

			_W.Write( m_parents.Count );
			foreach ( Neuron N in m_parents )
				_W.Write( _neuron2ID[N] );

			_W.Write( m_children.Count );
			foreach ( Neuron N in m_children )
				_W.Write( _neuron2ID[N] );

			_W.Write( m_features.Count );
			foreach ( Neuron N in m_features )
				_W.Write( _neuron2ID[N] );

			if ( m_value != null ) {
				if ( m_value is NeuronValue )
					_W.Write( 2 );
				else if ( m_value is NeuronValueBase )
					_W.Write( 1 );
				else
					throw new Exception( "Unsupported value type!" );

				m_value.Write( _W );
			} else {
				_W.Write( 0 );
			}
		}

		public void		Read( BinaryReader _R, List< Neuron > _neurons ) {
			if ( _R.ReadBoolean() ) {
				m_name = _R.ReadString();
			} else {
				m_name = null;	// Anonymous neuron
			}

			m_parents = new List<Neuron>( _R.ReadInt32() );
			for ( int i=0; i < m_parents.Capacity; i++ ) {
				m_parents.Add( _neurons[_R.ReadInt32()] );
			}

			m_children = new List<Neuron>( _R.ReadInt32() );
			for ( int i=0; i < m_children.Capacity; i++ ) {
				m_children.Add( _neurons[_R.ReadInt32()] );
			}

			m_features = new List<Neuron>( _R.ReadInt32() );
			for ( int i=0; i < m_features.Capacity; i++ ) {
				m_features.Add( _neurons[_R.ReadInt32()] );
			}

			int	valueType = _R.ReadInt32();
			switch ( valueType ) {
				case 0:	m_value = null; break;
				case 1:	m_value = new NeuronValueBase(); break;
				case 2:	m_value = new NeuronValue(); break;
				default: throw new Exception( "Unsupported value type!" );
			}
			if ( m_value != null )
				m_value.Read( _R );
		}

		#endregion
	}

	public class NeuronValueBase {

		#region I/O

		public virtual void		Write( BinaryWriter _W ) {
		}

		public virtual void		Read( BinaryReader _R ) {
		}

		#endregion
	}
	[System.Diagnostics.DebuggerDisplay( "[{m_valueMean}]" )]
	public class NeuronValue : NeuronValueBase {
		public string		m_valueMean = null;				// The mean value
		public string		m_valueStdDeviation = null;		// The standard deviation
		public string		m_units = null;					// The units

		#region I/O

		void	WriteString( BinaryWriter _W, string _string ) {
			if ( _string != null ) {
				_W.Write( true );
				_W.Write( _string );
			} else {
				_W.Write( false );
			}
		}

		string	ReadString( BinaryReader _R ) {
			return _R.ReadBoolean() ? _R.ReadString() : null;
		}

		public override void		Write( BinaryWriter _W ) {
			WriteString( _W, m_valueMean );
			WriteString( _W, m_valueStdDeviation );
			WriteString( _W, m_units );
		}

		public override void		Read( BinaryReader _R ) {
			m_valueMean = ReadString( _R );
			m_valueStdDeviation = ReadString( _R );
			m_units = ReadString( _R );
		}

		#endregion
	}
}
