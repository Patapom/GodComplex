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
	[System.Diagnostics.DebuggerDisplay( "{HelpString()} - P {ParentsCount,d} - C {ChildrenCount,d} - F {FeaturesCount,d}" )]
	public class Neuron {
		public string				m_name = "";
		private List< Neuron >		m_parents = new List<Neuron>();
		private List< Neuron >		m_children = new List<Neuron>();
		private List< Neuron >		m_features = new List<Neuron>();
		public NeuronValue			m_value = null;

		public int			ParentsCount	{ get { return m_parents.Count; } }
		public Neuron[]		Parents			{ get { return m_parents.ToArray(); } }
		public int			ChildrenCount	{ get { return m_children.Count; } }
		public Neuron[]		Children		{ get { return m_children.ToArray(); } }
		public int			FeaturesCount	{ get { return m_features.Count; } }
		public Neuron[]		Features		{ get {return m_features.ToArray(); } }

		public int			Distance2Root	{ get {
			int	minDistance = int.MaxValue;
			foreach ( Neuron P in m_parents ) {
				if ( P == this )
					return 0;	// We're the root
				minDistance = Math.Min( minDistance, P.Distance2Root );
			}

			return minDistance;
		} }

		internal Neuron() {}
		public Neuron( string _name ) {
			m_name = _name;
		}
		public override string ToString() { return m_name; }

		public void				LinkParent( Neuron _parent ) {
			m_parents.Add( _parent );
			_parent.m_children.Add( this );
		}
		public void				LinkChild( Neuron _child ) {
			m_children.Add( _child );
			_child.m_parents.Add( this );
		}
		public void				LinkFeature( Neuron _feature ) {
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

		public string	HelpString() {
			Neuron	parent = this;
			Neuron	child = null;
			string	result = "";
			int		count = 0;
			while ( parent != null && child != parent && count < 8 ) {
				result = parent + (child != null ? "." + result : "");
				child = parent;
				parent = child.m_parents.Count > 0 ? child.m_parents[0] : null;
				count++;
			}
			if ( count == 8 && parent != null && parent != child ) {
				result = "..." + result;
			}
			return result;
		}

		public Neuron			FindParent( string _parentName ) {
			foreach ( Neuron parent in m_parents )
				if ( parent.m_name == _parentName )
					return parent;

			return null;
		}

		public Neuron			FindChild( string _childName ) {
			foreach ( Neuron child in m_children )
				if ( child.m_name == _childName )
					return child;

			return null;
		}

		public Neuron			FindFeature( string _featureName ) {
			foreach ( Neuron feature in m_features )
				if ( feature.m_name == _featureName )
					return feature;

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
				_W.Write( true );
				m_value.Write( _W );
			} else {
				_W.Write( false );
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

			if ( _R.ReadBoolean() ) {
				m_value = new NeuronValue();
				m_value.Read( _R );
			} else {
				m_value = null;
			}
		}

		#endregion
	}

	[System.Diagnostics.DebuggerDisplay( "[{m_value}]" )]
	public class NeuronValue {
		public string		m_value = null;

		#region I/O

		public void		Write( BinaryWriter _W ) {
			if ( m_value != null ) {
				_W.Write( true );
				_W.Write( m_value );
			} else {
				_W.Write( false );
			}
		}

		public void		Read( BinaryReader _R ) {
			if ( _R.ReadBoolean() ) {
				m_value = _R.ReadString();
			} else {
				m_value = null;
			}
		}

		#endregion
	}
}
