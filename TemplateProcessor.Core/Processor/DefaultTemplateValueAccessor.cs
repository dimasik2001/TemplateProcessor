using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemplateProcessor.Abstractions;
using TemplateProcessor.Core.Models;
using TemplateProcessor.Core.Parsers;
using TemplateProcessor.Core.Parsers.Abstractions;
using TemplateProcessor.Core.TemplateStructureModels;
using TemplateProcessor.Core.Validators;

namespace TemplateProcessor.Core.Processor
{
    public class DefaultTemplateValueAccessor : ITemplateValueAccessor
    {
        private readonly TemplateDescriptor _templateDescriptor;

        public ITemplateValidator TemplateValidator { get; }

        private readonly ITemplateParser _templateParser;

        public DefaultTemplateValueAccessor(TemplateDescriptor templateDescriptor)
        {
            _templateDescriptor = templateDescriptor;
            TemplateValidator = new DefaultTemplateValidator();
            _templateParser = new TemplateParser(templateDescriptor, TemplateValidator);
        }

        public IEnumerable<object> GetCollection(string template)
        {
            if (_templateParser.TryParse(template, out var templateModel))
            {
                if (!templateModel.IsCollection)
                {
                    throw new MemberAccessException($"Cannot get value from {_templateDescriptor.SingleTemplateEntities[templateModel.RootName]} because it is not a collection. Use GetValue method instead");
                }

                IEnumerable<object> collection = _templateDescriptor.CollectionTemplateEntities[templateModel.RootName];

                return collection.Select(x => InvokeChain(x, templateModel.PropertiesInvokationChain));
                
            }
            throw new ArgumentException($"No appropriate parameter for input tamplate: {template}");
        }

        public object GetValue(string template)
        {
            if(_templateParser.TryParse(template, out var templateModel))
            {
                if (templateModel.IsCollection)
                {
                    throw new MemberAccessException($"Cannot get value from {_templateDescriptor.CollectionTemplateEntities[templateModel.RootName]} because it is a collection. Use GetCollection method instead");
                }

                object currentMember = _templateDescriptor.SingleTemplateEntities[templateModel.RootName];
                currentMember = InvokeChain(currentMember, templateModel.PropertiesInvokationChain);
                return currentMember;
            }
            throw new ArgumentException($"No appropriate parameter for input tamplate: {template}");
        }

        private object InvokeChain(object member, IEnumerable<string> propertyChain)
        {
            foreach (var propertyName in propertyChain)
            {
                member = GetPropertyValue(member, propertyName);
            }

            return member;
        }

        private object GetPropertyValue(object currentMember, string propertyName)
        {
            if (currentMember == null)
                return null;

            var type = currentMember.GetType();

            var property = type.GetProperty(propertyName);
            if (property == null)
                return null;

            return property.GetValue(currentMember);
        }

        public bool IsCollectionTemplate(string template)
        {
            return _templateParser.TryParse(template, out var templateModel) && templateModel.IsCollection;

        }
    }
}
