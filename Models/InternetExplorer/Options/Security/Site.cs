using Nager.PublicSuffix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustie.Models.InternetExplorer.Options.Security
{
    public class Site : IComparable<Site>
    {

        #region Public Members

        public string Url { get; }
        public string Protocol { get; set; }
        public string Subdomain { get; set; }
        public string Rootdomain { get; set; }
        public bool HasProtocol { get { return !string.IsNullOrEmpty(Protocol); } }
        public bool HasSubdomain { get { return !string.IsNullOrEmpty(Subdomain); } }

        #endregion

        #region Constructor

        public Site(string url)
        {
            Url = url;
            Protocol = GetProtocol();
            Subdomain = GetSubDomain();
            Rootdomain = GetRootDomain();
        }

        #endregion

        #region Private Methods

        private string GetProtocol()
        {
            if (Url.Contains("://"))
            {
                return Url.Split(':')[0];
            }

            return string.Empty;
        }

        private string GetSubDomain()
        {
            string url = Normalize(Url);
            DomainParser domainParser = new DomainParser(new WebTldRuleProvider());
            DomainName domainName = domainParser.Get(url);
            return string.IsNullOrEmpty(domainName.SubDomain) ? string.Empty : domainName.SubDomain;
        }

        private string GetRootDomain()
        {
            string url = Normalize(Url);
            DomainParser domainParser = new DomainParser(new WebTldRuleProvider());
            DomainName domainName = domainParser.Get(url);
            return string.IsNullOrEmpty(domainName.RegistrableDomain) ? string.Empty : domainName.RegistrableDomain;
        }

        private string Normalize(string url)
        {
            if (url.Contains("://"))
            {
                int index = url.IndexOf("://") + 3;
                url = url.Substring(index);
                global::System.Console.WriteLine(index);
            }

            if (url.Contains("*."))
            {
                int index = url.IndexOf("*.") + 2;
                url = url.Substring(index);
            }

            if (url.Last().Equals("."))
            {
                int index = url.Length - 1;
                url = url.Remove(index);
            }

            if (url.Contains("/"))
            {
                int index = url.IndexOf("/");
                url = url.Substring(0, index);
            }

            return url;
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is Site site &&
                   this.Url == site.Url &&
                   this.Protocol == site.Protocol &&
                   this.Subdomain == site.Subdomain &&
                   this.Rootdomain == site.Rootdomain;
        }

        public override int GetHashCode()
        {
            var hashCode = -945784499;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Url);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Protocol);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Subdomain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Rootdomain);
            return hashCode;
        }

        public override string ToString()
        {
            return $@"Url: ""{Url}"", Protocol: ""{Protocol}"", Subdomain: ""{Subdomain}"", Rootdomain: ""{Rootdomain}""";
        }

        public int CompareTo(Site other)
        {
            return this.Url.CompareTo(other.Url);
        }

        #endregion
    }
}