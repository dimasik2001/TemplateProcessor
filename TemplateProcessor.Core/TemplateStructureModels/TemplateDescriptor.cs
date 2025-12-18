using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Core.TemplateStructureModels
{
    public class TemplateDescriptor
    {
        public Dictionary<string, object> SingleTemplateEntities { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, IEnumerable<object>> CollectionTemplateEntities { get; set; } = new Dictionary<string, IEnumerable<object>>();
    }
}
