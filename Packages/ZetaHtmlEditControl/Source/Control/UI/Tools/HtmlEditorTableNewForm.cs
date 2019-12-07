namespace ZetaHtmlEditControl.UI.Tools
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms;
    using Code.Configuration;
    using Code.Helper;
    using mshtml;
    using Properties;

    public partial class HtmlEditorTableNewForm : Form
	{
        private string _html;
        private IHTMLTable _table;

        public HtmlEditorTableNewForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal IExternalInformationProvider ExternalInformationProvider
		{
			get;
			set;
		}

        private string StoreID
		{
			get
			{
				return string.Format(
					@"{0}.{1}.{2}",
					GetType().Name,
					Name,
					Text);
			}
		}

        public string Html
        {
            get
            {
                return _html;
            }
        }

        public IHTMLTable Table
        {
            set
            {
                _table = value;
            }
        }

        private bool IsNew
        {
            get
            {
                return _table == null;
            }
        }

        public static IHTMLTableRow AddTableRowsAfterRow(
			IHTMLTable table,
			int afterRowIndex,
			int count)
		{
			var columnCount = CountTableColumns(table);

			IHTMLTableRow lastRow = null;

			// Add rows.
			for (var j = 0; j < count; ++j)
			{
				var row = (IHTMLTableRow)table.insertRow(afterRowIndex);
				lastRow = row;

				// Add columns in the newly added row.
				while (row.cells.length < columnCount)
				{
					var cell = (IHTMLTableCell)row.insertCell();

					var element = (IHTMLElement)cell;
					element.innerHTML = @"&nbsp;";
				}
			}

			return lastRow;
		}

		public static void AddTableColumnsAfterColumn(
			IHTMLTable table,
			int afterColumnIndex,
			int count)
		{
			// Add columns in each row.
			for (var i = 0; i < table.rows.length; ++i)
			{
				var row = (IHTMLTableRow)table.rows.item(i, i);

				for (var j = 0; j < count; ++j)
				{
					var cell = (IHTMLTableCell)row.insertCell(afterColumnIndex);

					var element = (IHTMLElement)cell;
					element.innerHTML = @"&nbsp;";
				}
			}
		}

		public static IHTMLTableRow AddTableRowsAtBottom(
			IHTMLTable table,
			int count)
		{
			return AddTableRowsAfterRow(
				table,
				-1,
				count);
		}

		public static void AddTableColumnsAtRight(
			IHTMLTable table,
			int count)
		{
			AddTableColumnsAfterColumn(
				table,
				-1,
				count);
		}

        private static int CountTableColumns(
			IHTMLTable table)
		{
			int cols = 0;

			for (int i = 0; i < table.rows.length; ++i)
			{
				int c = CountTableRowColumns(
					table.rows.item(i, i)
					as IHTMLTableRow);
				if (c > cols)
				{
					cols = c;
				}
			}

			return cols;
		}

		private static int CountTableRowColumns(
			IHTMLTableRow row)
		{
			var count = 0;

			for (var i = 0; i < row.cells.length; ++i)
			{
				var cell = (IHTMLTableCell)row.cells.item(i, i);

				count += Math.Max(1, cell.colSpan);
			}

			return count;
		}

        private void HtmlEditorTableNewForm_Load(
			object sender,
			EventArgs e)
		{
			CenterToParent();

			// --

			horizontalAlignmentComboBox.Items.Clear();
			horizontalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Standard),
					HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Standard));
			horizontalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Left),
					HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Left));
			horizontalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Right),
					HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Right));
			horizontalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Center),
					HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Center));
			horizontalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Justify),
					HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Justify));

			verticalAlignmentComboBox.Items.Clear();
			verticalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.VerticalAlignmentType.Standard),
					HtmlEditorCellPropertiesForm.VerticalAlignmentType.Standard));
			verticalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.VerticalAlignmentType.Top),
					HtmlEditorCellPropertiesForm.VerticalAlignmentType.Top));
			verticalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.VerticalAlignmentType.Middle),
					HtmlEditorCellPropertiesForm.VerticalAlignmentType.Middle));
			verticalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.VerticalAlignmentType.BaseLine),
					HtmlEditorCellPropertiesForm.VerticalAlignmentType.BaseLine));
			verticalAlignmentComboBox.Items.Add(
				new Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>(
					StringHelper.GetEnumDescription(HtmlEditorCellPropertiesForm.VerticalAlignmentType.Bottom),
					HtmlEditorCellPropertiesForm.VerticalAlignmentType.Bottom));

			// --

			if (IsNew)
			{
				if (ExternalInformationProvider != null)
				{
					rowsUpDown.Value = ConvertHelper.ToDecimal(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.RowCount",
							rowsUpDown.Value.ToString(CultureInfo.InvariantCulture)));
					columnsUpDown.Value = ConvertHelper.ToDecimal(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.ColCount",
							columnsUpDown.Value.ToString(CultureInfo.InvariantCulture)));
					borderUpDown.Value = ConvertHelper.ToDecimal(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.Border",
							borderUpDown.Value.ToString(CultureInfo.InvariantCulture)));
					cellSpacingUpDown.Value = ConvertHelper.ToDecimal(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.CellSpacing",
							cellSpacingUpDown.Value.ToString(CultureInfo.InvariantCulture)));
					cellPaddingUpDown.Value = ConvertHelper.ToDecimal(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.CellPadding",
							cellPaddingUpDown.Value.ToString(CultureInfo.InvariantCulture)));

					horizontalAlignmentComboBox.SelectedIndex = ConvertHelper.ToInt32(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.HorizontalAlignmentIndex",
							0.ToString(CultureInfo.InvariantCulture)), 0);
					verticalAlignmentComboBox.SelectedIndex = ConvertHelper.ToInt32(
						ExternalInformationProvider.RestorePerUserPerWorkstationValue(
							StoreID + @"HtmlEditorTableNewDialog.VerticalAlignmentIndex",
							0.ToString(CultureInfo.InvariantCulture)), 0);
				}

				if (horizontalAlignmentComboBox.SelectedIndex < 0 &&
					horizontalAlignmentComboBox.Items.Count > 0)
				{
					horizontalAlignmentComboBox.SelectedIndex = 0;
				}
				if (verticalAlignmentComboBox.SelectedIndex < 0 &&
					verticalAlignmentComboBox.Items.Count > 0)
				{
					verticalAlignmentComboBox.SelectedIndex = 0;
				}
			}
			else
			{
				Text = Resources.Str_UIHtml_TableProperties;

				firstRowContainsHeadlineCheckBox.Visible = false;

				rowsUpDown.Minimum = rowsUpDown.Value = _table.rows.length;
				columnsUpDown.Minimum = columnsUpDown.Value = CountTableColumns(_table);
				borderUpDown.Value = ConvertHelper.ToDecimal(_table.border);
				cellSpacingUpDown.Value = ConvertHelper.ToDecimal(_table.cellSpacing);
				cellPaddingUpDown.Value = ConvertHelper.ToDecimal(_table.cellPadding);

				label1.Enabled =
					label2.Enabled =
					horizontalAlignmentComboBox.Enabled =
					verticalAlignmentComboBox.Enabled =
					false;

				if (horizontalAlignmentComboBox.SelectedIndex < 0 &&
					horizontalAlignmentComboBox.Items.Count > 0)
				{
					horizontalAlignmentComboBox.SelectedIndex = 0;
				}
				if (verticalAlignmentComboBox.SelectedIndex < 0 &&
					verticalAlignmentComboBox.Items.Count > 0)
				{
					verticalAlignmentComboBox.SelectedIndex = 0;
				}
			}

			// --

			updateUI();
		}

		private void buttonOK_Click(
			object sender,
			EventArgs e)
		{
			if (IsNew)
			{
				var tmpHtml = string.Format(
					@"<table border=""{0}"" cellpadding=""{1}"" cellspacing=""{2}"" width=""90%"">",
					borderUpDown.Value,
					cellPaddingUpDown.Value,
					cellSpacingUpDown.Value) + Environment.NewLine;

				for (var row = 0; row < rowsUpDown.Value; ++row)
				{
					tmpHtml += @"    <tr>" + Environment.NewLine;

					for (var column = 0; column < columnsUpDown.Value; ++column)
					{
						if (row == 0 && firstRowContainsHeadlineCheckBox.Checked)
						{
							tmpHtml += string.Format(
								@"        <th{0}{1}></th>" + Environment.NewLine,
								calculateHorizontalAlignment(),
								calculateVerticalAlignment());
						}
						else
						{
							tmpHtml += string.Format(
								@"        <td{0}{1}></td>" + Environment.NewLine,
								calculateHorizontalAlignment(),
								calculateVerticalAlignment());
						}
					}

					tmpHtml += @"    </tr>" + Environment.NewLine;
				}

				tmpHtml += @"</table>" + Environment.NewLine;

				_html = tmpHtml;
			}
			else
			{
				// Modify existing.

				// Add rows.
				while (_table.rows.length < rowsUpDown.Value)
				{
					_table.insertRow();
				}

				// Add columns.
				for (var i = 0; i < _table.rows.length; ++i)
				{
					var row = (IHTMLTableRow)_table.rows.item(i, i);

					while (row.cells.length < columnsUpDown.Value)
					{
						var cell = (IHTMLTableCell)row.insertCell();

						var element = (IHTMLElement)cell;
						element.innerHTML = @"&nbsp;";
					}
				}

				_table.border = ConvertHelper.ToInt32(borderUpDown.Value);
				_table.cellSpacing = ConvertHelper.ToInt32(cellSpacingUpDown.Value);
				_table.cellPadding = ConvertHelper.ToInt32(cellPaddingUpDown.Value);
			}

			// --

			if (ExternalInformationProvider != null)
			{
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID + @"HtmlEditorTableNewDialog.RowCount",
				                                                           rowsUpDown.Value.ToString(CultureInfo.InvariantCulture));
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID + @"HtmlEditorTableNewDialog.ColCount",
				                                                           columnsUpDown.Value.ToString(CultureInfo.InvariantCulture));
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID + @"HtmlEditorTableNewDialog.Border",
				                                                           borderUpDown.Value.ToString(CultureInfo.InvariantCulture));
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID + @"HtmlEditorTableNewDialog.CellSpacing",
				                                                           cellSpacingUpDown.Value.ToString(CultureInfo.InvariantCulture));
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID + @"HtmlEditorTableNewDialog.CellPadding",
				                                                           cellPaddingUpDown.Value.ToString(CultureInfo.InvariantCulture));

				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID +
				                                                           @"HtmlEditorTableNewDialog.HorizontalAlignmentIndex",
				                                                           horizontalAlignmentComboBox.SelectedIndex.ToString(CultureInfo.InvariantCulture));
				ExternalInformationProvider.SavePerUserPerWorkstationValue(StoreID +
				                                                           @"HtmlEditorTableNewDialog.VerticalAlignmentIndex",
				                                                           verticalAlignmentComboBox.SelectedIndex.ToString(CultureInfo.InvariantCulture));
			}
		}

		private string calculateHorizontalAlignment()
		{
			string a;

			var p =
				(Tuple<string, HtmlEditorCellPropertiesForm.HorizontalAlignmentType>)
					horizontalAlignmentComboBox.SelectedItem;

			switch (p.Item2)
			{
				default:
					a = string.Empty;
					break;
				case HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Center:
					a = @"center";
					break;
				case HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Left:
					a = @"left";
					break;
				case HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Right:
					a = @"right";
					break;
				case HtmlEditorCellPropertiesForm.HorizontalAlignmentType.Justify:
					a = @"justify";
					break;
			}

			if (string.IsNullOrEmpty(a))
			{
				return string.Empty;
			}
			else
			{
				return string.Format(@" align=""{0}""", a);
			}
		}

		private string calculateVerticalAlignment()
		{
			string a;

			var p =
				(Tuple<string, HtmlEditorCellPropertiesForm.VerticalAlignmentType>)
					verticalAlignmentComboBox.SelectedItem;

			switch (p.Item2)
			{
				default:
					a = string.Empty;
					break;
				case HtmlEditorCellPropertiesForm.VerticalAlignmentType.BaseLine:
					a = @"baseline";
					break;
				case HtmlEditorCellPropertiesForm.VerticalAlignmentType.Bottom:
					a = @"bottom";
					break;
				case HtmlEditorCellPropertiesForm.VerticalAlignmentType.Top:
					a = @"top";
					break;
				case HtmlEditorCellPropertiesForm.VerticalAlignmentType.Middle:
					a = @"middle";
					break;
			}

			return string.IsNullOrEmpty(a) ? string.Empty : string.Format(@" valign=""{0}""", a);
		}

		private void rowsUpDown_ValueChanged(object sender, EventArgs e)
		{
			updateUI();
		}

		private void columnsUpDown_ValueChanged(object sender, EventArgs e)
		{
			updateUI();
		}

		private void updateUI()
		{
			buttonOK.Enabled = rowsUpDown.Value > 0 && columnsUpDown.Value > 0;
		}
	}
}