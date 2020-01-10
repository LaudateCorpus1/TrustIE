using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;

namespace Trustie.Models.System.Users
{
    public static class CurrentUser
    {

        #region Public Members

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets current user name. Method checks owner of currently 
        /// running explorer.exe isntance. That owner is considered to be a current user.
        /// </summary>
        /// <returns>Current user name</returns>
        private static string CurrentUserName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            return username.Split('\\')[1];
        }

        /// <summary>
        /// Gets SID of current user.
        /// </summary>
        /// <returns>Current user SID</returns>
        private static string CurrentUserSID()
        {
            string name = CurrentUserName();
            NTAccount account = new NTAccount(name);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            return sid.ToString();
        }

        #endregion
    }
}
