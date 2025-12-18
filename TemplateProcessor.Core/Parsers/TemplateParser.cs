using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemplateProcessor.Core.Models;
using TemplateProcessor.Core.Parsers.Abstractions;
using TemplateProcessor.Core.TemplateStructureModels;

namespace TemplateProcessor.Core.Parsers
{
    internal class TemplateParser : ITemplateParser
    {
        private readonly TemplateDescriptor templateDescriptor;

        public TemplateParser(TemplateDescriptor templateDescriptor)
        {
            this.templateDescriptor = templateDescriptor;
        }

        public bool TryParse(string template, out TemplateParseModel result)
        {
            if (!IsValidTemplate(template))
            {
                result = null;
                return false;
            }
            template = template.Trim('{', '}');
            var splittedTokens = template.Split('.');
            var rootName = splittedTokens[0];
            var invocationModel = GetPropertyInvocationModel(rootName);
            rootName = invocationModel.Name;

            if (!templateDescriptor.TemplateEntities.ContainsKey(rootName))
            {
                result = null;
                return false;

            }

            result = new TemplateParseModel()
            {
                RootName = invocationModel.Name,
                InvokeInCollectionIterator = invocationModel.InvokeInCollectionIterator,
                PropertiesInvokationChain = splittedTokens.Skip(1).Select(GetPropertyInvocationModel),
            };
            return true;
        }

        private PropertyInvocationModel GetPropertyInvocationModel(string memberName)
        {
            var invokeInCollectionIterator = memberName.StartsWith("[") && memberName.EndsWith("]") && memberName.Length > 2;


            if (invokeInCollectionIterator)
            {
                memberName = memberName.Trim('[', ']');
            }
            return new PropertyInvocationModel()
            {
                InvokeInCollectionIterator = invokeInCollectionIterator,
                Name = memberName,
            };
        }

        private bool IsValidTemplate(string template)
        {
            return template.StartsWith("{{") && template.EndsWith("}}") && template.Length > 4;
        }

    }
}
