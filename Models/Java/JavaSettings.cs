using Microsoft.Win32;
using System.IO;
using System.Linq;
using Trustie.Models.Interfaces;

namespace Trustie.Models.Java.Security
{
    public class JavaSettings : ISecuritySettings
    {
        public static bool JavaInstalled 
        { 
            get 
            {
                return InstallPath != null; 
            } 
        }

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

        public string[] QuerySites()
        {
            string file = $@"C:\Users\{System.Users.CurrentUser.Name}\{Constants.Files.Java.ExceptionSites}";
            if (!File.Exists(file))
            {
                return null;
            }
            return File.ReadAllLines(file);
        }

        public void AddSite(string site)
        {
            string file = $@"C:\Users\{System.Users.CurrentUser.Name}\{Constants.Files.Java.ExceptionSites}";
            if (!File.Exists(file)) return;

            using (StreamWriter writer = new StreamWriter(file, true))
            {
                writer.WriteLine(site);
            }
        }

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

    }
}
