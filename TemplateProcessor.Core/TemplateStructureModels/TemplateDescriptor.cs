using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Core.TemplateStructureModels
{
    public class TemplateDescriptor
    {
        public Dictionary<string, object> SingleTemplateEntities { get; }
        public Dictionary<string, IEnumerable<object>> CollectionTemplateEntities { get; }
    }
}
