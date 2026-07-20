using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OmniLog.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OmniLog
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            AppWindow.SetIcon("Assets/AppIcon.ico");
        }

        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            NavFrame.GoBack();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString() ?? string.Empty;
                if (tag.StartsWith("log_"))
                {
                    string logName = tag.Substring(4); // e.g. "Application", "System", "Security"
                    NavFrame.Navigate(typeof(EventLogPage), logName);
                }
                else
                {
                    switch (tag)
                    {
                        case "home":
                            NavFrame.Navigate(typeof(HomePage));
                            break;
                        case "about":
                            NavFrame.Navigate(typeof(AboutPage));
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown navigation item tag: {tag}");
                    }
                }
            }
        }
    }
}
