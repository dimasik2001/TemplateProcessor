using System.Collections.Generic;

namespace TemplateProcessor.Abstractions
{
    public interface ITemplateValueAccessor
    {
        bool IsCollectionTemplate(string template);
        object GetValue(string template);
        IEnumerable<object> GetCollection(string template);
    }
}
