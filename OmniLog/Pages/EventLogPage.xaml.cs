using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using OmniLog.Models;
using OmniLog.ViewModels;

namespace OmniLog.Pages
{
    public sealed partial class EventLogPage : Page, INotifyPropertyChanged
    {
        public EventLogViewModel ViewModel { get; } = new();

        private string _targetLogName = "Application Log";
        public string TargetLogName
        {
            get => _targetLogName;
            private set
            {
                if (_targetLogName != value)
                {
                    _targetLogName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _logQueryName = "Application";

        public EventLogPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is string logName)
            {
                _logQueryName = logName;
                TargetLogName = $"{logName} Log";
            }
            else
            {
                _logQueryName = "Application";
                TargetLogName = "Application Log";
            }

            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            int limit = 1000;
            if (LimitComboBox?.SelectedItem is ComboBoxItem limitItem)
            {
                if (int.TryParse(limitItem.Content.ToString(), out int parsedLimit))
                {
                    limit = parsedLimit;
                }
            }

            await ViewModel.LoadLogsAsync(_logQueryName, limit);
        }

        private async void LimitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && IsLoaded)
            {
                await LoadLogsAsync();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLogsAsync();
        }

        private void SearchTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ViewModel.SearchText = sender.Text;
        }

        private void LogsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ListView selection updates the ViewModel SelectedLogEntry (bound TwoWay)
        }

        private void CopyMessage_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLogEntry != null)
            {
                CopyTextToClipboard(ViewModel.SelectedLogEntry.Message);
            }
        }

        private void CopyXml_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLogEntry != null && !string.IsNullOrEmpty(ViewModel.SelectedLogEntry.Xml))
            {
                CopyTextToClipboard(ViewModel.SelectedLogEntry.Xml);
            }
        }

        private void CopyItemMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is EventLogEntryModel model)
            {
                CopyTextToClipboard(model.Message);
            }
        }

        private void CopyTextToClipboard(string text)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(text);
                Clipboard.SetContent(package);
            }
            catch
            {
                // Gracefully ignore clipboard failures
            }
        }

        #region Binding Helper Methods (Runs on UI Thread)

        public static Visibility GetVisibility(bool val)
        {
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GetVisibility(string? val)
        {
            return !string.IsNullOrEmpty(val) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GetInverseVisibility(bool val)
        {
            return val ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "N/A";
        }

        public static string FormatSelectedDateTime(EventLogEntryModel? entry)
        {
            return entry?.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "N/A";
        }

        public static string GetLevelName(byte? level)
        {
            return level switch
            {
                1 => "Critical",
                2 => "Error",
                3 => "Warning",
                4 => "Information",
                5 => "Verbose",
                _ => "Information"
            };
        }

        public static Brush GetLevelBrush(byte? level)
        {
            return level switch
            {
                1 => new SolidColorBrush(Microsoft.UI.Colors.Plum),
                2 => new SolidColorBrush(Microsoft.UI.Colors.OrangeRed),
                3 => new SolidColorBrush(Microsoft.UI.Colors.Gold),
                4 => new SolidColorBrush(Microsoft.UI.Colors.LightSkyBlue),
                5 => new SolidColorBrush(Microsoft.UI.Colors.DarkGray),
                _ => new SolidColorBrush(Microsoft.UI.Colors.LightSkyBlue)
            };
        }

        public static Brush GetLevelBackgroundBrush(byte? level)
        {
            return level switch
            {
                1 => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 100, 20, 100)), // Deep Purple
                2 => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 120, 20, 20)),   // Deep Red
                3 => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 120, 90, 0)),    // Dark Amber
                4 => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 20, 60, 100)),   // Deep Blue
                5 => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 60, 60, 60)),     // Dark Gray
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 20, 60, 100))     // Deep Blue (Information)
            };
        }

        public static string GetLevelGlyph(byte? level)
        {
            return level switch
            {
                1 => "\uE7BA", // Alert
                2 => "\uEA39", // Error Badge (Circle X)
                3 => "\uE7BA", // Warning Triangle
                4 => "\uE946", // Info Circle
                5 => "\uE9F9", // Diagnostic/Verbose
                _ => "\uE946"
            };
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
