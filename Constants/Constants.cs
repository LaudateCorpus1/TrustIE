using System;

namespace Trustie
{
    public static class Constants
    {
        public static class Registry
        {
            public static class InternetSettings
            {
                public static readonly string Default = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
                public static readonly string Policies = @"Software\Policies\Microsoft\Windows\CurrentVersion\Internet Settings\";

                public static class SiteToZoneAssignmentList
                {
                    public static readonly string ValueName = "ListBox_Support_ZoneMapKey";
                    public static readonly int Value = 1;
                }

                public static class Domains
                {
                    public static readonly string Default = InternetSettings.Default + @"ZoneMap\Domains\";
                    public static readonly string Policies = InternetSettings.Policies + @"ZoneMap\Domains\";
                }
            }

            public static class JavaRuntimeEnvironment
            {
                public static readonly string Default = @"Software\JavaSoft\Java Runtime Environment\";
            }
        }

        public static class Paths
        {
            public static class Java
            {
                public static readonly string Security = @"AppData\LocalLow\Sun\Java\Deployment\security\";
                
            }
        }

        public static class Files
        {
            public static class Java
            {
                public static readonly string ExceptionSites = Paths.Java.Security + "exception.sites";
            }
        }

        public static class Application
        {
            public static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
            public static readonly string Name = "TrustIE";
        }
    }
}
