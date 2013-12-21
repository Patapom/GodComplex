using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShaderInterpreter
{
	public partial class SourceErrorForm : Form
	{
		private Font		m_ErrorFont = null;
		[System.ComponentModel.EditorBrowsable]
		public Font								ErrorFont
		{
			get { return m_ErrorFont; }
			set { m_ErrorFont = value; }
		}

		private string							m_ErrorMessage = "";
		private Converter.ConverterException	m_Exception = null;
		public  Converter.ConverterException	Exception
		{
			get { return m_Exception; }
			set
			{
				if ( value == null )
					return;

				m_Exception = value;

				string	Header = m_ErrorMessage + m_Exception.Message + "\n\n";

				richTextBoxErrors.Text = Header + m_Exception.m_Source;

				// Highlight error message
				richTextBoxErrors.SelectionStart = m_ErrorMessage.Length;
				richTextBoxErrors.SelectionLength = m_Exception.Message.Length;
				richTextBoxErrors.SelectionColor = Color.Red;

				// Highlight actual error
				int	SelectionStart = m_Exception.m_PositionStart;
				int	SelectionEnd = m_Exception.m_PositionEnd;
				if ( SelectionEnd == SelectionStart )
					SelectionEnd = m_Exception.FindEOL( SelectionStart );

				richTextBoxErrors.SelectionStart = Header.Length + SelectionStart;
				richTextBoxErrors.SelectionLength = SelectionEnd - SelectionStart;
				richTextBoxErrors.SelectionColor = Color.Red;

				SelectionStart = m_Exception.FindBOL( m_Exception.m_PositionStart );
				SelectionEnd = m_Exception.FindEOL( m_Exception.m_PositionStart );
				richTextBoxErrors.SelectionStart = Header.Length + SelectionStart;
				richTextBoxErrors.SelectionLength = SelectionEnd - SelectionStart;
				richTextBoxErrors.SelectionFont = m_ErrorFont;
				richTextBoxErrors.SelectionBackColor = Color.LightGray;

				richTextBoxErrors.SelectionLength = 0;
			}
		}

		public SourceErrorForm( string _ErrorMessage, Converter.ConverterException _ConverterException )
		{
			InitializeComponent();

			m_ErrorFont = new Font( this.Font, FontStyle.Bold );

			m_ErrorMessage = _ErrorMessage;
			Exception = _ConverterException;
		}
	}
}
