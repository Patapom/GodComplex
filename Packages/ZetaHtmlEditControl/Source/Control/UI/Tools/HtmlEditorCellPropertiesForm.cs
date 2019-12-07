namespace ZetaHtmlEditControl.UI.Tools
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using System.Windows.Forms;
    using Code.Configuration;
    using Code.Helper;
    using mshtml;
    using Properties;

    public partial class HtmlEditorCellPropertiesForm : 
		Form
	{
        public enum HorizontalAlignmentType
        {
            #region Enum members.

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Multiple",
                typeof(Resources))]
            Multiple = -1,

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Standard",
                typeof(Resources))]
            Standard = 0,

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Left",
                typeof(Resources))]
            Left = 1,

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Right",
                typeof(Resources))]
            Right = 2,

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Center",
                typeof(Resources))]
            Center = 3,

            [LocalizableDescription(
                @"Str_HorizontalAlignmentType_Justify",
                typeof(Resources))]
            Justify = 4

            #endregion
        }

        public enum VerticalAlignmentType
        {
            #region Enum members.

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_Multiple",
                typeof(Resources))]
            Multiple = -1,

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_Standard",
                typeof(Resources))]
            Standard = 0,

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_Top",
                typeof(Resources))]
            Top = 1,

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_Middle",
                typeof(Resources))]
            Middle = 2,

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_BaseLine",
                typeof(Resources))]
            BaseLine = 3,

            [LocalizableDescription(
                @"Str_VerticalAlignmentType_Bottom",
                typeof(Resources))]
            Bottom = 4

            #endregion
        }

        private readonly List<IHTMLTableCell> _cells =
            new List<IHTMLTableCell>();

        private bool _isDuringInitialization;

        public HtmlEditorCellPropertiesForm()
        {
            InitializeComponent();

            widthDropDownControl.SelectedIndex = 0;
            heightDropDownControl.SelectedIndex = 0;
        }

        [Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal IExternalInformationProvider ExternalInformationProvider
		{
			get;
			set;
		}

        public void Initialize(
			IEnumerable<IHTMLTableCell> cells)
		{
			Text = Resources.Str_UIHtml_CellProperties;

			_cells.AddRange(cells);
		}

		public void Initialize(
			IHTMLTableRow row)
		{
			Text = Resources.Str_UIHtml_RowProperties;

			IHTMLElementCollection cells = row.cells;

			if (cells != null)
			{
				for (int i = 0; i < cells.length; ++i)
				{
					_cells.Add(cells.item(i, i)
						as IHTMLTableCell);
				}
			}
		}

		public void Initialize(
			IHTMLTable table,
			int columnIndex)
		{
			Text = Resources.Str_UIHtml_ColumnProperties;

			var rows = table.rows;

			if (rows != null)
			{
				for (var i = 0; i < rows.length; ++i)
				{
					var row = (IHTMLTableRow)rows.item(i, i);
					var cells = row.cells;

					if (cells != null)
					{
						for (int j = 0; j < cells.length; ++j)
						{
							if (j == columnIndex)
							{
								_cells.Add(cells.item(j, j) as IHTMLTableCell);
								break;
							}
						}
					}
				}
			}
		}

		private void updateUI()
		{
			var enableWidth = defineWidthCheckBox.CheckState == CheckState.Checked;
			widthTextBox.Enabled = enableWidth;
			widthDropDownControl.Enabled = enableWidth;

			var enableHeight = defineHeightCheckBox.CheckState == CheckState.Checked;
			heightTextBox.Enabled = enableHeight;
			heightDropDownControl.Enabled = enableHeight;
		}

		private void FillCellValuesToDialogControls()
		{
			if (_cells.Count > 0)
			{
				// --
				// Read the values from all cells.

				int differentHorizontalAlignmentCount = 0;
				string horizontalAlignment = string.Empty;

				int differentVerticalAlignmentCount = 0;
				string verticalAlignment = string.Empty;

				int differentWidthCount = 0;
				string width = string.Empty;

				int differentHeightCount = 0;
				string height = string.Empty;

				int differentNoWrapCount = 0;
				var noWrap = CheckState.Unchecked;

				int differentContainsHeadlineCount = 0;
				var containsHeadline = CheckState.Unchecked;

				foreach (var cell in _cells)
				{
					var cellElement = (IHTMLElement)cell;

					string tagName = cellElement.tagName.ToLowerInvariant();

					// --
					// Horizontal alignment.

					string currentHorizontalAlignment =
						ConvertHelper.ToString(
						cell.align,
						string.Empty).ToLowerInvariant();

					if (differentHorizontalAlignmentCount == 0)
					{
						horizontalAlignment = currentHorizontalAlignment;
						differentHorizontalAlignmentCount++;
					}
					else
					{
						if (horizontalAlignment != currentHorizontalAlignment)
						{
							differentHorizontalAlignmentCount++;
						}
					}

					// --
					// Vertical alignment.

					var currentVerticalAlignment =
						ConvertHelper.ToString(
							cell.vAlign, string.Empty).ToLowerInvariant();

					if (differentVerticalAlignmentCount == 0)
					{
						verticalAlignment = currentVerticalAlignment;
						differentVerticalAlignmentCount++;
					}
					else
					{
						if (verticalAlignment != currentVerticalAlignment)
						{
							differentVerticalAlignmentCount++;
						}
					}

					// --
					// Width.

					string currentWidth = Convert.ToString(
					   cellElement.getAttribute(@"width"));

					if (differentWidthCount == 0)
					{
						width = currentWidth;
						differentWidthCount++;
					}
					else
					{
						if (width != currentWidth)
						{
							differentWidthCount++;
						}
					}

					// -- 
					// Height.

					var currentHeight = Convert.ToString(
					   cellElement.getAttribute(@"height"));

					if (differentHeightCount == 0)
					{
						height = currentHeight;
						differentHeightCount++;
					}
					else
					{
						if (height != currentHeight)
						{
							differentHeightCount++;
						}
					}

					// --
					// Wrap.

					CheckState currentNoWrap =
						cell.noWrap ?
						CheckState.Checked :
						CheckState.Unchecked;

					if (differentNoWrapCount == 0)
					{
						noWrap = currentNoWrap;
						differentNoWrapCount++;
					}
					else
					{
						if (noWrap != currentNoWrap)
						{
							differentNoWrapCount++;
						}
					}

					// --
					// Headline.

					var currentHeadline =
						tagName == @"th" ?
						CheckState.Checked :
						CheckState.Unchecked;

					if (differentContainsHeadlineCount == 0)
					{
						containsHeadline = currentHeadline;
						differentContainsHeadlineCount++;
					}
					else
					{
						if (containsHeadline != currentHeadline)
						{
							differentContainsHeadlineCount++;
						}
					}
				}

				// --
				// Apply horizontal alignment.

				// If more than one, select none.
				var horizontalAlignmentIndex =
					HorizontalAlignmentType.Multiple;

				if (differentHorizontalAlignmentCount == 1)
				{
					if (string.IsNullOrEmpty(horizontalAlignment))
					{
						horizontalAlignmentIndex =
							HorizontalAlignmentType.Standard;
					}
					else if (horizontalAlignment == @"center")
					{
						horizontalAlignmentIndex =
							HorizontalAlignmentType.Center;
					}
					else if (horizontalAlignment == @"justify")
					{
						horizontalAlignmentIndex =
							HorizontalAlignmentType.Justify;
					}
					else if (horizontalAlignment == @"left")
					{
						horizontalAlignmentIndex =
							HorizontalAlignmentType.Left;
					}
					else if (horizontalAlignment == @"right")
					{
						horizontalAlignmentIndex =
							HorizontalAlignmentType.Right;
					}
				}

				// Select.
				if (horizontalAlignmentIndex ==
					HorizontalAlignmentType.Multiple)
				{
					horizontalAlignmentComboBox.SelectedIndex = -1;
				}
				else
				{
					foreach (Tuple<string, HorizontalAlignmentType> p in
						horizontalAlignmentComboBox.Items)
					{
						if (p.Item2 == horizontalAlignmentIndex)
						{
							horizontalAlignmentComboBox.SelectedItem = p;
							break;
						}
					}
				}

				// --
				// Apply vertical alignment.

				// If more than one, select none.
				var verticalAlignmentIndex =
					VerticalAlignmentType.Multiple;

				if (differentVerticalAlignmentCount == 1)
				{
					if (string.IsNullOrEmpty(verticalAlignment))
					{
						verticalAlignmentIndex =
							VerticalAlignmentType.Standard;
					}
					else if (verticalAlignment == @"top")
					{
						verticalAlignmentIndex =
							VerticalAlignmentType.Top;
					}
					else if (verticalAlignment == @"middle")
					{
						verticalAlignmentIndex =
							VerticalAlignmentType.Middle;
					}
					else if (verticalAlignment == @"bottom")
					{
						verticalAlignmentIndex =
							VerticalAlignmentType.Bottom;
					}
					else if (verticalAlignment == @"baseline")
					{
						verticalAlignmentIndex =
							VerticalAlignmentType.BaseLine;
					}
				}

				// Select.
				if (verticalAlignmentIndex == VerticalAlignmentType.Multiple)
				{
					verticalAlignmentComboBox.SelectedIndex = -1;
				}
				else
				{
					foreach (Tuple<string, VerticalAlignmentType> p in
						verticalAlignmentComboBox.Items)
					{
						if (p.Item2 == verticalAlignmentIndex)
						{
							verticalAlignmentComboBox.SelectedItem = p;
							break;
						}
					}
				}

				// --
				// Apply width.

				var isWidthPercentage = width.IndexOf(@"%", StringComparison.Ordinal) >= 0;
				width = width.Trim('%');
				width = width.Trim();

				widthTextBox.Text = width;
				widthDropDownControl.SelectedIndex = isWidthPercentage ? 1 : 0;

				if (differentWidthCount == 1)
				{
					defineWidthCheckBox.CheckState =
						string.IsNullOrEmpty(width) ?
						CheckState.Unchecked :
						CheckState.Checked;
				}
				else
				{
					defineWidthCheckBox.CheckState = CheckState.Indeterminate;
				}

				// --
				// Apply height.

				var isHeightPercentage = height.IndexOf(@"%", StringComparison.Ordinal) >= 0;
				height = height.Trim('%');
				height = height.Trim();

				heightTextBox.Text = height;
				heightDropDownControl.SelectedIndex = isHeightPercentage ? 1 : 0;

				if (differentHeightCount == 1)
				{
					defineHeightCheckBox.CheckState =
						string.IsNullOrEmpty(height) ?
						CheckState.Unchecked :
						CheckState.Checked;
				}
				else
				{
					defineHeightCheckBox.CheckState = CheckState.Indeterminate;
				}

				// --
				// Apply wrap.

				noWrapCheckBox.CheckState = differentNoWrapCount == 1 ? noWrap : CheckState.Indeterminate;

				// --
				// Apply headline.

				containsHeadlineCheckBox.CheckState =
					differentContainsHeadlineCount == 1
						? containsHeadline
						: CheckState.Indeterminate;

				// --

				updateUI();
			}
		}

		private void FillDialogControlsToCellValues()
		{
			if (_cells.Count > 0)
			{
				foreach (IHTMLTableCell it in _cells)
				{
					FillDialogControlsToCellValue(it);
				}
			}
		}

		private void FillDialogControlsToCellValue(
			IHTMLTableCell cell)
		{
			var cellElement = (IHTMLElement)cell;

			// --

			if (horizontalAlignmentComboBox.SelectedItem != null)
			{
				var p =
					(Tuple<string, HorizontalAlignmentType>)
						horizontalAlignmentComboBox.SelectedItem;

				if (p.Item2 != HorizontalAlignmentType.Multiple)
				{
					string a = string.Empty;

					switch (p.Item2)
					{
						case HorizontalAlignmentType.Standard:
							a = string.Empty;
							break;
						case HorizontalAlignmentType.Center:
							a = @"center";
							break;
						case HorizontalAlignmentType.Left:
							a = @"left";
							break;
						case HorizontalAlignmentType.Right:
							a = @"right";
							break;
						case HorizontalAlignmentType.Justify:
							a = @"justify";
							break;
					}

					cell.align = a;
				}
			}

			// --

			if (verticalAlignmentComboBox.SelectedItem != null)
			{
				var p =
					(Tuple<string, VerticalAlignmentType>)
					verticalAlignmentComboBox.SelectedItem;

				if (p.Item2 != VerticalAlignmentType.Multiple)
				{
					var a = string.Empty;

					switch (p.Item2)
					{
						case VerticalAlignmentType.Standard:
							a = string.Empty;
							break;
						case VerticalAlignmentType.BaseLine:
							a = @"baseline";
							break;
						case VerticalAlignmentType.Bottom:
							a = @"bottom";
							break;
						case VerticalAlignmentType.Top:
							a = @"top";
							break;
						case VerticalAlignmentType.Middle:
							a = @"middle";
							break;
					}

					cell.vAlign = a;
				}
			}

			// --

			if (defineWidthCheckBox.CheckState != CheckState.Indeterminate)
			{
				string w = widthTextBox.Text;

				if (widthDropDownControl.SelectedIndex==1)
				{
					w += @"%";
				}

				cellElement.setAttribute(
					@"width",
					w,
					0);
			}

			// --

			if (defineHeightCheckBox.CheckState != CheckState.Indeterminate)
			{
				var h = heightTextBox.Text;

				if (heightDropDownControl.SelectedIndex==1)
				{
					h += @"%";
				}

				cellElement.setAttribute(
					@"height",
					h,
					0);
			}

			// --

			if (noWrapCheckBox.CheckState != CheckState.Indeterminate)
			{
				cell.noWrap = noWrapCheckBox.CheckState == CheckState.Checked;
			}

			// --

			// Change the tag name between TH <. TD.
			if (containsHeadlineCheckBox.CheckState != CheckState.Indeterminate)
			{
				string newTagName =
				   containsHeadlineCheckBox.CheckState == CheckState.Checked ? @"th" : @"td";

				string tagName = cellElement.tagName.ToLowerInvariant();

				if (tagName != newTagName)
				{
					var outerHtmlSb =
						new StringBuilder(cellElement.outerHTML.Trim());

					// Replace start.
					outerHtmlSb[1] = newTagName[0];
					outerHtmlSb[2] = newTagName[1];

					// Replace end.
					outerHtmlSb[outerHtmlSb.Length - 3] = newTagName[0];
					outerHtmlSb[outerHtmlSb.Length - 2] = newTagName[1];

					string outerHtml = outerHtmlSb.ToString();

					// --

					const int startPos1 = 0;
					int endPos1 = outerHtml.IndexOf('>');

					int startPos2 = outerHtml.LastIndexOf('<');
					int endPos2 = outerHtml.Length - 1;

					Debug.Assert(startPos1 != -1);
					Debug.Assert(endPos1 != -1);
					Debug.Assert(startPos2 != -1);
					Debug.Assert(endPos2 != -1);

					string newHtmlTag =
						outerHtml.Substring(startPos1, endPos1 - startPos1 + 1);
					string newHtmlInnerHtml =
						outerHtml.Substring(endPos1 + 1, startPos2 - endPos1 - 1);

					// --

					var cellNode =(IHTMLDOMNode2)cell;
					var doc =(IHTMLDocument2)cellNode.ownerDocument;

					var newElement = doc.createElement(
						newHtmlTag);
					newElement.innerHTML = newHtmlInnerHtml;

					var newNode =
						newElement as IHTMLDOMNode;

					// Set the new.
                    var cellNode1 = (IHTMLDOMNode)cell;
					cellNode1.replaceNode(newNode);
				}
			}
		}

        private void HtmlEditorCellPropertiesForm_Load(
			object sender,
			EventArgs e)
		{
			CenterToParent();

			_isDuringInitialization = true;
			try
			{
				// --
				// Fill lists.

				horizontalAlignmentComboBox.Items.Clear();
				horizontalAlignmentComboBox.Items.Add(new Tuple<string, HorizontalAlignmentType>(StringHelper.GetEnumDescription(HorizontalAlignmentType.Standard), HorizontalAlignmentType.Standard));
				horizontalAlignmentComboBox.Items.Add(new Tuple<string, HorizontalAlignmentType>(StringHelper.GetEnumDescription(HorizontalAlignmentType.Left), HorizontalAlignmentType.Left));
				horizontalAlignmentComboBox.Items.Add(new Tuple<string, HorizontalAlignmentType>(StringHelper.GetEnumDescription(HorizontalAlignmentType.Right), HorizontalAlignmentType.Right));
				horizontalAlignmentComboBox.Items.Add(new Tuple<string, HorizontalAlignmentType>(StringHelper.GetEnumDescription(HorizontalAlignmentType.Center), HorizontalAlignmentType.Center));
				horizontalAlignmentComboBox.Items.Add(new Tuple<string, HorizontalAlignmentType>(StringHelper.GetEnumDescription(HorizontalAlignmentType.Justify), HorizontalAlignmentType.Justify));

				verticalAlignmentComboBox.Items.Clear();
				verticalAlignmentComboBox.Items.Add(new Tuple<string, VerticalAlignmentType>(StringHelper.GetEnumDescription(VerticalAlignmentType.Standard), VerticalAlignmentType.Standard));
				verticalAlignmentComboBox.Items.Add(new Tuple<string, VerticalAlignmentType>(StringHelper.GetEnumDescription(VerticalAlignmentType.Top), VerticalAlignmentType.Top));
				verticalAlignmentComboBox.Items.Add(new Tuple<string, VerticalAlignmentType>(StringHelper.GetEnumDescription(VerticalAlignmentType.Middle), VerticalAlignmentType.Middle));
				verticalAlignmentComboBox.Items.Add(new Tuple<string, VerticalAlignmentType>(StringHelper.GetEnumDescription(VerticalAlignmentType.BaseLine), VerticalAlignmentType.BaseLine));
				verticalAlignmentComboBox.Items.Add(new Tuple<string, VerticalAlignmentType>(StringHelper.GetEnumDescription(VerticalAlignmentType.Bottom), VerticalAlignmentType.Bottom));

				FillCellValuesToDialogControls();

				// --

				updateUI();
			}
			finally
			{
				_isDuringInitialization = false;
			}
		}

		private void buttonOK_Click(
			object sender,
			EventArgs e)
		{
			FillDialogControlsToCellValues();
		}

		private void defineWidthCheckBox_CheckedChanged(
			object sender,
			EventArgs e)
		{
			if (!_isDuringInitialization)
			{
				// Only allow two states after click.
				if (defineWidthCheckBox.CheckState ==
					CheckState.Indeterminate)
				{
					defineWidthCheckBox.CheckState =
						CheckState.Unchecked;
				}

				updateUI();
			}
		}

		private void defineHeightCheckBox_CheckedChanged(
			object sender,
			EventArgs e)
		{
			if (!_isDuringInitialization)
			{
				// Only allow two states after click.
				if (defineHeightCheckBox.CheckState ==
					CheckState.Indeterminate)
				{
					defineHeightCheckBox.CheckState =
						CheckState.Unchecked;
				}

				updateUI();
			}
		}

		private void containsHeadlineCheckBox_CheckedChanged(
			object sender,
			EventArgs e)
		{
			if (!_isDuringInitialization)
			{
				// Only allow two states after click.
				if (containsHeadlineCheckBox.CheckState ==
					CheckState.Indeterminate)
				{
					containsHeadlineCheckBox.CheckState =
						CheckState.Unchecked;
				}

				updateUI();
			}
		}

		private void noWrapCheckBox_CheckedChanged(
			object sender,
			EventArgs e)
		{
			if (!_isDuringInitialization)
			{
				// Only allow two states after click.
				if (noWrapCheckBox.CheckState == CheckState.Indeterminate)
				{
					noWrapCheckBox.CheckState = CheckState.Unchecked;
				}

				updateUI();
			}
		}
	}
}