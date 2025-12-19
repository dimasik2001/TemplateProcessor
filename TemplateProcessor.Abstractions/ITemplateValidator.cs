using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateProcessor.Abstractions
{
    public interface ITemplateValidator
    {
        bool IsCollectionTemplate(string template);
        bool IsTemplate(string template);
        bool HasTemplates(string text, out IEnumerable<string> template);
    }
}
