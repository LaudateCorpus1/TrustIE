using Microsoft.Win32;
using System.IO;
using System.Linq;
using Trustie.Models.Interfaces;

namespace Trustie.Models.Java.Security
{
    public class JavaSettings : ISecuritySettings
    {

        #region Public Members

        /// <summary>
        /// Java is installed
        /// </summary>
        public static bool JavaInstalled 
        { 
            get 
            {
                return InstallPath != null; 
            } 
        }

        /// <summary>
        /// Java installation path 
        /// </summary>
        public static string InstallPath
        {
            get
            {
                string key = Constants.Registry.JavaRuntimeEnvironment.Default;
                if (key == null) return null;

                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(key))
                {
                    if (baseKey == null) return null;
                    string currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                        return homeKey.GetValue("JavaHome").ToString();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Queries sites in Java Exceptions.
        /// </summary>
        /// <returns>Array of sites</returns>
        public string[] QuerySites()
        {
            string file = $@"C:\Users\{System.Users.CurrentUser.Name}\{Constants.Files.Java.ExceptionSites}";
            if (!File.Exists(file)) return null;
            return File.ReadAllLines(file);
        }

        /// <summary>
        /// Adds site to Java Exceptions.
        /// </summary>
        /// <param name="site">Site to add</param>
        public void AddSite(string site)
        {
            string file = $@"C:\Users\{System.Users.CurrentUser.Name}\{Constants.Files.Java.ExceptionSites}";
            if (!File.Exists(file)) return;

            using (StreamWriter writer = new StreamWriter(file, true))
            {
                writer.WriteLine(site);
            }
        }

        /// <summary>
        /// Removes site from Java Exceptions.
        /// </summary>
        /// <param name="site">Site to delete</param>
        public void DeleteSite(string site)
        {
            string file = $@"C:\Users\{System.Users.CurrentUser.Name}\{Constants.Files.Java.ExceptionSites}";
            if (!File.Exists(file)) return;

            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines(file).Where(l => l != site);

            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(file);
            File.Move(tempFile, file);
        }

        #endregion

    }
}
