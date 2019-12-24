using Caliburn.Micro;
using System.Security;
using System.Windows.Input;
using Trustie.Models.InternetExplorer.Options.Security;

namespace Trustie.ViewModels
{
    public class MainViewModel : Screen
    {

        #region Private Members

        private readonly SecuritySettings _securitySettings;
        private string _customSite;
        private Site _selectedSite;

        #endregion

        #region Public Members

        public string CustomSite
        {
            get { return _customSite; }
            set
            {
                _customSite = value;
                NotifyOfPropertyChange(() => CustomSite);
            }
        }

        public BindableCollection<Site> Sites { get; set; } = new BindableCollection<Site>();

        public Site SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                _selectedSite = value;
                NotifyOfPropertyChange(() => SelectedSite);
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _securitySettings = new SecuritySettings();
            QueryTrustedSites();
        }

        #endregion

        #region Actions

        public void CustomSite_KeyUp(ActionExecutionContext context)
        {
            var eventArgs = (KeyEventArgs)context.EventArgs;
            if (eventArgs.Key == Key.Enter)
            {
                var site = new Site(CustomSite);
                CustomSite = string.Empty;
                AddSiteToTrusted(site);
                QueryTrustedSites();
            }
        }

        public void Sites_KeyUp(ActionExecutionContext context)
        {
            var eventArgs = (KeyEventArgs)context.EventArgs;
            if (eventArgs.Key == Key.Delete)
            {
                DeleteSite(SelectedSite);
            }
        }

        public void Close()
        {
            System.Environment.Exit(0);
        }

        #endregion

        #region Private Methods

        private void AddSiteToTrusted(Site site)
        {
            _securitySettings.AddSite(site, SecurityZone.Trusted);
            QueryTrustedSites();
        }

        private void DeleteSite(Site site)
        {
            _securitySettings.DeleteSite(site);
            QueryTrustedSites();
        }

        private void QueryTrustedSites()
        {
            var trustedSites = _securitySettings.QuerySites(SecurityZone.Trusted);

            trustedSites.Sort();
            Sites.Clear();

            foreach (var site in trustedSites)
            {
                Sites.Add(site);
            }
        }

        #endregion

    }
}
