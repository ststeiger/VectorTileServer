namespace VectorTileServer
{

    public class DownloadsPath
    {
        public static string GetDownloadsFolderPath()
        {
            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return DownloadsPathWindows.GetDownloadsFolderPath();


            string userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string downloadsPath = System.IO.Path.Combine(userProfile, "Downloads");
            return downloadsPath;
        }

    }


    internal class DownloadsPathWindows
    {
        // GUID for the Downloads folder
        private static readonly System.Guid FOLDERID_Downloads = new System.Guid("374DE290-123F-4565-9164-39C4925E467B");

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)]
            System.Guid rfid,
            uint dwFlags,
            System.IntPtr hToken,
            out string pszPath
        );

        internal static string GetDownloadsFolderPath()
        {
            string path = null;
            try
            {
                SHGetKnownFolderPath(FOLDERID_Downloads, 0, System.IntPtr.Zero, out path);
            }
            catch (System.Exception ex)
            {
                // Handle exceptions
                System.Console.WriteLine($"Error getting downloads folder path: {ex.Message}");
            }
            return path;
        } // End Function GetDownloadsFolderPath


    } // End Class DownloadsPath 


} // End Namespace 
