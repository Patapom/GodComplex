﻿//////////////////////////////////////////////////////////////////////////
// Implements the graph class
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
	/// Graph class hosting neurons
	/// </summary>
	public class Graph {

		#region FIELDS

		// The list of neurons in the graph
		List< Neuron >		m_neurons = new List<Neuron>();

		// The mapping from neuron name to neurons
		// Neurons can have the same name as long as the context is different (e.g. "Physics.Volume" and "Sound.Volume")
		Dictionary< string, List< Neuron > >	m_name2Neurons = new Dictionary<string, List<Neuron>>();

		// Dictionary of aliases
		Dictionary< string, Neuron[] >	m_aliasName2Neurons = new Dictionary<string, Neuron[]>();

		// The compulsory root neuron
		Neuron		m_root = null;

		#endregion

		#region PROPERTIES

		public int			NeuronsCount		{ get { return m_neurons.Count; } }
		public Neuron[]		Neurons				{ get { return m_neurons.ToArray(); } }
		public Neuron		this[int _index]	{ get { return m_neurons[_index]; } }
		public Neuron		this[string _name] {
			get {
				if ( !m_name2Neurons.ContainsKey( _name ) )
					return null;
				List<Neuron>	neurons = m_name2Neurons[_name];
				if ( neurons.Count > 1 )
					throw new Exception( "Ambiguous neuron name! Need to specify context..." );

				return neurons[0];
			}
		}

		public Neuron		Root { get { return m_root; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Registers the unique root neuron for the graph
		/// Throws an exception if the root is already assigned
		/// </summary>
		/// <param name="_neuronName"></param>
		/// <returns></returns>
		internal void	RegisterRoot( Neuron _rootNeuron ) {
			if ( m_root != null )
				throw new Exception( "Root already exists and used by neuron " + m_root );

			m_root = _rootNeuron;
		}

		/// <summary>
		/// Registers one or several neurons, resolving the provided hierarchy
		/// Throws an exception if the neuron has the same name as an existing alias
		/// </summary>
		/// <param name="_neuronName"></param>
		internal Neuron	RegisterNeuron( string _neuronName ) {
			Neuron	N = new Neuron( _neuronName );
			m_neurons.Add( N );

			if ( _neuronName == null )
				return N;

			if ( m_aliasName2Neurons.ContainsKey( _neuronName ) )
				throw new Exception( "Neuron name \"" + _neuronName + "\" cannot be used because if shadows an existing alias with the same name!" );	// If we allowed that, then we could never create any other homonym neuron

			if ( !m_name2Neurons.ContainsKey( _neuronName ) )
				m_name2Neurons.Add( _neuronName, new List<Neuron>() );
			m_name2Neurons[_neuronName].Add( N );

			return N;
		}

		/// <summary>
		/// Finds neurons by name
		/// </summary>
		/// <param name="_neuronName"></param>
		/// <returns></returns>
		public Neuron[]	FindNeurons( string _neuronName ) {
			if ( !m_name2Neurons.ContainsKey( _neuronName ) )
				return null;

			return m_name2Neurons[_neuronName].ToArray();
		}

		/// <summary>
		/// Finds a single neuron by its fully-qualified name (e.g. "a.b.c")
		/// </summary>
		/// <param name="_fullyQualifiedName"></param>
		/// <returns></returns>
		public Neuron	FindNeuron( string _fullyQualifiedName ) {
			return FindNeuron( _fullyQualifiedName, null );
		}
		public Neuron	FindNeuron( string _fullyQualifiedName, Neuron _context ) {
			if ( _fullyQualifiedName == null || _fullyQualifiedName == "" )
				return null;
			if ( _context == null )
				_context = m_root;

			Parser.Name	fullName = Parser.Name.Read( _fullyQualifiedName );

			// 1] Find the root anchor of the fully-qualified name
			//	• Can be an alias
			//	• Can exist in the current context
			//	• Can exist in the global context
			//
			Neuron	anchor = null;
			int		nameIndex = 0;
			string	rootName = fullName[nameIndex++];
			if ( rootName == "" ) {
				if ( nameIndex == fullName.Length )
					return null;
				rootName = fullName[nameIndex++];
			}

			// Try alias first
			Neuron[]	alias = FindAlias( rootName );
			if ( alias != null ) {
				if ( alias.Length != 1 )
					throw new Exception( "Alias \"" + rootName + "\" points to multiple neurons and cannot be resolved as a single neuron to follow the qualified name!" );

				anchor = alias[0];
			} else {
				// Try context neurons
				if ( _context != null )
					anchor = _context.FindChild( rootName );

				if ( anchor == null ) {
					// Try root
					anchor = _context != m_root ? m_root.FindChild( rootName ) : null;
					if ( anchor == null ) {
						// Try everything
						Neuron[]	children = FindNeurons( rootName );
						if ( children == null )
							return null;
						if ( children.Length == 0 )
							throw new Exception( "Failed to find root name \"" + rootName + "\" of fully-qualified neuron name \"" + fullName + "\" in any context!" );
						if ( children.Length != 1 )
							throw new Exception( "Root name \"" + rootName + "\" of fully-qualified neuron name \"" + fullName + "\" is ambiguous in the global context: could be (" + string.Join<Neuron>( ") or (", children ) + ")" );

						anchor = children[0];
					}
				}
			}

			// 2] Follow/Create the rest of the hierarchy
			Neuron	parent = anchor;
			for ( ; nameIndex < fullName.Length; nameIndex++ ) {
				string	name = fullName[nameIndex];

				// Find neuron in anchor
				Neuron	child = parent.FindChild( name );
				if ( child == null )
					throw new Exception( "Child name \"" + name + "\" of fully-qualified neuron name \"" + fullName + "\" not found in parent context \"" + parent + "\"!" );

				parent = child;
			}

			return parent;
		}

		/// <summary>
		/// Register an alias to a collection of neurons
		/// Throws an exception if the alias has the same name as an existing neuron
		/// </summary>
		/// <param name="_aliasName"></param>
		/// <param name="_neurons"></param>
		internal void	RegisterAlias( string _aliasName, Neuron[] _neurons ) {
			if ( m_name2Neurons.ContainsKey( _aliasName ) )
				throw new Exception( "Alias name \"" + _aliasName + "\" cannot be used because it shadows an existing neuron \"" + m_name2Neurons[_aliasName][0].FullName + "\" with the same name!" );	// If we allowed that, then we could never create any other homonym neuron

			if ( !m_aliasName2Neurons.ContainsKey( _aliasName ) )
				m_aliasName2Neurons.Add( _aliasName, _neurons );
			else
				m_aliasName2Neurons[_aliasName] = _neurons;
		}

		public Neuron[]	FindAlias( string _aliasName ) {
			if ( m_aliasName2Neurons.ContainsKey( _aliasName ) )
				return m_aliasName2Neurons[_aliasName];
			else
				return null;
		}

		#region I/O

		public void		Write( BinaryWriter _W ) {
			_W.Write( m_neurons.Count );

			Dictionary< Neuron, int >	neuron2ID = new Dictionary<Neuron, int>();
			int	ID = 0;
			foreach ( Neuron N in m_neurons ) {
				neuron2ID.Add( N, ID++ );
			}

			foreach ( Neuron N in m_neurons ) {
				N.Write( _W, neuron2ID );
			}
		}

		public void		Read( BinaryReader _R ) {
			// Pre-create neurons so we can directly reference them when we read back their IDs
			m_neurons = new List<Neuron>( _R.ReadInt32() );
			m_name2Neurons.Clear();
			for ( int i=0; i < m_neurons.Capacity; i++ ) {
				m_neurons.Add( new Neuron() );
			}

			foreach ( Neuron N in m_neurons ) {
				N.Read( _R, m_neurons );

				if ( N.m_name != null ) {
					if ( !m_name2Neurons.ContainsKey( N.m_name ) )
						m_name2Neurons.Add( N.m_name, new List<Neuron>() );
					m_name2Neurons[N.m_name].Add( N );
				}
			}
		}

		#endregion

		#endregion
	}
}
