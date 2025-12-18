using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Core.Models
{
    internal class TemplateParseModel
    {
        public string RootName { get; set; }
        public bool InvokeInCollectionIterator { get; set; }
        public IEnumerable<PropertyInvocationModel> PropertiesInvokationChain { get; set; }
    }
}
