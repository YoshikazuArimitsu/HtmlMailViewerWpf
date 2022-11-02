using HtmlMailControlWpf;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SampleApp
{

    public class EnumSourceProvider<T> : MarkupExtension
    {
        public IEnumerable Source { get; }
            = typeof(T).GetEnumValues()
                .Cast<T>()
                .Select(value => new { Code = value, Name = value!.ToString() });

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class SourceTypeEnumSourceProvider : EnumSourceProvider<SourceType> { }

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
            get {
                return _linkPatterns;
            }
            set {
                _linkPatterns = value;
                NotifyPropertyChanged();
            }
        }

        public string? _linkPatternsStr;

        public string? LinkPatternsStr
        {
            get
            {
                return _linkPatternsStr;
            }
            set
            {
                _linkPatternsStr = value;
                NotifyPropertyChanged();

                LinkPatterns = _linkPatternsStr?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public SourceType _sourceType;
        public SourceType SourceType
        {
            get
            {
                return _sourceType;
            }
            set
            {
                _sourceType = value;
                NotifyPropertyChanged();
            }
        }

        public string? _emlFile;
        public string? EmlFile
        {
            get
            {
                return _emlFile;
            }
            set
            {
                _emlFile = value;
                NotifyPropertyChanged();
            }
        }

        public string? _body;
        public string? Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
                NotifyPropertyChanged();
            }
        }


        public MainWindowViewModel(
            IOptions<AppSettings> settings,
            IConfiguration configuration
        )
        {
            _settings = settings.Value;

            SourceType = SourceType.EmlFile;
            EmlFile = _settings.EmlFile;
            Body = _settings.Body;
            LinkPatterns = _settings.LinkPatterns;
            LinkPatternsStr = string.Join(",", LinkPatterns ?? new string[0]);
        }
    }
}
