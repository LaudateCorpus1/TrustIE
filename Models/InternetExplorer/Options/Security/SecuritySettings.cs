using Microsoft.Win32;
using System.Collections.Generic;
using System.Security;

namespace Trustie.Models.InternetExplorer.Options.Security
{
    public class SecuritySettings
    {
        #region Public Member

        public RegistryKey Root { get; set; }

        #endregion

        #region Constructor

        public SecuritySettings()
        {
            Root = GetDomainsKey();
        }

        #endregion

        #region Public Methods

        public RegistryKey AddSite(Site site, SecurityZone zone)
        {
            // Create root domain key in registry
            RegistryKey rootDomain = Root.CreateSubKey(site.Rootdomain);

            // Store asterix if protocol was not defined
            string value = (site.Protocol.Length > 0) ? site.Protocol : "*";

            // If sub domain was defined
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
            var rootdomainKey = Root.OpenSubKey(site.Rootdomain, true);
            var subdomainKey = rootdomainKey.OpenSubKey(site.Subdomain, true);
            string value = site.HasProtocol ? site.Protocol : "*";

            if (subdomainKey != null)
            {
                if (rootdomainKey.SubKeyCount == 1)
                {
                    if (subdomainKey.ValueCount == 1)
                    {
                        global::System.Console.WriteLine(subdomainKey);
                        subdomainKey.DeleteSubKey(site.Subdomain);
                    }
                    else
                    {
                        subdomainKey.DeleteValue(value);
                    }
                }
                else
                {
                    if (subdomainKey.ValueCount == 1)
                    {
                        subdomainKey.DeleteSubKey(site.Subdomain);
                    }
                    else
                    {
                        subdomainKey.DeleteValue(value);
                    }
                }
            }
            else
            {
                if (rootdomainKey.SubKeyCount == 0)
                {
                    if (rootdomainKey.ValueCount == 1)
                    {
                        rootdomainKey.DeleteSubKey(site.Rootdomain);
                    }
                    else
                    {
                        rootdomainKey.DeleteValue(value);
                    }
                }
            }
            //if (string.IsNullOrEmpty(site.Subdomain))
            //{
            //    if (rootdomainKey.ValueCount > 1 || rootdomainKey.SubKeyCount > 0)
            //    {
            //        string value = string.IsNullOrEmpty(site.Protocol) ? "*" : site.Protocol;
            //        rootdomainKey.DeleteValue(value);
            //    }
            //    else
            //    {
            //        rootdomainKey.DeleteSubKey(site.Rootdomain);
            //    }
            //}
            //else
            //{
            //    var subdomainKey = rootdomainKey.OpenSubKey(site.Subdomain, true);

            //    if (rootdomainKey.SubKeyCount > 1)
            //    {
            //        if (string.IsNullOrEmpty(site.Protocol))
            //        {
            //            subdomainKey.DeleteValue("*");
            //        }
            //        else
            //        {
            //            subdomainKey.DeleteValue(site.Protocol);
            //        }
            //    }
            //    else
            //    {
            //        subdomainKey.DeleteSubKey(site.Subdomain);
            //    }
            //}

            //switch (rootdomainKey.SubKeyCount)
            //{
            //    case 0:
            //        rootdomainKey.DeleteSubKey(site.Rootdomain);
            //        break;
            //    case 1:
            //        if (string.IsNullOrEmpty(site.Subdomain))
            //        {

            //        }
            //        break;
            //    default:
            //        break;
            //}

        }

        public List<Site> QuerySites(SecurityZone zone)
        {
            // Define a list of sites
            List<Site> sites = new List<Site>();

            // Each subkey contains a root domain's name
            foreach (var rootDomain in Root.GetSubKeyNames())
            {
                // Define a root domain key
                RegistryKey rootDomainKey = Root.OpenSubKey(rootDomain);

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
