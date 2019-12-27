namespace Trustie.Models.Interfaces
{
    interface ISecuritySettings
    {
        string[] QuerySites();
        void AddSite(string site);
        void DeleteSite(string site);
    }
}
