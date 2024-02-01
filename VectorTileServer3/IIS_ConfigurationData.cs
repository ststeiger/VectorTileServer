
namespace VectorTileServer3
{

    public class IIS_ConfigurationData
    {

        private System.IntPtr m_nativeApplication;

        public string FullApplicationPath;
        public string VirtualApplicationPath;
        public bool WindowsAuthEnabled;
        public bool BasicAuthEnabled;
        public bool AnonymousAuthEnable;


        public IIS_ConfigurationData(System.IntPtr nativeApplication)
        {
            this.m_nativeApplication = nativeApplication;
        }


        public static IIS_ConfigurationData Init()
        {
            IIS_ConfigurationData props = IISHelperModule.GetConfigurationData();
            return props;
        }


        // See also:
        // NetCore7_Test\FirstTestApp\Code\NameValueCollectionHelper.cs
        // https://github.com/dotnet/aspnetcore/blob/c925f99cddac0df90ed0bc4a07ecda6b054a0b02/src/Servers/IIS/IIS/src/NativeMethods.cs
        // https://github.com/latra/BoomerBot/blob/b93db01342c503f726872cffd5be60fbe3e80d15/BoomerBot/Modules/IISHelperModule.cs
        // https://github.com/latra/BoomerBot/tree/b93db01342c503f726872cffd5be60fbe3e80d15
        private static class IISHelperModule
        {

            public static IIS_ConfigurationData GetConfigurationData()
            {
                try 
                {
                    IISConfigurationData props = NativeMethods.HttpGetApplicationProperties();
                    return CopyData(props);
                }
                catch (System.Exception)
                { }

                return null;
            }


            private static IIS_ConfigurationData CopyData(IISConfigurationData props)
            {
                IIS_ConfigurationData cfg = new IIS_ConfigurationData(props.pNativeApplication);

                cfg.FullApplicationPath = props.pwzFullApplicationPath;
                cfg.VirtualApplicationPath = props.pwzVirtualApplicationPath;
                cfg.WindowsAuthEnabled = props.fWindowsAuthEnabled;
                cfg.BasicAuthEnabled = props.fBasicAuthEnabled;
                cfg.AnonymousAuthEnable = props.fAnonymousAuthEnable;

                return cfg;
            }



            public static string GetAppPath()
            {
                return GetContentRoot() ?? System.IO.Directory.GetCurrentDirectory();
            } // End Function GetAppPath 


            public static string GetContentRoot()
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    && NativeMethods.IsAspNetCoreModuleLoaded())
                {
                    IISConfigurationData iisConfigData = NativeMethods.HttpGetApplicationProperties();
                    string contentRoot = iisConfigData.pwzFullApplicationPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);

                    return contentRoot;
                }

                return null;
            } // End Function GetContentRoot 


            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            private struct IISConfigurationData
            {
                public System.IntPtr pNativeApplication;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)]
                public string pwzFullApplicationPath;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)]
                public string pwzVirtualApplicationPath;
                public bool fWindowsAuthEnabled;
                public bool fBasicAuthEnabled;
                public bool fAnonymousAuthEnable;
            }


            private static class NativeMethods
            {
                internal const string AspNetCoreModuleDll = "aspnetcorev2_inprocess.dll";


                [System.Runtime.InteropServices.DllImport("kernel32.dll")]
                private static extern System.IntPtr GetModuleHandle(string lpModuleName);


                [System.Runtime.InteropServices.DllImport(AspNetCoreModuleDll)]
                private static extern int http_get_application_properties(ref IISConfigurationData iiConfigData);

                public static bool IsAspNetCoreModuleLoaded()
                {
                    return GetModuleHandle(AspNetCoreModuleDll) != System.IntPtr.Zero;

                }


                public static IISConfigurationData HttpGetApplicationProperties()
                {
                    IISConfigurationData iisConfigurationData = default(IISConfigurationData);
                    int errorCode = http_get_application_properties(ref iisConfigurationData);

                    if (errorCode != 0)
                    {
                        throw System.Runtime.InteropServices.Marshal.GetExceptionForHR(errorCode);
                    }

                    return iisConfigurationData;
                }

            } // End Class NativeMethods 


        } // End Class IISHelperModule 



    } // End Class IIS_ConfigurationData 


}