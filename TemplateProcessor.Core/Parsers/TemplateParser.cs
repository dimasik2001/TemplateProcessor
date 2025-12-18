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
            var isExists = false;
            var isCollection = false;
            if (templateDescriptor.SingleTemplateEntities.ContainsKey(rootName))
            {
                isExists = true;
                isCollection = false;
            }

            if (templateDescriptor.CollectionTemplateEntities.ContainsKey(rootName))
            {
                isExists = true;
                isCollection = true;
            }
            if (!isExists) 
            { 
                result = null;
                return false;
            }
            result = new TemplateParseModel()
            {
                RootName = rootName,
                PropertiesInvokationChain = splittedTokens.Skip(1),
                IsCollection = isCollection,
            };
            return true;
        }



        private bool IsValidTemplate(string template)
        {
            return template.StartsWith("{{") && template.EndsWith("}}") && template.Length > 4;
        }
       
    }
}
