namespace ZetaHtmlEditControl.Code.Helper
{
	#region Using directives.
	// ----------------------------------------------------------------------
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;

    // ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Class for scaling an image.
	/// </summary>
	internal static class ImageScaler
	{
	    #region Scaling.
	    // ------------------------------------------------------------------

	    /// <summary>
	    /// Scale the given image to the given size.
	    /// Uses a high interpolation mode quality.
	    /// </summary>
	    /// <param name="image">The image.</param>
	    /// <param name="width">The width.</param>
	    /// <param name="height">The height.</param>
	    /// <returns></returns>
	    public static Image ScaleImage(
	        Image image,
	        int width,
	        int height)
	    {
	        // http://stackoverflow.com/questions/249587/high-quality-image-scaling-c-sharp
	        // http://stackoverflow.com/questions/6821261/after-resizing-white-image-gets-gray-border

	        // NO "using" here, because the image is returned (and
	        // would be disposed by "using")!
	        var result = new Bitmap(image, width, height);

	        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

	        using (var g = Graphics.FromImage(result))
	        {
	            g.CompositingQuality = CompositingQuality.HighQuality;
	            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
	            g.SmoothingMode = SmoothingMode.HighQuality;
	            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

	            var ia = new ImageAttributes();
	            ia.SetWrapMode(WrapMode.TileFlipXY);

	            g.DrawImage(
	                image,
	                new Rectangle(
	                    0,
	                    0,
	                    width,
	                    height),
	                0,
	                0,
	                image.Width,
	                image.Height,
	                GraphicsUnit.Pixel,
	                ia);

	            return result;
	        }
	    }

	    // ------------------------------------------------------------------
	    #endregion
	}

	/////////////////////////////////////////////////////////////////////////
}