// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/xorandor
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{

	#region TYPES

	[System.Diagnostics.DebuggerDisplay( "{m_type} ({m_leftX}-{m_rightX}, {m_Y})" )]
	class Node {
		public enum TYPE {
			LED,
			NOT, AND, OR, XOR, NAND, NOR, XNOR,
			SWITCH,
			INPUT,
		}

		public TYPE m_type;
		public bool	m_defaultState;			// For switches: false = <, true = >
		public int  m_Y;					// Line where the node was found
		public int  m_leftX, m_rightX;		// Left/right coordinates the node is covering

		// Current state
		public bool	m_state = false;

		// Multiple possible inputs for LED's
		// (I have to say I don't like this: no LED has more than 1 input! We should use a bunch of AND nodes instead of this)
		public List< Node >	m_LEDInputs = new List< Node >();

		// 2 possible inputs (for regular nodes)
		public Node m_inputLeft = null;
		public Node m_inputRight = null;
		public int	m_inputLeftX = 0;
		public int	m_inputRightX = 0;

		// 2 possible outputs (for switches)
		public Node m_outputLeft = null;
		public Node m_outputRight = null;
		public int	m_outputLeftX = 0;
		public int	m_outputRightX = 0;

		public override string ToString() {
			return m_type + " (" + m_leftX + "-" + m_rightX + ", " + m_Y + ")";
		}

		/// <summary>
		/// Estimates the node's output state
		/// </summary>
		public bool		Estimate( Node _caller ) {
			switch ( m_type ) {
				case TYPE.LED:
					foreach ( Node I in m_LEDInputs ) {
						if ( !I.Estimate( this ) )
							return false;	// All inputs must be lit!
					}
					return true;

				case TYPE.NOT:
					if ( m_inputRight != null ) throw new Exception( "Unexpected second input!" );
					return !m_inputLeft.Estimate( this );
				case TYPE.AND: return m_inputLeft.Estimate( this ) & m_inputRight.Estimate( this );
				case TYPE.OR: return m_inputLeft.Estimate( this ) | m_inputRight.Estimate( this );
				case TYPE.XOR: return m_inputLeft.Estimate( this ) ^ m_inputRight.Estimate( this );
				case TYPE.NAND: return !(m_inputLeft.Estimate( this ) & m_inputRight.Estimate( this ));
				case TYPE.NOR: return !(m_inputLeft.Estimate( this ) | m_inputRight.Estimate( this ));
				case TYPE.XNOR: return !(m_inputLeft.Estimate( this ) ^ m_inputRight.Estimate( this ));

				case TYPE.INPUT: return m_defaultState ^ m_state;
				case TYPE.SWITCH: {
					if ( m_inputRight != null ) throw new Exception( "Unexpected second input!" );
					if ( _caller != m_outputLeft && _caller != m_outputRight ) throw new Exception( "Unexpected caller: should be either left or right output!" );

					bool	inputState = m_inputLeft.Estimate( this );
					bool	forwardToRight = m_defaultState ^ m_state;
					if ( forwardToRight ) {
						return _caller == m_outputRight ? inputState : false;
					} else {
						return _caller == m_outputLeft ? inputState : false;
					}
				}

				default:
					throw new Exception( "Unsupported node type!" );
			}
		}

		// Connects this node to all of its inputs by following the wires
		public void Connect( List< Node > _nodes, List< Wire > _wires ) {
			if ( m_type == TYPE.INPUT )
				return;	// No connection required for inputs...

			// We start by finding the wires connecting to our ass
			foreach ( Wire W in _wires ) {
				if ( W.m_Y != m_Y+1 || !W.IsVertical )
					continue;	// Not a proper connection: we're looking for vertical wires starting right below the node

				int wireX = W.m_leftX;
				if ( wireX >= m_leftX && wireX <= m_rightX ) {
					W.Connect( wireX, this, _nodes, _wires );
				}
			}

			if ( m_inputLeft == null && m_LEDInputs.Count == 0 )
				throw new Exception( "Failed to find any connection!" );
		}

		// Store as left or right input connection
		public void		ConnectInput( int _X, Node _inputNode ) {
			if ( m_type == TYPE.LED ) {
				// Unfortunately, LED's are special and have multiple inputs...
				m_LEDInputs.Add( _inputNode );
				return;
			}

			if ( m_inputLeft == null ) {
				m_inputLeft = _inputNode;
				m_inputLeftX = _X;
			} else if ( m_inputRight == null ) {
				m_inputRight = _inputNode;
				m_inputRightX = _X;
			} else {
				throw new Exception( "All connections are already made!" );
			}
		}

		// Store as left or right output connection
		public void		ConnectOutput( int _X, Node _outputNode ) {
			if ( m_outputLeft == null ) {
				m_outputLeft = _outputNode;
				m_outputLeftX = _X;
			} else if ( m_outputRight == null ) {
				m_outputRight = _outputNode;
				m_outputRightX = _X;
			} else {
				throw new Exception( "All connections are already made!" );
			}
		}

		// Order left/right input and output connections
		public void		OrderConnections() {
			if ( m_inputRight != null && m_inputRightX < m_inputLeftX ) {
				// Swap inputs
				Node	tempNode = m_inputLeft;
				m_inputLeft = m_inputRight;
				m_inputRight = tempNode;
				int	temp = m_inputLeftX;
				m_inputLeftX = m_inputRightX;
				m_inputRightX = temp;
			}

			if ( m_outputRight != null && m_outputRightX < m_outputLeftX ) {
				// Swap inputs
				Node	tempNode = m_outputLeft;
				m_outputLeft = m_outputRight;
				m_outputRight = tempNode;
				int	temp = m_outputLeftX;
				m_outputLeftX = m_outputRightX;
				m_outputRightX = temp;
			}
		}
	}

	[System.Diagnostics.DebuggerDisplay( "({m_leftX}-{m_rightX}, {m_Y}) Vertical={IsVertical?\"Yes\":\"No\"}" )]
	class Wire {
		public int  m_Y;
		public int  m_leftX, m_rightX;		// Left/right coordinates the wire is covering

		public bool		IsVertical  { get { return m_leftX == m_rightX; } }

		// Build a new wire
		public Wire( int _X, int _Y ) {
			m_Y = _Y;
			m_leftX = m_rightX = _X;
		}

		// Connects the wire to either other wires or target nodes
		public void     Connect( int _targetX, Node _targetNode, List< Node > _nodes, List< Wire > _wires ) {
			// Looking for wires starting right below the wire
			foreach ( Wire W in _wires ) {
				if ( W.m_Y != m_Y+1 )
					continue;

				if ( W.m_leftX <= m_rightX && W.m_rightX >= m_leftX )
					W.Connect( _targetX, _targetNode, _nodes, _wires );  // Continue connection
			}

			// Looking for target nodes starting right below the wire
			foreach ( Node N in _nodes ) {
				if ( N.m_Y != m_Y+1 )
					continue;

				if ( N.m_leftX <= m_rightX && N.m_rightX >= m_leftX ) {
					// Make a new connection between nodes
					_targetNode.ConnectInput( _targetX, N );
					N.ConnectOutput( m_leftX, _targetNode );

Console.Error.WriteLine( "Connected node " + N + " => " + _targetNode  );

					break;
				}
			}
		}
	}

	#endregion

	public static void Meuh(string[] args) {

		//////////////////////////////////////////////////////////////////////////
		// Parse input graph and gather nodes (gates + switches + inputs) and wires
		string	line = Console.ReadLine();
Console.Error.WriteLine( line );
		string[] inputText = line.Split(' ');
		int height = int.Parse(inputText[0]);
		int width = int.Parse(inputText[1]);

		List< Node >	nodes = new List< Node >();
		List< Node >	inputs = new List< Node >();
		List< Node >	switches = new List< Node >();
		List< Wire >	wires = new List< Wire >();

		Node	currentNode = null;
		Wire	currentWire = null;
		for (int Y = 0; Y < height-1; Y++) {
			line = Console.ReadLine();
Console.Error.WriteLine( line );

			if ( currentNode != null || currentWire != null )
				throw new Exception( "There is current node or wire that wasn't closed!" );

			for ( int X=0; X < width; X++ ) {
				switch ( line[X] ) {
					case '[':	// Start a new node
						currentNode = new Node() { m_Y = Y, m_leftX = X };
						nodes.Add( currentNode );
						break;
					case ']':	// Close the existing node
						if ( currentNode == null )
							throw new Exception( "No currently open node to close!" );
						currentNode.m_rightX = X;
						currentNode = null;
						break;

					case '@': currentNode.m_type = Node.TYPE.LED; break;
					case '~': currentNode.m_type = Node.TYPE.NOT; break;
					case '&': currentNode.m_type = Node.TYPE.AND; break;
					case '^': currentNode.m_type = Node.TYPE.NAND; break;
					case '=': currentNode.m_type = Node.TYPE.XNOR; break;
					case '<': currentNode.m_type = Node.TYPE.SWITCH; currentNode.m_defaultState = false; switches.Add( currentNode ); break;
					case '>': currentNode.m_type = Node.TYPE.SWITCH; currentNode.m_defaultState = true; switches.Add( currentNode ); break;
					case '|':
						if ( currentNode != null ) currentNode.m_type = Node.TYPE.OR;
						else wires.Add( new Wire( X, Y ) );   // Deal with a new vertical wire
						break;
					case '+':
						if ( currentNode != null ) currentNode.m_type = Node.TYPE.XOR;
						else {
							// Deal with horizontal wires
							if ( currentWire == null ) {
								currentWire = new Wire( X, Y ); // Start of a new wire
								wires.Add( currentWire );
							} else {
								currentWire.m_rightX = X;       // Or expand the existing one
							}
						}
						break;
					case '-':
						if ( currentNode != null ) currentNode.m_type = Node.TYPE.NOR;
						else if ( currentWire == null ) throw new Exception( "No current node or wire to handle the character!" );
						break;

					case ' ':
						currentWire = null; // End any current wire
						break;

					default:
						throw new Exception( "Unsupported character!" );
				}
			}
		}

		// Read last line of inputs
		string inputLine = Console.ReadLine();
Console.Error.WriteLine( inputLine );

		for ( int X=0; X < width; X++ ) {
			if ( inputLine[X] != ' ' ) {
				bool	startState = false;
				if ( inputLine[X] == '0' )
					startState = false;
				else if ( inputLine[X] == '1' )
					startState = true;
				else
					throw new Exception( "Unexpected character!" );

				Node    input = new Node() {
					m_type = Node.TYPE.INPUT,
					m_Y = height-1, m_leftX = X, m_rightX = X,
					m_defaultState = startState
				};
				nodes.Add( input );
				inputs.Add( input );
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Build connections between nodes and order them if required
		// (although, since we parsed them from top to bottom and left to right, the order should already be correct)
		foreach ( Node N in nodes )
			N.Connect( nodes, wires );
		foreach ( Node N in nodes )
			N.OrderConnections();

		//////////////////////////////////////////////////////////////////////////
		// Solve the puzzle for all possible input and switch values
		int		totalVariablesCount = inputs.Count + switches.Count;
		int		solutionsCount = 1 << totalVariablesCount;	// Total amount of possibilities we need to try

		string	leastSteps = "";
		int		leastStepsCount = int.MaxValue;
		for ( int solutionIndex=0; solutionIndex < solutionsCount; solutionIndex++ ) {
			// Prepare initial states
			string	steps = "";
			int		stepsCount = 0;
			int		variableIndex = 1;

			for ( int switchIndex=0; switchIndex < switches.Count; switchIndex++ ) {
				Node	K = switches[switchIndex];
				if ( (solutionIndex & variableIndex) != 0 ) {
					K.m_state = true;
					steps += " K" + (1+switchIndex);
					stepsCount++;
				} else {
					K.m_state = false;
				}
				variableIndex <<= 1;
			}
			for ( int inputIndex=0; inputIndex < inputs.Count; inputIndex++ ) {
				Node	I = inputs[inputIndex];
				if ( (solutionIndex & variableIndex) != 0 ) {
					I.m_state = true;
					steps += " I" + (1+inputIndex);
					stepsCount++;
				} else {
					I.m_state = false;
				}
				variableIndex <<= 1;
			}

			// Estimate LED's state
			if ( nodes[0].Estimate( null ) ) {
				// This state lights the LED!
				if ( stepsCount < leastStepsCount ) {
					// And it does it in better time than existing solution!
					leastStepsCount = stepsCount;
					leastSteps = steps;
				}
			}
		}

		// We found a state that lights the LED up, write each step in order...
		string[]	splitSteps = leastSteps.Split( ' ' );
		for ( int stepIndex=1; stepIndex < splitSteps.Length; stepIndex++ ) {	// We start at 1 because the first step is always empty
			Console.WriteLine( splitSteps[stepIndex] );
		}
	}
}
