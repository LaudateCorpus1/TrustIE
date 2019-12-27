using Microsoft.Win32;
using Nager.PublicSuffix;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Trustie.Models.Interfaces;

namespace Trustie.Models.InternetExplorer
{
    public class IESettings : ISecuritySettings
    {

        #region Public Members

        /// <summary>
        /// Contains registry path to the current Domains key.
        /// </summary>
        public RegistryKey Domains { get; set; }

        /// <summary>
        /// Internet Explorer's Security Zone. Default is Trusted Zone.
        /// </summary>
        public SecurityZone SecurityZone { get; set; } = SecurityZone.Trusted;

        #endregion

        #region Constructor

        /// <summary>
        /// The primary constructor assigns a proper Domains key to the Root.
        /// </summary>
        public IESettings()
        {
            Domains = GetDomainsKey();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add site to registry.
        /// </summary>
        /// <param name="s">Site to be added to the registry</param>
        public void AddSite(string s)
        {
            var site = new Site(s);

            // Create rootdomain key in registry
            var rootDomain = Domains.CreateSubKey(site.RootDomain);

            // Subdomain is not defined
            if (string.IsNullOrEmpty(site.SubDomain))
            {
                // Set value for a given zone to rootdomain
                rootDomain.SetValue(site.Protocol, (int)SecurityZone);
            }
            // Subdomain is defined
            else
            {
                // Create subdomain key in registry
                var subDomain = rootDomain.CreateSubKey(site.SubDomain);

                // Set value for a given zone to subdomain
                subDomain.SetValue(site.Protocol, (int)SecurityZone);
            }
        }

        /// <summary>
        /// Delete site from registry.
        /// </summary>
        /// <param name="s">site</param>
        public void DeleteSite(string s)
        {
            var site = new Site(s);

            var rootkey = Domains;
            var subkey = Domains.OpenSubKey(site.RootDomain, true);

            // RootDomain was not found in registry, abort.
            if (subkey == null) return;

            if (!string.IsNullOrEmpty(site.SubDomain))
            {
                rootkey = subkey;
                subkey = rootkey.OpenSubKey(site.SubDomain, true);

                // SubDomain was not found in registry, abort.
                if (subkey == null) return;
            }

            // Protocol was not found among values, abort.
            if (!subkey.GetValueNames().Contains(site.Protocol)) return;

            // If subdomain has more than one protocol
            if (subkey.ValueCount > 1)
            {
                // Delete only subdomain's protocol
                subkey.DeleteValue(site.Protocol);
            }
            else if (subkey.SubKeyCount == 0)
            {
                // Delete subkey. There are no subkeys and it contains only one value.
                string name = subkey.Name.Split('\\').Last();
                rootkey.DeleteSubKey(name);
            }

        }
            //// Check each value name and compare it to protocol
            //foreach (string valueName in subkey.GetValueNames())
            //{
            //    // If the value name equals to the protocol
            //    if (valueName.Equals(site.Protocol))
            //    {
            //        // If subdomain has more than one protocol
            //        if (subkey.ValueCount > 1)
            //        {
            //            // Delete only subdomain's protocol
            //            subkey.DeleteValue(valueName);
            //        }
            //        else
            //        {
            //            // Delete the rootkey. There are no subkeys and it contains only one value.
            //            rootkey.DeleteSubKey(site.SubDomain);
            //        }
            //    }
            //}

            //// Open rootdomain key
            //var rootkey = Domains.OpenSubKey(site.RootDomain, true);

            //// Check each subkey name and compare it's name to the subdomain name
            //foreach (string subkeyName in rootkey.GetSubKeyNames())
            //{
            //    // If subkey name is the same as the subdomain name
            //    if (subkeyName.Equals(site.SubDomain))
            //    {
            //        // Open subdomain key
            //        var subkey = rootkey.OpenSubKey(site.SubDomain, true);

            //        // Check each value name and compare it to protocol
            //        foreach (string valueName in subkey.GetValueNames())
            //        {
            //            // If the value name equals to the protocol
            //            if (valueName.Equals(site.Protocol))
            //            {
            //                // If subdomain has more than one protocol
            //                if (subkey.ValueCount > 1)
            //                {
            //                    // Delete only subdomain's protocol
            //                    subkey.DeleteValue(valueName);
            //                }
            //                else
            //                {
            //                    // Delete subdomain key
            //                    rootkey.DeleteSubKey(site.SubDomain);
            //                }
            //            }
            //        }
            //    }
            //}

            //// If the rootkey now contains a zero subdomains, or if the site has no subdomains
            //if (rootkey.SubKeyCount == 0 || string.IsNullOrEmpty(site.SubDomain))
            //{
            //    // Check each value name of the rootkey
            //    foreach (string valueName in rootkey.GetValueNames())
            //    {
            //        // If the value name equals to the protocol
            //        if (valueName.Equals(site.Protocol))
            //        {
            //            // And if there are more than one value, or if the site has no subdomains
            //            if (rootkey.ValueCount > 1 || string.IsNullOrEmpty(site.SubDomain))
            //            {
            //                // Delete only rootkey's protocol
            //                rootkey.DeleteValue(valueName);
            //            }
            //            else
            //            {
            //                // Delete the rootkey. There are no subkeys and it contains only one value.
            //                Domains.DeleteSubKey(site.RootDomain);
            //            }
            //        }
            //    }
            //}
        //}

        public string[] QuerySites()
        {
            // Define a list of sites
            var sites = new List<string>();

            // Each subkey contains a root domain's name
            foreach (var rootDomain in Domains.GetSubKeyNames())
            {
                // Define a root domain key
                var rootDomainKey = Domains.OpenSubKey(rootDomain);

                // Extract value names based on a security zone
                var rootDomainValueNames = this.QueryValueNames(rootDomainKey);

                // Each value name forms a new site to be added to the list
                // If there are no value names, then urls should be formed with subdomains
                foreach (var valueName in rootDomainValueNames)
                {
                    string site = string.Empty;

                    // Each url is formed depending on either it contains an asterix or a protocol
                    if (valueName.Equals("*"))
                    {
                        // Root domain url is formed with an asteriks
                        site = $"*.{rootDomain}";
                    }
                    else
                    {
                        // Root domain url is formed with a protocol
                        site = $"{valueName}://*.{rootDomain}";
                    }

                    sites.Add(site);
                }

                // Each subkey contains a subdomain's name
                foreach (var subDomain in rootDomainKey.GetSubKeyNames())
                {
                    // Define a subdomain key
                    var subDomainKey = rootDomainKey.OpenSubKey(subDomain);

                    // Extract value names based on a security zone
                    var subDomainValueNames = QueryValueNames(subDomainKey);

                    // Each value name forms a new site to be added to the list
                    foreach (var valueName in subDomainValueNames)
                    {
                        string site = string.Empty;
                        // Each url is formed depending on either it contains an asterix or a protocol
                        if (valueName.Equals("*"))
                        {
                            site = $"{subDomain}.{rootDomain}";
                        }
                        else
                        {
                            site = $"{valueName}://{subDomain}.{rootDomain}";
                        }

                        sites.Add(site);
                    }
                }
            }

            sites.Sort();
            return sites.ToArray();
        }

        #endregion

        #region Private Methods

        private List<string> QueryValueNames(RegistryKey key)
        {
            var valueNames = new List<string>();

            // Process each value name in a given registry key
            foreach (var valueName in key.GetValueNames())
            {
                // Get numeric value assiociated with the current value name
                int value = (int)key.GetValue(valueName);

                // Add current value name to the list if it corresponds to a required security zone
                if (value == (int)SecurityZone)
                {
                    valueNames.Add(valueName);
                }
            }

            return valueNames;
        }

        private RegistryKey GetDomainsKey()
        {
            var keys = new List<RegistryKey>
            {
                Registry.LocalMachine.OpenSubKey(Constants.Registry.InternetSettings.Policies),
                Registry.Users.OpenSubKey($@"{System.Users.CurrentUser.SID}\{Constants.Registry.InternetSettings.Policies}"),
                Registry.CurrentUser.OpenSubKey(Constants.Registry.InternetSettings.Policies)
            };

            foreach (RegistryKey key in keys)
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (valueName.Equals(Constants.Registry.InternetSettings.SiteToZoneAssignmentList.ValueName))
                        {
                            int value = (int)key.GetValue(valueName);
                            if (value == Constants.Registry.InternetSettings.SiteToZoneAssignmentList.Value)
                            {
                                var result = key.OpenSubKey("ZoneMap");
                                if (result != null) result = result.OpenSubKey("Domains", true);
                                if (result != null) return result;
                            }
                        }
                    }
                }
            }

