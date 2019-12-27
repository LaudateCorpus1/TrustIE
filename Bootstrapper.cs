using Caliburn.Micro;
using System.Windows;
using Trustie.ViewModels;

namespace Trustie
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();

            AddConvention(UIElement.IsEnabledProperty, "IsEnabled", "IsEnabledChanged", "Enabled");
        }

        private void AddConvention(DependencyProperty dependencyProperty, string parameterProperty, string eventName, string name)
        {
            ConventionManager.AddElementConvention<FrameworkElement>(dependencyProperty, parameterProperty, eventName);
            var baseBindProperties = ViewModelBinder.BindProperties;
            ViewModelBinder.BindProperties = (frameWorkElements, viewModels) =>
            {
                foreach (var frameworkElement in frameWorkElements)
                {
                    var propertyName = frameworkElement.Name + name;
                    var property = viewModels.GetPropertyCaseInsensitive(propertyName);
                    if (property != null)
                    {
                        var convention = ConventionManager.GetElementConvention(typeof(FrameworkElement));
                        ConventionManager.SetBindingWithoutBindingOverwrite
                        (
                            viewModels,
                            propertyName,
                            property,
                            frameworkElement,
                            convention,
                            convention.GetBindableProperty(frameworkElement)
                        );
                    }
                }
                return baseBindProperties(frameWorkElements, viewModels);
            };
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
