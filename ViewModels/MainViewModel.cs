using Caliburn.Micro;
using System;
using System.Linq;
using System.Windows;
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

        /// <summary>
        /// Label on top of TextBox.
        /// </summary>
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                NotifyOfPropertyChange(() => Label);
            }
        }

        /// <summary>
        /// Site TextBox.
        /// </summary>
        public string Site
        {
            get { return _site; }
            set
            {
                _site = value;
                NotifyOfPropertyChange(() => Site);
            }
        }

        /// <summary>
        /// Sites ListBox.
        /// </summary>
        public BindableCollection<string> Sites { get; set; } = new BindableCollection<string>();

        /// <summary>
        /// Selected site in Sites list.
        /// </summary>
        public string SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                _selectedSite = value;
                NotifyOfPropertyChange(() => SelectedSite);
            }
        }

        /// <summary>
        /// InternetExplorer checkbox.
        /// </summary>
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

        /// <summary>
        /// JavaSecurity checkbox.
        /// </summary>
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

        /// <summary>
        /// Enables or disables JavaSecurity checkbox.
        /// </summary>
        public bool JavaSecurityEnabled
        {
            get { return _javaSecurityEnabled; }
            set
            {
                _javaSecurityEnabled = value;
                NotifyOfPropertyChange(() => JavaSecurityEnabled);
            }
        }

        /// <summary>
        /// Color brush for TextBox where user enters site.
        /// </summary>
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

        /// <summary>
        /// Default constructor. Sets InternetExplorer checkbox. 
        /// Disables JavaSecurity checkbox if Java is not installed.
        /// </summary>
        public MainViewModel()
        {
            InternetExplorer = true;
            JavaSecurityEnabled = JavaSettings.JavaInstalled;
        }

        #endregion

        #region Actions

        /// <summary>
        /// User pressed Enter key on Site TextBox to add site to security settings.
        /// </summary>
        /// <param name="context">The context used during the exectuion</param>
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

        /// <summary>
        /// User pressed Delete key on SelectedSite to remove site from security settings.
        /// </summary>
        /// <param name="context">The context used during the exectuion</param>
        public void SitesListBox_KeyUp(ActionExecutionContext context)
        {
            var eventArgs = (KeyEventArgs)context.EventArgs;
            if (eventArgs.Key == Key.Delete)
            {
                DeleteSite(SelectedSite);
                QuerySites();
            }
        }

        /// <summary>
        /// Closes TrustIE
        /// </summary>
        public void Close()
        {
            Environment.Exit(0);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Queries sites and updates Sites list
        /// </summary>
        private void QuerySites()
        {
            Sites.Clear();

            var sites = _securitySettings.QuerySites();
            foreach (var site in sites)
            {
                Sites.Add(site);
            }
        }

        /// <summary>
        /// Adds site to security settings
        /// </summary>
        /// <param name="site">Site to add</param>
        private void AddSite(string site)
        {
            _securitySettings.AddSite(site);
        }

        /// <summary>
        /// Deletes site from security settings
        /// </summary>
        /// <param name="site">Site to delete</param>
        private void DeleteSite(string site)
        {
            _securitySettings.DeleteSite(site);
        }

        /// <summary>
        /// Changes label text, initializes ISecuritySettings and queries it for sites.
        /// </summary>
        private void RadioButtonChecked()
        {
            if (InternetExplorer)
            {
                _securitySettings = new IESettings();
                Label = Constants.Text.Label.InternetExplorer;
                
            }
            if (JavaSecurity)
            {
                _securitySettings = new JavaSettings();
                Label = Constants.Text.Label.JavaSecurity;
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
