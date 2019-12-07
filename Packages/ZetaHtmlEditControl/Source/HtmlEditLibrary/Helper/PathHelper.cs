namespace ZetaHtmlEditControl.Code.Helper
{
	#region Using directives.
	// ----------------------------------------------------------------------
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    // ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	internal static class PathHelper
	{
	    #region HTML- and URL-encoding/-decoding.
	    // ------------------------------------------------------------------

	    public enum UrlEncoding
	    {
	        #region Enum members.

	        /// <summary>
	        /// Escape all unsafe characters.
	        /// </summary>
	        XAlphas,

	        /// <summary>
	        /// As XAlphas but allows '+'.
	        /// </summary>
	        XPAlphas,

	        /// <summary>
	        /// As XPAlphas but allows '/'.
	        /// </summary>
	        Path,

	        /// <summary>
	        /// As Path but allows ':'.
	        /// </summary>
	        DosFile

	        #endregion
	    }

	    public static string UrlEncode(
	        string text)
	    {
	        return UrlEncode(
	            text,
	            UrlEncoding.XAlphas,
	            Encoding.UTF8);
	    }

	    public static string UrlEncode(
	        string s,
	        UrlEncoding urlEncoding)
	    {
	        return UrlEncode(
	            s,
	            urlEncoding,
	            Encoding.UTF8);
	    }

	    public static string UrlEncode(
	        string s,
	        UrlEncoding urlEncoding,
	        Encoding e)
	    {
	        if (s == null)
	        {
	            return null;
	        }
	        else if (s.Length <= 0)
	        {
	            return string.Empty;
	        }
	        else
	        {
	            var bytes = e.GetBytes(s);
	            return Encoding.ASCII.GetString(
	                urlEncodeToBytes(
	                    bytes,
	                    0, bytes.Length,
	                    urlEncoding));
	        }
	    }

	    public static string UrlDecode(
	        string s)
	    {
	        return UrlDecode(s, Encoding.UTF8);
	    }

	    public static string UrlDecode(
	        string s,
	        Encoding e)
	    {
	        if (null == s)
	        {return null;}

	        if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
	        {	return s;}

	        if (e == null)
	        {	e = Encoding.UTF8;}

	        var output = new StringBuilder();
	        long len = s.Length;
	        const NumberStyles hexa = NumberStyles.HexNumber;
	        using (var bytes = new MemoryStream())
	        {
	            for (var i = 0; i < len; i++)
	            {
	                if (s[i] == '%' && i + 2 < len)
	                {
	                    if (s[i + 1] == 'u' && i + 5 < len)
	                    {
	                        if (bytes.Length > 0)
	                        {
	                            output.Append(getChars(bytes, e));
	                            bytes.SetLength(0);
	                        }
	                        output.Append((char)Int32.Parse(s.Substring(i + 2, 4), hexa));
	                        i += 5;
	                    }
	                    else
	                    {
	                        bytes.WriteByte((byte)Int32.Parse(s.Substring(i + 1, 2), hexa));
	                        i += 2;
	                    }
	                    continue;
	                }

	                if (bytes.Length > 0)
	                {
	                    output.Append(getChars(bytes, e));
	                    bytes.SetLength(0);
	                }

	                output.Append(s[i] == '+' ? ' ' : s[i]);
	            }

	            if (bytes.Length > 0)
	            {
	                output.Append(getChars(bytes, e));
	            }

	            return output.ToString();
	        }
	    }

	    public static string HtmlEncode(
	        string s)
	    {
	        if (s == null)
	        {
	            return null;
	        }
	        else
	        {
	            var output = new StringBuilder();

	            foreach (var c in s)
	            {
	                switch (c)
	                {
	                    case '&':
	                        output.Append(@"&amp;");
	                        break;
	                    case '>':
	                        output.Append(@"&gt;");
	                        break;
	                    case '<':
	                        output.Append(@"&lt;");
	                        break;
	                    case '"':
	                        output.Append(@"&quot;");
	                        break;
	                    default:
	                        if (c > 128)
	                        {
	                            output.Append(@"&#");
	                            output.Append(((int)c).ToString());
	                            output.Append(@";");
	                        }
	                        else
	                            output.Append(c);
	                        break;
	                }
	            }
	            return output.ToString();
	        }
	    }

	    public static string HtmlDecode(
	        string s)
	    {
	        if (s == null)
	        {
	            throw new ArgumentNullException(@"s");
	        }
	        else if (s.IndexOf('&') == -1)
	        {
	            return s;
	        }
	        else
	        {
	            var insideEntity = false; // used to indicate that we are in a potential entity
	            var entity = String.Empty;
	            var output = new StringBuilder();
	            var len = s.Length;

	            for (var i = 0; i < len; i++)
	            {
	                var c = s[i];
	                switch (c)
	                {
	                    case '&':
	                        output.Append(entity);
	                        entity = @"&";
	                        insideEntity = true;
	                        break;
	                    case ';':
	                        if (!insideEntity)
	                        {
	                            output.Append(c);
	                            break;
	                        }

	                        entity += c;
	                        var length = entity.Length;
	                        if (length >= 2 && entity[1] == '#' && entity[2] != ';')
	                        {
	                            entity = ((char)Int32.Parse(entity.Substring(2, entity.Length - 3))).ToString();
	                        }
	                        else if (length > 1 && entities.ContainsKey(entity.Substring(1, entity.Length - 2)))
	                        {
	                            entity = entities[entity.Substring(1, entity.Length - 2)].ToString();
	                        }
	                        output.Append(entity);
	                        entity = String.Empty;
	                        insideEntity = false;
	                        break;
	                    default:
	                        if (insideEntity)
	                        {
	                            entity += c;
	                        }
	                        else
	                        {
	                            output.Append(c);
	                        }
	                        break;
	                }
	            }
	            output.Append(entity);
	            return output.ToString();
	        }
	    }

	    /// <summary>
	    /// Converts a windows file path 
	    /// (with drive letter or UNC) to a "file://"-URL.
	    /// </summary>
	    public static string ConvertFilePathToFileUrl(
	        string filePath)
	    {
	        var fileUrl = filePath;
	        fileUrl = fileUrl.Replace(@"\", @"/");

	        fileUrl = UrlEncode(
	            fileUrl,
	            UrlEncoding.DosFile);

	        fileUrl = fileUrl.TrimStart('/');

	        if (IsUncPath(filePath))
	        {
	            fileUrl = @"file://" + fileUrl;
	        }
	        else if (IsDriveLetterPath(filePath))
	        {
	            fileUrl = @"file:///" + fileUrl;
	        }
	        else
	        {
	            fileUrl = @"file:///" + fileUrl;
	        }

	        return fileUrl;
	    }

	    public static string ConvertFileUrlToFilePath(
	        string fileUrl)
	    {
	        const string prefixA = @"file:///";
	        const string prefixB = @"file://";

	        string filePath;

	        if (fileUrl.IndexOf(prefixA) == 0)
	        {
	            filePath = UrlDecode(fileUrl.Substring(prefixA.Length));
	        }
	        else if (fileUrl.IndexOf(prefixB) == 0)
	        {
	            filePath = UrlDecode(fileUrl.Substring(prefixB.Length));
	            filePath = @"\\" + filePath;
	        }
	        else
	        {
	            filePath = UrlDecode(fileUrl);
	        }

	        filePath = filePath.Replace(@"/", @"\");

	        return filePath;
	    }

	    public static bool IsDriveLetterPath(
	        string filePath)
	    {
	        if (string.IsNullOrEmpty(filePath))
	        {
	            return false;
	        }
	        else
	        {
	            return filePath.IndexOf(':') == 1;
	        }
	    }

	    public static bool IsUncPath(
	        string filePath)
	    {
	        if (string.IsNullOrEmpty(filePath))
	        {
	            return false;
	        }
	        else
	        {
	            return
	                ConvertForwardSlashsToBackSlashs(
	                    filePath).StartsWith(@"\\") &&
	                !string.IsNullOrEmpty(GetShare(filePath));
	        }
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Miscellaneous function.
	    // ------------------------------------------------------------------

	    public static string SetBackSlashEnd(
	        string path,
	        bool setSlash)
	    {
	        return setSlashEnd(path, setSlash, '\\');
	    }

	    public static string SetForwardSlashEnd(
	        string path,
	        bool setSlash)
	    {
	        return setSlashEnd(path, setSlash, '/');
	    }

	    public static string SetBackSlashBegin(
	        string path,
	        bool setSlash)
	    {
	        return setSlashBegin(path, setSlash, '\\');
	    }

	    public static string SetForwardSlashBegin(
	        string path,
	        bool setSlash)
	    {
	        return setSlashBegin(path, setSlash, '/');
	    }

	    public static string GetParentPath(
	        string text)
	    {
	        if (text == null)
	        {
	            return null;
	        }
	        else
	        {
	            return Path.GetFullPath(Path.Combine(text, @".."));
	        }
	    }

	    /// <summary>
	    /// Get a temporary file path with file extension ".tmp". 
	    /// The file will NOT be created.
	    /// </summary>
	    public static FileInfo GetTempFileName()
	    {
	        return GetTempFileName(@"tmp");
	    }

	    /// <summary>
	    /// Get a temporary file path with the given file extension. 
	    /// The file will NOT be created.
	    /// </summary>
	    public static FileInfo GetTempFileName(
	        string extension)
	    {
	        if (string.IsNullOrEmpty(extension))
	        {
	            extension = @"tmp";
	        }

	        extension = extension.Trim('.');

	        string tempFolderPath = Path.GetTempPath();
	        string tempFileName = Guid.NewGuid().ToString(@"N");

	        return new FileInfo(Combine(
	            tempFolderPath,
	            tempFileName + @"." + extension));
	    }

	    public static string ConvertBackSlashsToForwardSlashs(
	        string text)
	    {
	        return text == null ? null : text.Replace(@"\", @"/");
	    }

	    public static string ConvertForwardSlashsToBackSlashs(
	        string text)
	    {
	        return text == null ? null : text.Replace(@"/", @"\");
	    }

	    /// <summary>
	    /// Check whether a given path contains an absolute or relative path.
	    /// No disk-access is performed, only the syntax of the given string
	    /// is checked.
	    /// </summary>
	    /// <param name="path">The path to check.</param>
	    /// <returns>Returns TRUE if the given path is an absolute path,
	    /// returns FALSE if the given path is a relative path.</returns>
	    public static bool IsAbsolutePath(
	        string path)
	    {
	        path = path.Replace('/', '\\');

	        if (path.Length < 2)
	        {
	            return false;
	        }
	        else if (path.Substring(0, 2) == @"\\")
	        {
	            // UNC.
	            return IsUncPath(path);
	        }
	        else if (path.Substring(1, 1) == @":")
	        {
	            // "C:"
	            return IsDriveLetterPath(path);
	        }
	        else
	        {
	            return false;
	        }
	    }

	    /// <summary>
	    /// Makes 'path' an absolute path, based on 'basePath'.
	    /// If the given path is already an absolute path, the path
	    /// is returned unmodified.
	    /// </summary>
	    /// <param name="pathToMakeAbsolute">The path to make absolute.</param>
	    /// <param name="basePathToWhichToMakeAbsoluteTo">The base path to use when making an
	    /// absolute path.</param>
	    /// <returns>Returns the absolute path.</returns>
	    public static string GetAbsolutePath(
	        string pathToMakeAbsolute,
	        string basePathToWhichToMakeAbsoluteTo)
	    {
	        if (IsAbsolutePath(pathToMakeAbsolute))
	        {
	            return pathToMakeAbsolute;
	        }
	        else
	        {
	            return Path.GetFullPath(
	                Path.Combine(
	                    basePathToWhichToMakeAbsoluteTo,
	                    pathToMakeAbsolute));
	        }
	    }

	    /// <summary>
	    /// Makes a path relative to another.
	    /// (i.e. what to type in a "cd" command to get from
	    /// the PATH1 folder to PATH2). works like e.g. developer studio,
	    /// when you add a file to a project: there, only the relative
	    /// path of the file to the project is stored, too.
	    /// e.g.:
	    /// path1  = "c:\folder1\folder2\folder4\"
	    /// path2  = "c:\folder1\folder2\folder3\file1.txt"
	    /// result = "..\folder3\file1.txt"
	    /// </summary>
	    /// <param name="pathToWhichToMakeRelativeTo">The path to which to make relative to.</param>
	    /// <param name="pathToMakeRelative">The path to make relative.</param>
	    /// <returns>Returns the relative path, IF POSSIBLE. 
	    /// If not possible (i.e. no same parts in PATH2 and the PATH1), 
	    /// returns the complete PATH2.</returns>
	    public static string GetRelativePath(
	        string pathToWhichToMakeRelativeTo,
	        string pathToMakeRelative)
	    {
	        if (string.IsNullOrEmpty(pathToWhichToMakeRelativeTo) ||
	            string.IsNullOrEmpty(pathToMakeRelative))
	        {
	            return pathToMakeRelative;
	        }
	        else
	        {
	            string o = pathToWhichToMakeRelativeTo.ToLower().Replace('/', '\\').TrimEnd('\\');
	            string t = pathToMakeRelative.ToLower().Replace('/', '\\');

	            // --
	            // Handle special cases for Driveletters and UNC shares.

	            string td = GetDriveOrShare(t);
	            string od = GetDriveOrShare(o);

	            td = td.Trim();
	            td = td.Trim('\\', '/');

	            od = od.Trim();
	            od = od.Trim('\\', '/');

	            // Different drive or share, i.e. nothing common, skip.
	            if (td != od)
	            {
	                return pathToMakeRelative;
	            }
	            else
	            {
	                int ol = o.Length;
	                int tl = t.Length;

	                // compare each one, until different.
	                int pos = 0;
	                while (pos < ol && pos < tl && o[pos] == t[pos])
	                {
	                    pos++;
	                }
	                if (pos < ol)
	                {
	                    pos--;
	                }

	                // after comparison, make normal (i.e. NOT lowercase) again.
	                t = pathToMakeRelative;

	                // --

	                // noting in common.
	                if (pos <= 0)
	                {
	                    return t;
	                }
	                else
	                {
	                    // If not matching at a slash-boundary, navigate back until slash.
	                    if (!(pos == ol || o[pos] == '\\' || o[pos] == '/'))
	                    {
	                        while (pos > 0 && (o[pos] != '\\' && o[pos] != '/'))
	                        {
	                            pos--;
	                        }
	                    }

	                    // noting in common.
	                    if (pos <= 0)
	                    {
	                        return t;
	                    }
	                    else
	                    {
	                        // --
	                        // grab and split the reminders.

	                        string oRemaining = o.Substring(pos);
	                        oRemaining = oRemaining.Trim('\\', '/');

	                        // Count how many folders are following in 'path1'.
	                        // Count by splitting.
	                        string[] oRemainingParts = oRemaining.Split('\\');

	                        string tRemaining = t.Substring(pos);
	                        tRemaining = tRemaining.Trim('\\', '/');

	                        // --

	                        string result = string.Empty;

	                        // Path from path1 to common root.
	                        foreach (string oRemainingPart in oRemainingParts)
	                        {
	                            if (!string.IsNullOrEmpty(oRemainingPart))
	                            {
	                                result += @"..\";
	                            }
	                        }

	                        // And up to 'path2'.
	                        result += tRemaining;

	                        // --

	                        return result;
	                    }
	                }
	            }
	        }
	    }

	    /// <summary>
	    /// A "less intelligent" Combine (in contrast to to Path.Combine).
	    /// </summary>
	    public static string Combine(
	        string path1,
	        string path2)
	    {
	        if (string.IsNullOrEmpty(path1))
	        {
	            return path2;
	        }
	        else if (string.IsNullOrEmpty(path2))
	        {
	            return path1;
	        }
	        else
	        {
	            path1 = path1.TrimEnd('\\', '/').Replace('/', '\\');
	            path2 = path2.TrimStart('\\', '/').Replace('/', '\\');

	            return string.Format(@"{0}\{1}", path1, path2);
	        }
	    }

	    /// <summary>
	    /// A "less intelligent" Combine (in contrast to to Path.Combine).
	    /// </summary>
	    public static string Combine(
	        string path1,
	        string path2,
	        string path3,
	        params string[] paths)
	    {
	        string resultPath = Combine(path1, path2);
	        resultPath = Combine(resultPath, path3);

	        if (paths != null)
	        {
	            foreach (string path in paths)
	            {
	                resultPath = Combine(resultPath, path);
	            }
	        }

	        return resultPath;
	    }

	    /// <summary>
	    /// A "less intelligent" Combine (in contrast to to Path.Combine).
	    /// For paths with forward slash.
	    /// </summary>
	    public static string CombineVirtual(
	        string path1,
	        string path2)
	    {
	        if (string.IsNullOrEmpty(path1))
	        {
	            return path2;
	        }
	        else if (string.IsNullOrEmpty(path2))
	        {
	            return path1;
	        }
	        else
	        {
	            // Avoid removing too much "/", so that "file://" still
	            // stays "file://" and does not become "file:/".
	            // (The same applies for other protocols.

	            path1 = path1.Replace('\\', '/');
	            if (path1[path1.Length - 1] != '/')
	            {
	                path1 += @"/";
	            }

	            path2 = path2.Replace('\\', '/');

	            // Do allow "file://" + "/C:/..." to really form "file:///C:/...",
	            // with three slashes.
	            if (path2.Length >= 3)
	            {
	                if (path2[0] == '/' && path2[2] == ':' && char.IsLetter(path2[1]))
	                {
	                    // Is OK to have a leading slash.
	                }
	                else
	                {
	                    path2 = path2.TrimStart('/', '\\');
	                }
	            }
	            else
	            {
	                path2 = path2.TrimStart('/', '\\');
	            }

	            return path1 + path2;
	        }
	    }

	    /// <summary>
	    /// A "less intelligent" Combine (in contrast to to Path.Combine).
	    /// For paths with forward slash.
	    /// </summary>
	    public static string CombineVirtual(
	        string path1,
	        string path2,
	        string path3,
	        params string[] paths)
	    {
	        string resultPath = CombineVirtual(path1, path2);
	        resultPath = CombineVirtual(resultPath, path3);

	        if (paths != null)
	        {
	            foreach (string path in paths)
	            {
	                resultPath = CombineVirtual(resultPath, path);
	            }
	        }

	        return resultPath;
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Splitting a path into different parts.
	    // ------------------------------------------------------------------

	    /// <summary>
	    /// Checks for the drive part in a given string.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Example:  "C:\Team\Text\Test.Txt" would return "C:".
	    /// </remarks>
	    public static string GetDrive(
	        string path)
	    {
	        if (string.IsNullOrEmpty(path))
	        {
	            return path;
	        }
	        else
	        {
	            path = ConvertForwardSlashsToBackSlashs(path);

	            var colonPos = path.IndexOf(':');
	            var slashPos = path.IndexOf('\\');

	            if (colonPos <= 0)
	            {
	                return string.Empty;
	            }
	            else
	            {
	                if (slashPos < 0 || slashPos > colonPos)
	                {
	                    return path.Substring(0, colonPos + 1);
	                }
	                else
	                {
	                    return string.Empty;
	                }
	            }
	        }
	    }

	    /// <summary>
	    /// Retrieves the share in a given string.
	    /// </summary>
	    /// <param name="path">The path to retrieve the share from.</param>
	    /// <returns>Returns the share or an empty string if not found.</returns>
	    /// <remarks>
	    /// Example: "\\Server\C\Team\Text\Test.Txt" would return "\\Server\C".
	    /// -
	    /// Please note_: Searches until the last backslash (including).
	    /// If none is present, the share will not be detected. The schema of
	    /// a share looks like: "\\Server\Share\Dir1\Dir2\Dir3". The backslash
	    /// after "Share" MUST be present to be detected successfully as a share.
	    /// </remarks>
	    public static string GetShare(
	        string path)
	    {
	        if (string.IsNullOrEmpty(path))
	        {
	            return path;
	        }
	        else
	        {
	            var str = path;

	            // Nach Doppel-Slash suchen.
	            // Kann z.B. "\\server\share\" sein,
	            // aber auch "http:\\www.xyz.com\".
	            const string dblslsh = @"\\";
	            var n = str.IndexOf(dblslsh);
	            if (n < 0)
	            {
	                return string.Empty;
	            }
	            else
	            {
	                // Übernehme links von Doppel-Slash alles in Rückgabe
	                // (inkl. Doppel-Slash selbst).
	                var ret = str.Substring(0, n + dblslsh.Length);
	                str = str.Remove(0, n + dblslsh.Length);

	                // Jetzt nach Slash nach Server-Name suchen.
	                // Dieser Slash darf nicht unmittelbar nach den 2 Anfangsslash stehen.
	                n = str.IndexOf('\\');
	                if (n <= 0)
	                {
	                    return string.Empty;
	                }
	                else
	                {
	                    // Wiederum übernehmen in Rückgabestring.
	                    ret += str.Substring(0, n + 1);
	                    str = str.Remove(0, n + 1);

	                    // Jetzt nach Slash nach Share-Name suchen.
	                    // Dieser Slash darf ebenfalls nicht unmittelbar 
	                    // nach dem jetzigen Slash stehen.
	                    n = str.IndexOf('\\');
	                    if (n < 0)
	                    {
	                        n = str.Length;
	                    }
	                    else if (n == 0)
	                    {
	                        return string.Empty;
	                    }

	                    // Wiederum übernehmen in Rückgabestring, 
	                    // aber ohne letzten Slash.
	                    ret += str.Substring(0, n);

	                    // The last item must not be a slash.
	                    if (ret[ret.Length - 1] == '\\')
	                    {
	                        return string.Empty;
	                    }
	                    else
	                    {
	                        return ret;
	                    }
	                }
	            }
	        }
	    }

	    /// <summary>
	    /// Searches for drive or share.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    public static string GetDriveOrShare(
	        string path)
	    {
	        if (string.IsNullOrEmpty(path))
	        {
	            return path;
	        }
	        else
	        {
	            if (!string.IsNullOrEmpty(GetDrive(path)))
	            {
	                return GetDrive(path);
	            }
	            else if (!string.IsNullOrEmpty(GetShare(path)))
	            {
	                return GetShare(path);
	            }
	            else
	            {
	                return string.Empty;
	            }
	        }
	    }

	    /// <summary>
	    /// Retrieves the path part in a given string (without the drive or share).
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Example: "C:\Team\Text\Test.Txt" would return "\Test\Text\".
	    /// -
	    /// Please note_: Searches until the last backslash (including).
	    /// If not present, the path is not treated as a directory.
	    /// (E.g.. "C:\Test\MyDir" would return "\Test" only as the directory).
	    /// </remarks>
	    public static string GetDirectory(
	        string path)
	    {
	        if (string.IsNullOrEmpty(path))
	        {
	            return path;
	        }
	        else
	        {
	            var driveOrShare = GetDriveOrShare(path);

	            var dir = Path.GetDirectoryName(path);

	            Debug.Assert(
	                string.IsNullOrEmpty(driveOrShare) ||
	                dir.StartsWith(driveOrShare),

	                string.Format(
	                    @"Variable 'dir' ('{0}') must start with drive or share '{1}'.",
	                    dir,
	                    driveOrShare));

	            if (!string.IsNullOrEmpty(driveOrShare) &&
	                dir.StartsWith(driveOrShare))
	            {
	                return dir.Substring(driveOrShare.Length);
	            }
	            else
	            {
	                return dir;
	            }
	        }
	    }

	    /// <summary>
	    /// Retrieves the file name without the extension in a given string.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Examples: 
	    /// "C:\Team\Text\Test.Txt" would return "Test".
	    /// "C:\Team\Text\Test" would also return "Test".
	    /// </remarks>
	    public static string GetNameWithoutExtension(
	        string path)
	    {
	        return string.IsNullOrEmpty(path)
	            ? path :
	            Path.GetFileNameWithoutExtension(path);
	    }

	    /// <summary>
	    /// Retrieves the file name with the extension in a given string.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Examples: 
	    /// "C:\Team\Text\Test.Txt" would return "Test.Txt".
	    /// "C:\Team\Text\Test" would return "Test".
	    /// </remarks>
	    public static string GetNameWithExtension(
	        FileInfo path)
	    {
	        return GetNameWithExtension(path.FullName);
	    }

	    /// <summary>
	    /// Retrieves the file name with the extension in a given string.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Examples: 
	    /// "C:\Team\Text\Test.Txt" would return "Test.Txt".
	    /// "C:\Team\Text\Test" would return "Test".
	    /// </remarks>
	    public static string GetNameWithExtension(
	        string path)
	    {
	        return string.IsNullOrEmpty(path) ? path : Path.GetFileName(path);
	    }

	    /// <summary>
	    /// Retrieves the file extension in a given string. Including the dot.
	    /// </summary>
	    /// <param name="path"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Examples: 
	    /// "C:\Team\Text\Test.Txt" would return ".Txt".
	    /// "C:\Team\Text\Test." would return ".".
	    /// "C:\Team\Text\Test" would return "".
	    /// </remarks>
	    public static string GetExtension(
	        string path)
	    {
	        return string.IsNullOrEmpty(path) ? path : Path.GetExtension(path);
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Private helper for HTML- and URL-encoding/-decoding.
	    // ------------------------------------------------------------------

	    private static Hashtable _entities;
	    private static readonly object Lock = new object();
	    private static readonly char[] HexChars = @"0123456789abcdef".ToCharArray();

	    private static Hashtable entities
	    {
	        get
	        {
	            lock (Lock)
	            {
	                if (_entities == null)
	                {
	                    initEntities();
	                }

	                return _entities;
	            }
	        }
	    }

	    private static char[] getChars(MemoryStream b, Encoding e)
	    {
	        return e.GetChars(b.GetBuffer(), 0, (int)b.Length);
	    }

	    private static void initEntities()
	    {
	        // Build the hash table of HTML entity references.  
	        // This list comes from the HTML 4.01 W3C recommendation.
	        _entities = new Hashtable
	        {
	            {@"nbsp", '\u00A0'},
	            {@"iexcl", '\u00A1'},
	            {@"cent", '\u00A2'},
	            {@"pound", '\u00A3'},
	            {@"curren", '\u00A4'},
	            {@"yen", '\u00A5'},
	            {@"brvbar", '\u00A6'},
	            {@"sect", '\u00A7'},
	            {@"uml", '\u00A8'},
	            {@"copy", '\u00A9'},
	            {@"ordf", '\u00AA'},
	            {@"laquo", '\u00AB'},
	            {@"not", '\u00AC'},
	            {@"shy", '\u00AD'},
	            {@"reg", '\u00AE'},
	            {@"macr", '\u00AF'},
	            {@"deg", '\u00B0'},
	            {@"plusmn", '\u00B1'},
	            {@"sup2", '\u00B2'},
	            {@"sup3", '\u00B3'},
	            {@"acute", '\u00B4'},
	            {@"micro", '\u00B5'},
	            {@"para", '\u00B6'},
	            {@"middot", '\u00B7'},
	            {@"cedil", '\u00B8'},
	            {@"sup1", '\u00B9'},
	            {@"ordm", '\u00BA'},
	            {@"raquo", '\u00BB'},
	            {@"frac14", '\u00BC'},
	            {@"frac12", '\u00BD'},
	            {@"frac34", '\u00BE'},
	            {@"iquest", '\u00BF'},
	            {@"Agrave", '\u00C0'},
	            {@"Aacute", '\u00C1'},
	            {@"Acirc", '\u00C2'},
	            {@"Atilde", '\u00C3'},
	            {@"Auml", '\u00C4'},
	            {@"Aring", '\u00C5'},
	            {@"AElig", '\u00C6'},
	            {@"Ccedil", '\u00C7'},
	            {@"Egrave", '\u00C8'},
	            {@"Eacute", '\u00C9'},
	            {@"Ecirc", '\u00CA'},
	            {@"Euml", '\u00CB'},
	            {@"Igrave", '\u00CC'},
	            {@"Iacute", '\u00CD'},
	            {@"Icirc", '\u00CE'},
	            {@"Iuml", '\u00CF'},
	            {@"ETH", '\u00D0'},
	            {@"Ntilde", '\u00D1'},
	            {@"Ograve", '\u00D2'},
	            {@"Oacute", '\u00D3'},
	            {@"Ocirc", '\u00D4'},
	            {@"Otilde", '\u00D5'},
	            {@"Ouml", '\u00D6'},
	            {@"times", '\u00D7'},
	            {@"Oslash", '\u00D8'},
	            {@"Ugrave", '\u00D9'},
	            {@"Uacute", '\u00DA'},
	            {@"Ucirc", '\u00DB'},
	            {@"Uuml", '\u00DC'},
	            {@"Yacute", '\u00DD'},
	            {@"THORN", '\u00DE'},
	            {@"szlig", '\u00DF'},
	            {@"agrave", '\u00E0'},
	            {@"aacute", '\u00E1'},
	            {@"acirc", '\u00E2'},
	            {@"atilde", '\u00E3'},
	            {@"auml", '\u00E4'},
	            {@"aring", '\u00E5'},
	            {@"aelig", '\u00E6'},
	            {@"ccedil", '\u00E7'},
	            {@"egrave", '\u00E8'},
	            {@"eacute", '\u00E9'},
	            {@"ecirc", '\u00EA'},
	            {@"euml", '\u00EB'},
	            {@"igrave", '\u00EC'},
	            {@"iacute", '\u00ED'},
	            {@"icirc", '\u00EE'},
	            {@"iuml", '\u00EF'},
	            {@"eth", '\u00F0'},
	            {@"ntilde", '\u00F1'},
	            {@"ograve", '\u00F2'},
	            {@"oacute", '\u00F3'},
	            {@"ocirc", '\u00F4'},
	            {@"otilde", '\u00F5'},
	            {@"ouml", '\u00F6'},
	            {@"divide", '\u00F7'},
	            {@"oslash", '\u00F8'},
	            {@"ugrave", '\u00F9'},
	            {@"uacute", '\u00FA'},
	            {@"ucirc", '\u00FB'},
	            {@"uuml", '\u00FC'},
	            {@"yacute", '\u00FD'},
	            {@"thorn", '\u00FE'},
	            {@"yuml", '\u00FF'},
	            {@"fnof", '\u0192'},
	            {@"Alpha", '\u0391'},
	            {@"Beta", '\u0392'},
	            {@"Gamma", '\u0393'},
	            {@"Delta", '\u0394'},
	            {@"Epsilon", '\u0395'},
	            {@"Zeta", '\u0396'},
	            {@"Eta", '\u0397'},
	            {@"Theta", '\u0398'},
	            {@"Iota", '\u0399'},
	            {@"Kappa", '\u039A'},
	            {@"Lambda", '\u039B'},
	            {@"Mu", '\u039C'},
	            {@"Nu", '\u039D'},
	            {@"Xi", '\u039E'},
	            {@"Omicron", '\u039F'},
	            {@"Pi", '\u03A0'},
	            {@"Rho", '\u03A1'},
	            {@"Sigma", '\u03A3'},
	            {@"Tau", '\u03A4'},
	            {@"Upsilon", '\u03A5'},
	            {@"Phi", '\u03A6'},
	            {@"Chi", '\u03A7'},
	            {@"Psi", '\u03A8'},
	            {@"Omega", '\u03A9'},
	            {@"alpha", '\u03B1'},
	            {@"beta", '\u03B2'},
	            {@"gamma", '\u03B3'},
	            {@"delta", '\u03B4'},
	            {@"epsilon", '\u03B5'},
	            {@"zeta", '\u03B6'},
	            {@"eta", '\u03B7'},
	            {@"theta", '\u03B8'},
	            {@"iota", '\u03B9'},
	            {@"kappa", '\u03BA'},
	            {@"lambda", '\u03BB'},
	            {@"mu", '\u03BC'},
	            {@"nu", '\u03BD'},
	            {@"xi", '\u03BE'},
	            {@"omicron", '\u03BF'},
	            {@"pi", '\u03C0'},
	            {@"rho", '\u03C1'},
	            {@"sigmaf", '\u03C2'},
	            {@"sigma", '\u03C3'},
	            {@"tau", '\u03C4'},
	            {@"upsilon", '\u03C5'},
	            {@"phi", '\u03C6'},
	            {@"chi", '\u03C7'},
	            {@"psi", '\u03C8'},
	            {@"omega", '\u03C9'},
	            {@"thetasym", '\u03D1'},
	            {@"upsih", '\u03D2'},
	            {@"piv", '\u03D6'},
	            {@"bull", '\u2022'},
	            {@"hellip", '\u2026'},
	            {@"prime", '\u2032'},
	            {@"Prime", '\u2033'},
	            {@"oline", '\u203E'},
	            {@"frasl", '\u2044'},
	            {@"weierp", '\u2118'},
	            {@"image", '\u2111'},
	            {@"real", '\u211C'},
	            {@"trade", '\u2122'},
	            {@"alefsym", '\u2135'},
	            {@"larr", '\u2190'},
	            {@"uarr", '\u2191'},
	            {@"rarr", '\u2192'},
	            {@"darr", '\u2193'},
	            {@"harr", '\u2194'},
	            {@"crarr", '\u21B5'},
	            {@"lArr", '\u21D0'},
	            {@"uArr", '\u21D1'},
	            {@"rArr", '\u21D2'},
	            {@"dArr", '\u21D3'},
	            {@"hArr", '\u21D4'},
	            {@"forall", '\u2200'},
	            {@"part", '\u2202'},
	            {@"exist", '\u2203'},
	            {@"empty", '\u2205'},
	            {@"nabla", '\u2207'},
	            {@"isin", '\u2208'},
	            {@"notin", '\u2209'},
	            {@"ni", '\u220B'},
	            {@"prod", '\u220F'},
	            {@"sum", '\u2211'},
	            {@"minus", '\u2212'},
	            {@"lowast", '\u2217'},
	            {@"radic", '\u221A'},
	            {@"prop", '\u221D'},
	            {@"infin", '\u221E'},
	            {@"ang", '\u2220'},
	            {@"and", '\u2227'},
	            {@"or", '\u2228'},
	            {@"cap", '\u2229'},
	            {@"cup", '\u222A'},
	            {@"int", '\u222B'},
	            {@"there4", '\u2234'},
	            {@"sim", '\u223C'},
	            {@"cong", '\u2245'},
	            {@"asymp", '\u2248'},
	            {@"ne", '\u2260'},
	            {@"equiv", '\u2261'},
	            {@"le", '\u2264'},
	            {@"ge", '\u2265'},
	            {@"sub", '\u2282'},
	            {@"sup", '\u2283'},
	            {@"nsub", '\u2284'},
	            {@"sube", '\u2286'},
	            {@"supe", '\u2287'},
	            {@"oplus", '\u2295'},
	            {@"otimes", '\u2297'},
	            {@"perp", '\u22A5'},
	            {@"sdot", '\u22C5'},
	            {@"lceil", '\u2308'},
	            {@"rceil", '\u2309'},
	            {@"lfloor", '\u230A'},
	            {@"rfloor", '\u230B'},
	            {@"lang", '\u2329'},
	            {@"rang", '\u232A'},
	            {@"loz", '\u25CA'},
	            {@"spades", '\u2660'},
	            {@"clubs", '\u2663'},
	            {@"hearts", '\u2665'},
	            {@"diams", '\u2666'},
	            {@"quot", '\u0022'},
	            {@"amp", '\u0026'},
	            {@"lt", '\u003C'},
	            {@"gt", '\u003E'},
	            {@"OElig", '\u0152'},
	            {@"oelig", '\u0153'},
	            {@"Scaron", '\u0160'},
	            {@"scaron", '\u0161'},
	            {@"Yuml", '\u0178'},
	            {@"circ", '\u02C6'},
	            {@"tilde", '\u02DC'},
	            {@"ensp", '\u2002'},
	            {@"emsp", '\u2003'},
	            {@"thinsp", '\u2009'},
	            {@"zwnj", '\u200C'},
	            {@"zwj", '\u200D'},
	            {@"lrm", '\u200E'},
	            {@"rlm", '\u200F'},
	            {@"ndash", '\u2013'},
	            {@"mdash", '\u2014'},
	            {@"lsquo", '\u2018'},
	            {@"rsquo", '\u2019'},
	            {@"sbquo", '\u201A'},
	            {@"ldquo", '\u201C'},
	            {@"rdquo", '\u201D'},
	            {@"bdquo", '\u201E'},
	            {@"dagger", '\u2020'},
	            {@"Dagger", '\u2021'},
	            {@"permil", '\u2030'},
	            {@"lsaquo", '\u2039'},
	            {@"rsaquo", '\u203A'},
	            {@"euro", '\u20AC'}
	        };
	    }

	    private static byte[] urlEncodeToBytes(
	        byte[] bytes,
	        int offset,
	        int count,
	        UrlEncoding urlEncoding)
	    {
	        if (bytes == null)
	            return null;

	        var len = bytes.Length;
	        if (len == 0)
	            return new byte[0];

	        if (offset < 0 || offset >= len)
	        {
	            throw new ArgumentOutOfRangeException(@"offset");
	        }

	        if (count < 0 || count > len - offset)
	        {
	            throw new ArgumentOutOfRangeException(@"count");
	        }

	        // --

	        string additionalSafeChars;
	        switch (urlEncoding)
	        {
	            case UrlEncoding.XAlphas:
	                additionalSafeChars = @"+";
	                break;
	            case UrlEncoding.XPAlphas:
	                additionalSafeChars = @"+/";
	                break;
	            case UrlEncoding.DosFile:
	                additionalSafeChars = @"+/:";
	                break;
	            default:
	                additionalSafeChars = string.Empty;
	                break;
	        }

	        // --

	        using (var result = new MemoryStream())
	        {
	            var end = offset + count;
	            for (var i = offset; i < end; i++)
	            {
	                var c = (char)bytes[i];

	                var isUnsafe =
	                    (c == ' ') || (c < '0' && c != '-' && c != '.') ||
	                    (c < 'A' && c > '9') ||
	                    (c > 'Z' && c < 'a' && c != '_') ||
	                    (c > 'z');

	                if (isUnsafe &&
	                    additionalSafeChars.IndexOf(c) >= 0)
	                {
	                    isUnsafe = false;
	                }

	                if (isUnsafe)
	                {
	                    // An unsafe character, must escape.
	                    result.WriteByte((byte)'%');
	                    var idx = c >> 4;
	                    result.WriteByte((byte)HexChars[idx]);
	                    idx = c & 0x0F;
	                    result.WriteByte((byte)HexChars[idx]);
	                }
	                else
	                {
	                    // A safe character just write.
	                    result.WriteByte((byte)c);
	                }
	            }

	            return result.ToArray();
	        }
	    }

	    // ------------------------------------------------------------------
	    #endregion

	    #region Miscellaneous private helper.
	    // ------------------------------------------------------------------

	    private static string setSlashBegin(
	        string path,
	        bool setSlash,
	        char directorySeparatorChar)
	    {
	        if (setSlash)
	        {
	            if (string.IsNullOrEmpty(path))
	            {
	                return directorySeparatorChar.ToString();
	            }
	            else
	            {
	                if (path[0] == directorySeparatorChar)
	                {
	                    return path;
	                }
	                else
	                {
	                    return directorySeparatorChar + path;
	                }
	            }
	        }
	        else
	        {
	            if (string.IsNullOrEmpty(path))
	            {
	                return path;
	            }
	            else
	            {
	                if (path[0] == directorySeparatorChar)
	                {
	                    return path.Substring(1);
	                }
	                else
	                {
	                    return path;
	                }
	            }
	        }
	    }

	    private static string setSlashEnd(
	        string path,
	        bool setSlash,
	        char directorySeparatorChar)
	    {
	        if (setSlash)
	        {
	            if (string.IsNullOrEmpty(path))
	            {
	                return directorySeparatorChar.ToString();
	            }
	            else
	            {
	                if (path[path.Length - 1] == directorySeparatorChar)
	                {
	                    return path;
	                }
	                else
	                {
	                    return path + directorySeparatorChar;
	                }
	            }
	        }
	        else
	        {
	            if (string.IsNullOrEmpty(path))
	            {
	                return path;
	            }
	            else
	            {
	                if (path[path.Length - 1] == directorySeparatorChar)
	                {
	                    return path.Substring(0, path.Length - 1);
	                }
	                else
	                {
	                    return path;
	                }
	            }
	        }
	    }

	    // ------------------------------------------------------------------
	    #endregion
	}

	/////////////////////////////////////////////////////////////////////////
}