using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Core.Models
{
    internal class TemplateParseModel
    {
        public string RootName { get; set; }
        public bool IsCollection { get; set; }
        public IEnumerable<string> PropertiesInvokationChain { get; set; }
        public string Format {  get; set; }
    }
}