            return Registry.CurrentUser.OpenSubKey(Constants.Registry.InternetSettings.Domains.Default, true);
        }

        #endregion

        #region Internal Class

        protected class Site
        {
            private readonly string site;

            public string Protocol { get { return GetProtocol(); } }
            public string SubDomain { get { return GetSubDomain(); } }
            public string RootDomain { get { return GetRootDomain(); } }

            public Site(string site)
            {
                this.site = site;
            }

            private string GetProtocol()
            {
                if (site.Contains("://"))
                {
                    return site.Split(':')[0];
                }

                return "*";
            }

            private string GetSubDomain()
            {
                DomainParser domainParser = new DomainParser(new WebTldRuleProvider());
                DomainName domainName = domainParser.Get(Normalize(site));
                return domainName.SubDomain;
            }

            private string GetRootDomain()
            {
                DomainParser domainParser = new DomainParser(new WebTldRuleProvider());
                DomainName domainName = domainParser.Get(Normalize(site));
                return domainName.RegistrableDomain;
            }

            private string Normalize(string site)
            {
                if (site.Contains("://"))
                {
                    int index = site.IndexOf("://") + 3;
                    site = site.Substring(index);
                }

                if (site.Contains("*."))
                {
                    int index = site.IndexOf("*.") + 2;
                    site = site.Substring(index);
                }

                if (site.Last().Equals("."))
                {
                    int index = site.Length - 1;
                    site = site.Remove(index);
                }

                if (site.Contains("/"))
                {
                    int index = site.IndexOf("/");
                    site = site.Substring(0, index);
                }

                return site;
            }
        }

        #endregion
    }
}
