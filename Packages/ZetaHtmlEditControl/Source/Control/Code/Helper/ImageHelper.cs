namespace ZetaHtmlEditControl.Code.Helper
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    internal static class ImageHelper
	{
        #region Miscellaneous methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Provides a file-locking-safe alternative to Image.FromFile().
        /// See http://support.microsoft.com/kb/311754/EN-US/ for details.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Returns the loaded image.</returns>
        public static Image LoadImage(
            string filePath)
        {
            using (var fs = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read))
            using (var br = new BinaryReader(
                fs))
            {
                var buffer = new byte[fs.Length];
                br.Read(buffer, 0, (int)fs.Length);

                // The documentation says "...You must keep the stream open 
                // for the lifetime of the Image object...".
                // Therefore read into memory ad keep the memory stream open.
                // I.e. use NO "using" directive here.
                var ms = new MemoryStream(buffer);
                return Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Generic saving function. Correctly handles file extension and
        /// the associated image format.
        /// </summary>
        /// <param name="image">The image to save.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// Since the function could change the file extension, it returns the
        /// newly written path.
        /// </returns>
        public static void SaveImage(Image image, string filePath)
        {
            var initialFilePath = filePath;

            Trace.WriteLine(
                string.Format(
                    @"About to save image to file path '{0}'...",
                    filePath));

            var format =
                GetImageFormatFromFileExtension(
                    Path.GetExtension(filePath));

            if (string.Compare(filePath, initialFilePath, StringComparison.OrdinalIgnoreCase) != 0)
            {
                Trace.WriteLine(
                    string.Format(
                        @"Changed image file path path from '{0}' to '{1}'.",
                        initialFilePath,
                        filePath));
            }

            // --

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Use NO "using" directive here.
            var ms = new MemoryStream();
            image.Save(ms, format);

            using (var fs = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.ReadWrite))
            using (var bw = new BinaryWriter(
                fs))
            {
                var buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);

                bw.Write(buffer);
            }

            Trace.WriteLine(
                string.Format(
                    @"Successfully saved image to file path '{0}'.",
                    filePath));
        }

        private static ImageFormat GetImageFormatFromFileExtension(
            string extension)
        {
            extension = extension.Trim('.').ToLowerInvariant();

            ImageFormat format;
            switch (extension)
            {
                case @"bmp":
                    format = ImageFormat.Bmp;
                    break;

                case @"png":
                    format = ImageFormat.Png;
                    break;

                case @"gif":
                    format = ImageFormat.Gif;
                    break;

                case @"jpg":
                case @"jpeg":
                    format = ImageFormat.Jpeg;
                    break;

                case @"tif":
                case @"tiff":
                    format = ImageFormat.Tiff;
                    break;

                default:
                    Trace.WriteLine(
                        string.Format(
                            @"Unknown file format extension '{0}'. Using PNG instead.",
                            extension));
                    format = ImageFormat.Png;
                    break;
            }

            return format;
        }

        // ------------------------------------------------------------------
        #endregion
	}

	/////////////////////////////////////////////////////////////////////////
}