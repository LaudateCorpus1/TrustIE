using Microsoft.Win32;
using System.Collections.Generic;
using System.Security;

namespace Trustie.Models.InternetExplorer.Options.Security
{
    public class SecuritySettings
    {

        #region Public Member

        /// <summary>
        /// Contains registry path to the current Domains key.
        /// </summary>
        public RegistryKey Domains { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The primary constructor assigns a proper Domains key to the Root.
        /// </summary>
        public SecuritySettings()
        {
            Domains = GetDomainsKey();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a site to registry as the new registry key.
        /// </summary>
        /// <param name="site">Site to add</param>
        /// <param name="zone">Site's security zone</param>
        /// <returns>Registry path to added site</returns>
        public RegistryKey AddSite(Site site, SecurityZone zone = SecurityZone.Trusted)
        {
            // Create root domain key in registry
            RegistryKey rootDomain = Domains.CreateSubKey(site.Rootdomain);

            // Value is asteriks if protocol is not defined
            string value = (site.Protocol.Length > 0) ? site.Protocol : "*";

            // Subdomain is defined
            if (site.Subdomain.Length > 0)
            {
                // Create subdomain key in registry
                RegistryKey subDomain = rootDomain.CreateSubKey(site.Subdomain);

                // Set value for a given zone
                subDomain.SetValue(value, (int)zone);

                return subDomain;
            }

            // Set value for a given zone
            rootDomain.SetValue(value, (int)zone);

            return rootDomain;
        }

        public void DeleteSite(Site site)
        {
            var rootkey = Domains.OpenSubKey(site.Rootdomain, true);
            string value = site.HasProtocol ? site.Protocol : "*"; //TODO: Maybe site itself should handle asteriks as a protocol?

            foreach (string subkeyName in rootkey.GetSubKeyNames())
            {
                if (subkeyName.Equals(site.Subdomain))
                {
                    var subkey = rootkey.OpenSubKey(site.Subdomain, true);

                    foreach (string valueName in subkey.GetValueNames())
                    {
                        if (valueName.Equals(value))
                        {
                            if (subkey.ValueCount > 1)
                            {
                                subkey.DeleteValue(valueName);
                            }
                            else
                            {
                                rootkey.DeleteSubKey(site.Subdomain);
                            }
                        }
                    }
                }
            }

            if (rootkey.SubKeyCount == 0 || !site.HasSubdomain)
            {
                foreach (string valueName in rootkey.GetValueNames())
                {
                    if (valueName.Equals(value))
                    {
                        if (rootkey.ValueCount > 1 || !site.HasSubdomain)
                        {
                            rootkey.DeleteValue(valueName);
                        }
                        else
                        {
                            Domains.DeleteSubKey(site.Rootdomain);
                        }
                    }
                }
            }
        }

        public List<Site> QuerySites(SecurityZone zone)
        {
            // Define a list of sites
            List<Site> sites = new List<Site>();

            // Each subkey contains a root domain's name
            foreach (var rootDomain in Domains.GetSubKeyNames())
            {
                // Define a root domain key
                RegistryKey rootDomainKey = Domains.OpenSubKey(rootDomain);

                // Extract value names based on a security zone
                List<string> rootDomainValueNames = this.QueryValueNames(rootDomainKey, zone);

                // Each value name forms a new site to be added to the list
                // If there are no value names, then urls should be formed with subdomains
                foreach (var valueName in rootDomainValueNames)
                {
                    string url = string.Empty;
                    string protocol = string.Empty;

                    // Each url is formed depending on either it contains an asterix or a protocol
                    if (valueName.Equals("*"))
                    {
                        // Root domain url is formed with an asteriks
                        url = $"*.{rootDomain}";
                    }
                    else
                    {
                        // Root domain url is formed with a protocol
                        protocol = valueName;
                        url = $"{protocol}://*.{rootDomain}";
                    }

                    // Define a site and it's parameters
                    Site site = new Site(url)
                    {
                        Protocol = protocol,
                        Rootdomain = rootDomain
                    };

                    sites.Add(site);
                }

                // Each subkey contains a subdomain's name
                foreach (var subDomain in rootDomainKey.GetSubKeyNames())
                {
                    // Define a subdomain key
                    RegistryKey subDomainKey = rootDomainKey.OpenSubKey(subDomain);

                    // Extract value names based on a security zone
                    List<string> subDomainValueNames = QueryValueNames(subDomainKey, zone);

                    // Each value name forms a new site to be added to the list
                    foreach (var valueName in subDomainValueNames)
                    {
                        string protocol = string.Empty;

                        // Each url is formed depending on either it contains an asterix or a protocol
                        string url;
                        if (valueName.Equals("*"))
                        {
                            url = $"{subDomain}.{rootDomain}";
                        }
                        else
                        {
                            protocol = valueName;
                            url = $"{protocol}://{subDomain}.{rootDomain}";
                        }

                        // Define a site and it's parameters
                        Site site = new Site(url)
                        {
                            Protocol = protocol,
                            Rootdomain = rootDomain,
                            Subdomain = subDomain
                        };

                        sites.Add(site);
                    }
                }
            }

            return sites;
        }

        #endregion

        #region Private Methods

        private List<string> QueryValueNames(RegistryKey key, SecurityZone zone)
        {
            List<string> valueNames = new List<string>();

            // Process each value name in a given registry key
            foreach (var valueName in key.GetValueNames())
            {
                // Get numeric value assiociated with the current value name
                int value = (int)key.GetValue(valueName);

                // Add current value name to the list if it corresponds to a required security zone
                if (value == (int)zone)
                {
                    valueNames.Add(valueName);
                }
            }

            return valueNames;
        }

        private RegistryKey GetDomainsKey()
        {
            List<RegistryKey> keys = new List<RegistryKey>
            {
                Registry.LocalMachine.OpenSubKey(Constants.Registry.InternetSettings.Policies),
                Registry.Users.OpenSubKey($@"{System.Users.Queries.CurrentUser.SID}\{Constants.Registry.InternetSettings.Policies}"),
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
                                RegistryKey result = key.OpenSubKey("ZoneMap");
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

    }
}
