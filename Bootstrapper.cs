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

            // Add IsEnabled convention for IsEnabledPropery of the UIElement
            AddConvention(UIElement.IsEnabledProperty, "IsEnabled", "IsEnabledChanged", "Enabled");
        }

        /// <summary>
        /// Add an element convention to a property in Caliburn.Micro.
        /// </summary>
        /// <param name="dependencyProperty">Element property</param>
        /// <param name="parameterProperty">The default property of action parameters.</param>
        /// <param name="eventName">The default event to trigger actions.</param>
        /// <param name="name">String to append to a property name.</param>
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
