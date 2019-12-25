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
        private string _customSiteTextBox;
        private Site _selectedSite;

        #endregion

        #region Public Members

        public string CustomSiteTextBox
        {
            get { return _customSiteTextBox; }
            set
            {
                _customSiteTextBox = value;
                NotifyOfPropertyChange(() => CustomSiteTextBox);
            }
        }

        public BindableCollection<Site> SitesListBox { get; set; } = new BindableCollection<Site>();

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
                var site = new Site(CustomSiteTextBox);
                CustomSiteTextBox = string.Empty;
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
            SitesListBox.Clear();

            foreach (var site in trustedSites)
            {
                SitesListBox.Add(site);
            }
        }

        #endregion

    }
}
