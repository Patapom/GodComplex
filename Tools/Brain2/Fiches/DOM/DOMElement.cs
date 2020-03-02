using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;

namespace Brain2 {

	/// <summary>
	/// A DOM Element consists in a type and a normalized rectangle, as well as child elements
	/// </summary>
	public class	DOMElement {

		#region NESTED TYPES

		public enum		ELEMENT_TYPE {
			ROOT,
			TEXT,
			LINK,
			IMAGE,
			UNKNOWN,
		}

		#endregion

		#region FIELDS

		/// <summary>
		/// The path within the HTML document
		/// The path has the form "0.1.2.3.4.5...." where each number indicates the index of the child to fetch at any given level
		/// </summary>
		public string		m_path = "";
		public ELEMENT_TYPE	m_type = ELEMENT_TYPE.UNKNOWN;
		public RectangleF	m_rectangle;
		public string		m_URL = null;	// Link elements can have an URL
		public DOMElement[]	m_children;

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public DOMElement() {

		}

		public DOMElement( BinaryReader _reader ) {
			Read( _reader );
		}

		#region I/O

		public void	Write( BinaryWriter _writer ) {
			_writer.Write( m_path );
			_writer.Write( (int) m_type );

			_writer.Write( m_rectangle.X );
			_writer.Write( m_rectangle.Y );
			_writer.Write( m_rectangle.Width );
			_writer.Write( m_rectangle.Height );

			if ( m_type == ELEMENT_TYPE.LINK ) {
				_writer.Write( m_URL );
			}

			_writer.Write( m_children != null ? m_children.Length : 0 );
			if ( m_children != null ) {
				foreach ( DOMElement child in m_children )
					child.Write( _writer );
			}
		}

		public void	Read( BinaryReader _reader ) {
			m_path = _reader.ReadString();
			m_type = (ELEMENT_TYPE) _reader.ReadInt32();
			m_rectangle = new RectangleF( _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle() );

			if ( m_type == ELEMENT_TYPE.LINK ) {
				m_URL = _reader.ReadString();
			}

			int	childrenCount = _reader.ReadInt32();
			m_children = childrenCount > 0 ? new DOMElement[childrenCount] : null;
			for ( int childIndex=0; childIndex < childrenCount; childIndex++ ) {
				m_children[childIndex] = new DOMElement( _reader );
			}
		}

		#endregion

		public static DOMElement	FromPageRendererXML( XmlDocument _XML ) {
			if ( _XML == null || !_XML.HasChildNodes )
				return null;

			return FromPageRendererXML( _XML["root"] );
		}

		private static DOMElement	FromPageRendererXML( XmlElement _element ) {
			DOMElement	R = new DOMElement();

			if ( _element.Name == "root" ) {
				R.m_type = ELEMENT_TYPE.ROOT;
			} else {
				R.m_path = _element.GetAttribute( "path" );
				string	typeName = _element.GetAttribute( "type" );
				switch ( typeName ) {
					case "TEXT": R.m_type = ELEMENT_TYPE.TEXT; break;
					case "LINK": R.m_type = ELEMENT_TYPE.LINK; break;
					case "IMAGE": R.m_type = ELEMENT_TYPE.IMAGE; break;
					default: R.m_type = ELEMENT_TYPE.UNKNOWN; break;
				}

				float	X, Y, W, H;
				if (	float.TryParse( _element.GetAttribute( "x" ), out X )
					&&	float.TryParse( _element.GetAttribute( "y" ), out Y )
					&&	float.TryParse( _element.GetAttribute( "w" ), out W )
					&&	float.TryParse( _element.GetAttribute( "h" ), out H ) ) {
					R.m_rectangle = new RectangleF( X, Y, W, H );
				}

				if ( R.m_type == ELEMENT_TYPE.LINK ) {
					R.m_URL = _element.GetAttribute( "URL" );
				}
			}

			List< DOMElement >	tempChildren = new List<DOMElement>();
			foreach ( XmlNode childNode in _element.ChildNodes ) {
				if ( childNode is XmlElement ) {
					tempChildren.Add( FromPageRendererXML( childNode as XmlElement ) );
				}
			}
			R.m_children = tempChildren.ToArray();

			return R;
		}

		#endregion
	}
}
