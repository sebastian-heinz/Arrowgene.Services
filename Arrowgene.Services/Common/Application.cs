namespace ArrowgeneServices.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public static class Application
    {

        public static string DirectoryPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().EscapedCodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static void TryCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AppKit::TryCreateDirectory:" + ex.ToString());
            }
        }

        public static string Version
        {
            get
            {
                System.Reflection.Assembly current = System.Reflection.Assembly.GetExecutingAssembly();
                string exe = current.GetModules()[0].FullyQualifiedName;
                FileVersionInfo fi = FileVersionInfo.GetVersionInfo(exe);
                return fi.FileVersion;
            }
        }

    }
}
