using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemplateProcessor.Abstractions;
using TemplateProcessor.Core.Models;
using TemplateProcessor.Core.Parsers.Abstractions;
using TemplateProcessor.Core.TemplateStructureModels;

namespace TemplateProcessor.Core.Parsers
{
    internal class TemplateParser : ITemplateParser
    {
        private readonly TemplateDescriptor templateDescriptor;
        private readonly ITemplateValidator templateValidator;

        public TemplateParser(TemplateDescriptor templateDescriptor, ITemplateValidator templateValidator) 
        {
            this.templateDescriptor = templateDescriptor;
            this.templateValidator = templateValidator;
        }

        public bool TryParse(string template, out TemplateParseModel result)
        {
            string format = null;
            
            if (!templateValidator.IsTemplate(template))
            {
                result = null;
                return false;
            }

            template = template.Trim('{', '}');

            if (template.Contains("|"))
            {
                var templateAndFormat = template.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                template = templateAndFormat[0].Trim();
                format = templateAndFormat[1];
            }

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
                Format = format,
            };
            return true;
        }
       
    }
}
