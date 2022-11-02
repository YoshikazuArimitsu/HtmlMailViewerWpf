using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    public class AppSettings
    {
        public string? EmlFile { get; set; }
        public string? Body { get; set; }
        public string[]? LinkPatterns { get; set; }
    }
}
