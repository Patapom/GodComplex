namespace ZetaHtmlEditControl.Code.Helper
{
	#region Public methods.
	// ----------------------------------------------------------------------
    using System;
    using System.Resources;
    using System.Reflection;
    using System.Globalization;
    using System.ComponentModel;

    // ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	[AttributeUsage(
		AttributeTargets.All,
		Inherited = false,
		AllowMultiple = true)]
	internal sealed class LocalizableDescriptionAttribute :
		DescriptionAttribute
	{
	    #region Public methods.
	    // ------------------------------------------------------------------

	    public LocalizableDescriptionAttribute(
	        string description,
	        Type resourcesType)
	        :
	            base(description)
	    {
	        _resourcesType = resourcesType;
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Public properties.
	    // ------------------------------------------------------------------

	    public override string Description
	    {
	        get
	        {
	            if (!_isLocalized)
	            {
	                var resMan =
	                    _resourcesType.InvokeMember(
	                        @"ResourceManager",
	                        BindingFlags.GetProperty | BindingFlags.Static |
	                        BindingFlags.Public | BindingFlags.NonPublic,
	                        null,
	                        null,
	                        new object[] { }) as ResourceManager;

	                var culture =
	                    _resourcesType.InvokeMember(
	                        @"Culture",
	                        BindingFlags.GetProperty | BindingFlags.Static |
	                        BindingFlags.Public | BindingFlags.NonPublic,
	                        null,
	                        null,
	                        new object[] { }) as CultureInfo;

	                _isLocalized = true;

	                if (resMan != null)
	                {
	                    DescriptionValue =
	                        resMan.GetString(DescriptionValue, culture);
	                }
	            }

	            return DescriptionValue;
	        }
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Private variables.
	    // ------------------------------------------------------------------

	    private readonly Type _resourcesType;
	    private bool _isLocalized;

	    // ------------------------------------------------------------------
	    #endregion
	}

	/////////////////////////////////////////////////////////////////////////
}