using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateProcessor.Abstractions;

namespace TemplateProcessor.Core.Validators
{
    public class DefaultTemplateValidator : ITemplateValidator
    {
        private static readonly Regex TemplateRegex =
       new Regex(@"\{\{#?[\w]+(?:\.[\w]+)*\}\}",
           RegexOptions.Compiled);
        public bool HasTemplates(string text, out IEnumerable<string> templates)
        {
            templates = Enumerable.Empty<string>();

            if (string.IsNullOrEmpty(text))
                return false;

            var matches = TemplateRegex.Matches(text);

            if (matches.Count == 0)
                return false;

            templates = matches
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();

            return true;
        }

        public bool IsTemplate(string template)
        {
            return template.StartsWith("{{") && template.EndsWith("}}") && template.Length > 4;
        }
    }
}
