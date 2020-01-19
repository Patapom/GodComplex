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

		#region FIELDS

		public string		m_type = null;
		public RectangleF	m_rectangle;
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
			_writer.Write( m_type != null );
			if ( m_type != null )
				_writer.Write( m_type );

			_writer.Write( m_rectangle.X );
			_writer.Write( m_rectangle.Y );
			_writer.Write( m_rectangle.Width );
			_writer.Write( m_rectangle.Height );

			_writer.Write( m_children != null ? m_children.Length : 0 );
			if ( m_children != null ) {
				foreach ( DOMElement child in m_children )
					child.Write( _writer );
			}
		}

		public void	Read( BinaryReader _reader ) {
			m_type = _reader.ReadBoolean() ? _reader.ReadString() : null;
			m_rectangle = new RectangleF( _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle() );

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

			return FromPageRendererXML( _XML["ROOT"] );
		}

		static List< DOMElement >	ms_tempChildren = new List<DOMElement>();
		private static DOMElement	FromPageRendererXML( XmlElement _element ) {
			DOMElement	R = new DOMElement();

			R.m_type = _element.GetAttribute( "type" );

			float	X, Y, W, H;
			if (	float.TryParse( _element.GetAttribute( "x" ), out X )
				&&	float.TryParse( _element.GetAttribute( "y" ), out Y )
				&&	float.TryParse( _element.GetAttribute( "w" ), out W )
				&&	float.TryParse( _element.GetAttribute( "h" ), out H ) ) {
				R.m_rectangle = new RectangleF( X, Y, W, H );
			}

			ms_tempChildren.Clear();
			foreach ( XmlNode childNode in _element.ChildNodes ) {
				if ( childNode is XmlElement ) {
					ms_tempChildren.Add( FromPageRendererXML( childNode as XmlElement ) );
				}
			}
			R.m_children = ms_tempChildren.ToArray();

			return R;
		}

		#endregion
	}
}
