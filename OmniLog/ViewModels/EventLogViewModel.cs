using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmniLog.Models;
using OmniLog.Services;

namespace OmniLog.ViewModels
{
    public class EventLogViewModel : INotifyPropertyChanged
    {
        private readonly EventLogService _eventLogService = new();

        public ObservableCollection<EventLogEntryModel> DisplayedLogs { get; } = new();
        public List<EventLogEntryModel> AllLoadedLogs { get; private set; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        private bool _filterCritical = true;
        public bool FilterCritical
        {
            get => _filterCritical;
            set { if (SetProperty(ref _filterCritical, value)) ApplyFilters(); }
        }

        private bool _filterError = true;
        public bool FilterError
        {
            get => _filterError;
            set { if (SetProperty(ref _filterError, value)) ApplyFilters(); }
        }

        private bool _filterWarning = true;
        public bool FilterWarning
        {
            get => _filterWarning;
            set { if (SetProperty(ref _filterWarning, value)) ApplyFilters(); }
        }

        private bool _filterInformation = true;
        public bool FilterInformation
        {
            get => _filterInformation;
            set { if (SetProperty(ref _filterInformation, value)) ApplyFilters(); }
        }

        private bool _filterVerbose = false;
        public bool FilterVerbose
        {
            get => _filterVerbose;
            set { if (SetProperty(ref _filterVerbose, value)) ApplyFilters(); }
        }

        private EventLogEntryModel? _selectedLogEntry;
        public EventLogEntryModel? SelectedLogEntry
        {
            get => _selectedLogEntry;
            set
            {
                if (SetProperty(ref _selectedLogEntry, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                }
            }
        }

        public bool HasSelection => SelectedLogEntry != null;

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public async Task LoadLogsAsync(string logName, int maxEntries)
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            SelectedLogEntry = null;
            StatusText = $"Loading events from '{logName}'...";
            DisplayedLogs.Clear();

            try
            {
                var startTime = DateTime.Now;
                var logs = await _eventLogService.GetEventLogsAsync(logName, maxEntries);
                AllLoadedLogs = logs;
                var duration = (DateTime.Now - startTime).TotalMilliseconds;

                ApplyFilters();
                StatusText = $"Loaded {AllLoadedLogs.Count} logs in {duration:F0}ms. Showing {DisplayedLogs.Count} logs.";
            }
            catch (Exception ex)
            {
                HasError = true;
                // Handle nested InnerExceptions for cleaner presentation
                ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                StatusText = "Error loading event logs.";
                AllLoadedLogs = new List<EventLogEntryModel>();
                DisplayedLogs.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ApplyFilters()
        {
            if (AllLoadedLogs == null) return;

            var filtered = AllLoadedLogs.Where(log =>
            {
                // Level Filter
                bool levelMatch = false;
                switch (log.Level)
                {
                    case 1: levelMatch = FilterCritical; break;
                    case 2: levelMatch = FilterError; break;
                    case 3: levelMatch = FilterWarning; break;
                    case 4:
                    case 0:
                    case null:
                        levelMatch = FilterInformation; break;
                    case 5: levelMatch = FilterVerbose; break;
                    default: levelMatch = FilterInformation; break;
                }
                if (!levelMatch) return false;

                // Search text filter
                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                string search = SearchText.ToLowerInvariant();
                return log.ProviderName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       log.EventId.ToString().Contains(search) ||
                       log.Message.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       log.MachineName.Contains(search, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            // Clear and add items to trigger binding notifications
            DisplayedLogs.Clear();
            foreach (var item in filtered)
            {
                DisplayedLogs.Add(item);
            }

            StatusText = $"Loaded {AllLoadedLogs.Count} logs. Showing {DisplayedLogs.Count} matching entries.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
