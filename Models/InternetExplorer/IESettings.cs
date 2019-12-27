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

            // Set rootkey to Domains, and subkey to rootdomain key.
            var rootkey = Domains;
            var subkey = Domains.OpenSubKey(site.RootDomain, true);

            // RootDomain was not found in registry, abort
            if (subkey == null) return;

            // If subdomain is defined then change rootkey to rootdomain key, and subkey to subdomain key
            if (!string.IsNullOrEmpty(site.SubDomain))
            {
                rootkey = subkey;
                subkey = rootkey.OpenSubKey(site.SubDomain, true);

                // SubDomain was not found in registry, abort
                if (subkey == null) return;
            }

            // Protocol was not found among values, abort
            if (!subkey.GetValueNames().Contains(site.Protocol)) return;

            // If subkey has more than one value, or if site doesn't contain a subdomain
            if (subkey.ValueCount > 1 || string.IsNullOrEmpty(site.SubDomain))
            {
                // Delete only value
                subkey.DeleteValue(site.Protocol);
            }
            // Or if subkey doesn't contain any subkeys
            else if (subkey.SubKeyCount == 0)
            {
                // Delete subkey. There are no subkeys and it contains only one value
                string name = subkey.Name.Split('\\').Last();
                rootkey.DeleteSubKey(name);
            }

            // Delete empty rootdomain key
            rootkey = Domains.OpenSubKey(site.RootDomain, true);
            if (rootkey.SubKeyCount == 0 && rootkey.ValueCount == 0)
            {
                Domains.DeleteSubKey(site.RootDomain);
            }
        }

        /// <summary>
        /// Queries registry for sites. Composes keys, subkeys and their values to rootdomains, subdomains and protocols.
        /// </summary>
        /// <returns>Array if site strings</returns>
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
                    // Each url is formed depending on either it contains an asterix or a protocol
                    string site;
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
                        // Each url is formed depending on either it contains an asterix or a protocol
                        string site;
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

        /// <summary>
        /// Queries value names of a given registry key.
        /// </summary>
        /// <param name="key">Key with values</param>
        /// <returns>List of value names</returns>
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

        /// <summary>
        /// Seaches registry for Domains key that contains sites.
        /// </summary>
        /// <returns>Domains registry key</returns>
        private RegistryKey GetDomainsKey()
        {
            // List of known places that can hold Domains key.
            var keys = new List<RegistryKey>
            {
                Registry.LocalMachine.OpenSubKey(Constants.Registry.InternetSettings.Policies),
                Registry.Users.OpenSubKey($@"{System.Users.CurrentUser.SID}\{Constants.Registry.InternetSettings.Policies}"),
                Registry.CurrentUser.OpenSubKey(Constants.Registry.InternetSettings.Policies)
            };

            // Each key from the list..
            foreach (RegistryKey key in keys)
            {
                // ..that is not null and..
                if (key != null)
                {
                    // ..that key has a value that..
                    foreach (string valueName in key.GetValueNames())
                    {
                        // ..corresponds to Site to Zone Assignment policy and..
                        if (valueName.Equals(Constants.Registry.InternetSettings.SiteToZoneAssignmentList.ValueName))
                        {
                            // ..the policy is enabled..
                            int value = (int)key.GetValue(valueName);
                            if (value == Constants.Registry.InternetSettings.SiteToZoneAssignmentList.Value)
                            {
                                // ..and subkeys are not null, then return this registry key.
                                var result = key.OpenSubKey("ZoneMap");
                                if (result != null) result = result.OpenSubKey("Domains", true);
                                if (result != null) return result;
                            }
                        }
                    }
                }
            }

            // Return default registry key
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
