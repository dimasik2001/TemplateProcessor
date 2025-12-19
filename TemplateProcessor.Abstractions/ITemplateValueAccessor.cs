using System.Collections.Generic;

namespace TemplateProcessor.Abstractions
{
    public interface ITemplateValueAccessor
    {    
        object GetValue(string template);
        IEnumerable<object> GetCollection(string template);
    }
}
