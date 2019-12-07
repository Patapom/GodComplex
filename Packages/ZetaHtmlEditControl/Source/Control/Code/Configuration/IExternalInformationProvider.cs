namespace ZetaHtmlEditControl.Code.Configuration
{
    using System.Drawing;

    public interface IExternalInformationProvider
	{
	    Font Font { get; }
        Color? ForeColor { get; }
        Color? ControlBorderColor { get; }

	    void SavePerUserPerWorkstationValue(
			string name,
			string value );

		string RestorePerUserPerWorkstationValue(
			string name,
			string fallBackTo );

		/// <summary>
		/// Return NULL to disable text module support.
		/// </summary>
		TextModuleInfo[] GetTextModules();
	}
}