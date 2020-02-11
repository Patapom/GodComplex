using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Brain2 {

	public interface IFicheEditor {
		/// <summary>
		/// Gets the form used by the editor
		/// </summary>
		Form	EditorForm	{ get; }

		/// <summary>
		/// Gets or sets the fiche that needs to be edited
		/// </summary>
		Fiche	EditedFiche { get; set; }
	}
}
