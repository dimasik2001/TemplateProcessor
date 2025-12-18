using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Core.TemplateStructureModels
{
    public class TemplateDescriptor
    {
        public Dictionary<string, object> TemplateEntities { get; set; } = new Dictionary<string, object>();
    }
}
