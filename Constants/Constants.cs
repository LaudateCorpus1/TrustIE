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
        }

        public static class Application
        {
            public static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
            public static readonly string Name = "TrustIE";
        }
    }
}
