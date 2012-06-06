using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nuaj.Cirrus.Utility
{
	public partial class IntegerTrackbarControl : Panel
	{
		#region CONSTANTS

		protected const int		DEFAULT_VISIBLE_RANGE_MIN = 0;
		protected const int		DEFAULT_VISIBLE_RANGE_MAX = 100;

		#endregion

		#region NESTED TYPES

		public delegate void	ValueChangedEventHandler( IntegerTrackbarControl _Sender, int _FormerValue );
		public delegate void	SliderDragStartEventHandler( IntegerTrackbarControl _Sender );
		public delegate void	SliderDragStopEventHandler( IntegerTrackbarControl _Sender, int _StartValue );

		#endregion

		#region FIELDS

		protected int				m_Value = 0;
		protected int				m_RangeMin = -int.MaxValue;
		protected int				m_RangeMax = +int.MaxValue;
		protected int				m_VisibleRangeMin = DEFAULT_VISIBLE_RANGE_MIN;
		protected int				m_VisibleRangeMax = DEFAULT_VISIBLE_RANGE_MAX;

			// Default visible range
		protected int				m_DefaultVisibleRangeMin = DEFAULT_VISIBLE_RANGE_MIN;
		protected int				m_DefaultVisibleRangeMax = DEFAULT_VISIBLE_RANGE_MAX;

		protected bool				m_bInternalChange = false;

		protected Rectangle			m_SliderRectangle = Rectangle.Empty;
		protected bool				m_bSliderDragging = false;
		protected int				m_StartValue = 0;

		// Graphics
		protected SolidBrush		m_BackgroundBrush = null;
		protected SolidBrush		m_SliderBrush = null;
		protected TextureBrush		m_BrushTrackbarMiddle = null;

		#endregion

		#region PROPERTIES

		[Description( "The value to edit" )]
		[Category( "Value" )]
		public int		Value
		{
			get { return m_Value; }
			set
			{
				value = Math.Max( m_RangeMin, Math.Min( m_RangeMax, value ) );	// Clamp the value to its range

				if ( value == m_Value || m_bInternalChange )
					return;	// No or internal change...

				int	fFormerValue = m_Value;
				m_Value = value;

				m_bInternalChange = true;

				// Update visible range
				if ( m_Value < VisibleRangeMin )
				{
					VisibleRangeMin = 2 * m_Value;				// Double negative range
					VisibleRangeMax = m_DefaultVisibleRangeMax;	// Restore maximal range
				}
				else if ( m_Value > VisibleRangeMax )
				{
					VisibleRangeMin = m_DefaultVisibleRangeMin;	// Restore minimal range
					VisibleRangeMax = 2 * m_Value;				// Double positive range
				}

				// Update GUI
				textBox.Text = m_Value.ToString();
				Invalidate();

				m_bInternalChange = false;

				// Notify
				if ( ValueChanged != null )
					ValueChanged( this, fFormerValue );
			}
		}

		[Description( "The minimum allowed value" )]
		[Category( "Value" )]
		[DefaultValue( -int.MaxValue )]
		public int	RangeMin
		{
			get { return m_RangeMin; }
			set
			{
				m_bInternalChange = true;

				m_RangeMin = value;
				Value = Math.Max( Value, m_RangeMin );	// Clamp value if needed

				// Update GUI
				Invalidate();

				m_bInternalChange = false;
			}
		}

		[Description( "The maximum allowed value" )]
		[Category( "Value" )]
		[DefaultValue( +int.MaxValue )]
		public int	RangeMax
		{
			get { return m_RangeMax; }
			set
			{
				m_bInternalChange = true;

				m_RangeMax = value;
				Value = Math.Min( Value, m_RangeMax );	// Clamp value if needed

				// Update GUI
				Invalidate();

				m_bInternalChange = false;
			}
		}

		[Description( "The minimum value shown by the trackbar" )]
		[Category( "Value" )]
		[DefaultValue( DEFAULT_VISIBLE_RANGE_MIN )]
		public int	VisibleRangeMin
		{
			get { return m_VisibleRangeMin; }
			set
			{
				m_VisibleRangeMin = Math.Max( m_RangeMin, value );			// Can't go further than allowed range anyway
				m_VisibleRangeMin = Math.Min( Value, m_VisibleRangeMin );	// But can't go higher than displayed value either...

				if ( !m_bInternalChange )
				{	// This means it's an external change (obvsiouly) so we can use the provided value as a default value
					m_DefaultVisibleRangeMin = m_VisibleRangeMin;
				}

				// Update GUI
				Invalidate();
			}
		}

		[Description( "The maximum value shown by the trackbar" )]
		[Category( "Value" )]
		[DefaultValue( DEFAULT_VISIBLE_RANGE_MAX )]
		public int	VisibleRangeMax
		{
			get { return m_VisibleRangeMax; }
			set
			{
				m_VisibleRangeMax = Math.Min( m_RangeMax, value );			// Can't go further than allowed range anyway
				m_VisibleRangeMax = Math.Max( Value, m_VisibleRangeMax );	// But can't go lower than displayed value either...

				if ( !m_bInternalChange )
				{	// This means it's an external change (obvsiouly) so we can use the provided value as a default value
					m_DefaultVisibleRangeMax = m_VisibleRangeMax;
				}

				// Update GUI
				Invalidate();
			}
		}

		[Description( "Triggered whenever the value changes" )]
		[Category( "Value" )]
		public event ValueChangedEventHandler		ValueChanged;

		[Description( "Triggered when starting to drag the slider" )]
		[Category( "Value" )]
		public event SliderDragStartEventHandler	SliderDragStart;

		[Description( "Triggered when stopped to drag the slider" )]
		[Category( "Value" )]
		public event SliderDragStopEventHandler		SliderDragStop;

		#endregion

		#region METHODS

		public IntegerTrackbarControl()
		{
			SetStyle( ControlStyles.Selectable, true );
			SetStyle( ControlStyles.DoubleBuffer, true );
			SetStyle( ControlStyles.AllPaintingInWmPaint, true );
			SetStyle( ControlStyles.UserPaint, true );
			SetStyle( ControlStyles.ResizeRedraw, true );

			InitializeComponent();

			textBox.Location = new System.Drawing.Point( 8, 3 );

			InitializeGraphics();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if ( components != null )
					components.Dispose();

				DisposeGraphics();
			}

			base.Dispose( disposing );
		}

		/// <summary>
		/// Cancels the current drag
		/// </summary>
		public void				CancelDrag()
		{
			if ( !m_bSliderDragging )
				return;	// Not dragging anyway...

			m_bSliderDragging = false;

			// Restore start value
			Value = m_StartValue;

			// Notify of the stop of value change
			if ( SliderDragStop != null )
				SliderDragStop( this, m_StartValue );
		}

		#region Control Members

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );

			// Recompute slider range
			Point	SliderTopLeft = new Point( 61, 2 );
			Point	SliderBottomRight = new Point( Width-4, 16 );

			m_SliderRectangle = new Rectangle( SliderTopLeft.X, SliderTopLeft.Y, 1 + SliderBottomRight.X - SliderTopLeft.X, 1 + SliderBottomRight.Y - SliderTopLeft.Y );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			if ( e.Button != MouseButtons.Left )
				return;
			if ( !m_SliderRectangle.Contains( e.X, e.Y ) )
				return;

			m_bSliderDragging = true;
			m_StartValue = Value;

			Focus();

			// Notify of the start of value change
			if ( SliderDragStart != null )
				SliderDragStart( this );

			// Simulate a move so we update the value
			OnMouseMove( e );
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			// Update value...
			if ( !m_bSliderDragging )
				return;

			int	NewValue = (int) Math.Floor( VisibleRangeMin + (float) (e.X - m_SliderRectangle.X) * (VisibleRangeMax - VisibleRangeMin) / m_SliderRectangle.Width );
			Value = Math.Max( VisibleRangeMin, Math.Min( VisibleRangeMax, NewValue ) );
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			if ( !m_bSliderDragging )
				return;

			m_bSliderDragging = false;

			// Notify of the stop of value change
			if ( SliderDragStop != null )
				SliderDragStop( this, m_StartValue );
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );

			if ( e.KeyCode == Keys.Escape )
				CancelDrag();		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			// Draw the slider
			float	fSizeToDraw = m_SliderRectangle.Width * (Value - VisibleRangeMin) / Math.Max( 1, VisibleRangeMax - VisibleRangeMin );
			e.Graphics.FillRectangle( m_BackgroundBrush, m_SliderRectangle.X + fSizeToDraw, m_SliderRectangle.Y, m_SliderRectangle.Width - fSizeToDraw, m_SliderRectangle.Height );
			e.Graphics.FillRectangle( m_SliderBrush, m_SliderRectangle.X, m_SliderRectangle.Y, fSizeToDraw, m_SliderRectangle.Height );

			// Draw the left image portion
			e.Graphics.DrawImage( Properties.Resources.Trackbar___Left, 0, 0, Properties.Resources.Trackbar___Left.Width, Properties.Resources.Trackbar___Left.Height );

			// Draw the middle image portion
			int		DrawLeft = Properties.Resources.Trackbar___Left.Width;
			int		DrawRight = Width - Properties.Resources.Trackbar___Right.Width;
			e.Graphics.FillRectangle( m_BrushTrackbarMiddle, DrawLeft, 0, DrawRight - DrawLeft, Properties.Resources.Trackbar___Middle.Height );

			// Draw the right image portion
			e.Graphics.DrawImage( Properties.Resources.Trackbar___Right, DrawRight, 0, Properties.Resources.Trackbar___Right.Width, Properties.Resources.Trackbar___Right.Height );
		}

		#endregion

		#region Graphics Creation

		protected void	InitializeGraphics()
		{
			m_BackgroundBrush = new SolidBrush( Color.FromArgb( 137, 137, 137 ) );
			m_SliderBrush = new SolidBrush( Color.FromArgb( 200, 200, 200 ) );

		    ImageAttributes	ImageAttr = new ImageAttributes();
							ImageAttr.SetWrapMode( WrapMode.Tile );	// Set the wrap mode so it tiles
 
			// Create a TextureBrush for the middle trackbar portion to tile
			Rectangle	BrushRect = new Rectangle( new Point( 0, 0 ), Properties.Resources.Trackbar___Middle.Size );
			m_BrushTrackbarMiddle = new TextureBrush( Properties.Resources.Trackbar___Middle, BrushRect, ImageAttr );
		}

		protected void	DisposeGraphics()
		{
			m_BrushTrackbarMiddle.Dispose();
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void textBox_Validating( object sender, CancelEventArgs e )
		{
			int	NewValue = 0;
			if ( !int.TryParse( textBox.Text, out NewValue ) )
			{	// Reset current value...
				textBox.Text = Value.ToString();
				e.Cancel = true;
				return;
			}

			// Notify of the start of value change
			if ( SliderDragStart != null )
				SliderDragStart( this );

			// Commit change
			int		StartValue = Value;
			Value = NewValue;

			// Notify of the stop of value change
			if ( SliderDragStop != null )
				SliderDragStop( this, StartValue );
		}

		private void textBox_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode != Keys.Return )
				return;

			this.Focus();	// Should validate the text...
		}

		#endregion
	}
}
