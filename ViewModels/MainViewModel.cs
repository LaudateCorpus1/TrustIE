using Caliburn.Micro;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Trustie.Models.Interfaces;
using Trustie.Models.InternetExplorer;
using Trustie.Models.Java.Security;

namespace Trustie.ViewModels
{
    public class MainViewModel : Screen
    {

        #region Private Members

        private ISecuritySettings _securitySettings;
        private string _label;
        private string _site;
        private string _selectedSite;
        private bool _internetExplorer;
        private bool _javaSecurity;
        private bool _javaSecurityEnabled;
        private Brush _siteTextBoxColor = Brushes.Black;

        #endregion

        #region Public Members

        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                NotifyOfPropertyChange(() => Label);
            }
        }

        public string Site
        {
            get { return _site; }
            set
            {
                _site = value;
                NotifyOfPropertyChange(() => Site);
            }
        }

        public BindableCollection<string> Sites { get; set; } = new BindableCollection<string>();

        public string SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                _selectedSite = value;
                NotifyOfPropertyChange(() => SelectedSite);
            }
        }

        public bool InternetExplorer
        {
            get { return _internetExplorer; }
            set
            {
                if (value.Equals(_internetExplorer)) return;
                _internetExplorer = value;
                NotifyOfPropertyChange(() => InternetExplorer);
                RadioButtonChecked();
            }
        }

        public bool JavaSecurity
        {
            get { return _javaSecurity; }
            set
            {
                if (value.Equals(_javaSecurity)) return;
                _javaSecurity = value;
                NotifyOfPropertyChange(() => JavaSecurity);
                RadioButtonChecked();
            }
        }

        public bool JavaSecurityEnabled
        {
            get { return _javaSecurityEnabled; }
            set
            {
                _javaSecurityEnabled = value;
                NotifyOfPropertyChange(() => JavaSecurityEnabled);
            }
        }

        public Brush SiteTextBoxColor
        {
            get { return _siteTextBoxColor; }
            set
            {
                _siteTextBoxColor = value;
                NotifyOfPropertyChange(() => SiteTextBoxColor);
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            InternetExplorer = true;
            JavaSecurityEnabled = JavaSettings.JavaInstalled;
        }

        #endregion

        #region Actions
        public void SiteTextBox_KeyUp(ActionExecutionContext context)
        {
            SiteTextBoxColor = Brushes.Black;

            var eventArgs = (KeyEventArgs)context.EventArgs;
            if (eventArgs.Key == Key.Enter)
            {
                if (IsWellFormedUriString())
                {
                    AddSite(Site);
                    QuerySites();
                    Site = string.Empty;
                }
                else
                {
                    SiteTextBoxColor = Brushes.Red;
                }
            }
        }

        public void SitesListBox_KeyUp(ActionExecutionContext context)
        {
            var eventArgs = (KeyEventArgs)context.EventArgs;
            if (eventArgs.Key == Key.Delete)
            {
                DeleteSite(SelectedSite);
                QuerySites();
            }
        }

        public void Close()
        {
            Environment.Exit(0);
        }

        #endregion

        #region Private Methods

        private void QuerySites()
        {
            var trustedSites = _securitySettings.QuerySites();

            Sites.Clear();

            foreach (var site in trustedSites)
            {
                Sites.Add(site);
            }
        }

        private void AddSite(string site)
        {
            _securitySettings.AddSite(site);
        }

        private void DeleteSite(string site)
        {
            _securitySettings.DeleteSite(site);
        }

        private void RadioButtonChecked()
        {
            if (InternetExplorer)
            {
                _securitySettings = new IESettings();
                Label = "Add site to Trusted zone:";
                
            }
            if (JavaSecurity)
            {
                _securitySettings = new JavaSettings();
                Label = "Add site to Exceptions:";
            }

            QuerySites();
        }

        /// <summary>
        /// Checks if a given string is a well formed URI.
        /// </summary>
        /// <returns></returns>
        private bool IsWellFormedUriString()
        {
            string[] asteriksCases = { "http://*.", "https://*.", "*." };

            // Ignore asteriks at the beginning of the url
            // by removing it before check starts.
            foreach (string allowedCase in asteriksCases)
            {
                if (Site.StartsWith(allowedCase))
                {
                    int asterixIndex = Site.IndexOf("*");
                    Site = Site.Remove(asterixIndex, 2);
                }
            }

            // Should contain at least one dot, but not as a last character
            if (!Site.Contains(".") || (Site.Last().Equals('.') && Site.Count(c => c == '.') == 1)) return false;

            return Uri.IsWellFormedUriString(Site, UriKind.RelativeOrAbsolute);
        }

        #endregion

    }
}
