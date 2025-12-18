using System;
using System.Collections.Generic;
using System.Text;
using TemplateProcessor.Core.Models;
using TemplateProcessor.Core.TemplateStructureModels;

namespace TemplateProcessor.Core.Parsers.Abstractions
{
    internal interface ITemplateParser
    {
        bool TryParse(string template, out TemplateParseModel result);
    }
}
