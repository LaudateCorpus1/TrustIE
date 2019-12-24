using System;
using System.IO;
using System.Management;
using System.Security.Principal;

namespace Trustie.Models.System.Users.Queries
{
    public static class CurrentUser
    {
        public static string Name
        {
            get
            {
                return CurrentUserName();
            }
        }

        public static string SID
        {
            get
            {
                return CurrentUserSID();
            }
        }

        private static string CurrentUserName()
        {
            SelectQuery query = new SelectQuery(@"Select * from Win32_Process");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject Process in searcher.Get())
                {
                    if (Process["ExecutablePath"] != null &&
                        string.Equals(Path.GetFileName(Process["ExecutablePath"].ToString()), "explorer.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] OwnerInfo = new string[2];
                        Process.InvokeMethod("GetOwner", OwnerInfo);

                        return OwnerInfo[0];
                    }
                }
            }
            return string.Empty;
        }

        private static string CurrentUserSID()
        {
            string name = CurrentUserName();
            NTAccount account = new NTAccount(name);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            return sid.ToString();
        }
    }
}
