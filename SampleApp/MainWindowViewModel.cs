using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly AppSettings _settings;

        public string[]? _linkPatterns;
        public string[]? LinkPatterns
        {
            get { return _linkPatterns; }
            set { _linkPatterns = value; }
        }

        public string? _source;
        public string? Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                NotifyPropertyChanged();
            }
        }

        public MainWindowViewModel(
            IOptions<AppSettings> settings,
            IConfiguration configuration
        )
        {
            _settings = settings.Value;
            Source = _settings.EmlFile;
            LinkPatterns = _settings.LinkPatterns;
        }
    }
}
