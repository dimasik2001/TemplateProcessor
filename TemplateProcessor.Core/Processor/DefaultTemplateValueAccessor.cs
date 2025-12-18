using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemplateProcessor.Abstractions;
using TemplateProcessor.Core.Models;
using TemplateProcessor.Core.Parsers;
using TemplateProcessor.Core.Parsers.Abstractions;
using TemplateProcessor.Core.TemplateStructureModels;

namespace TemplateProcessor.Core.Processor
{
    public class DefaultTemplateValueAccessor : ITemplateValueAccessor
    {
        private readonly TemplateDescriptor _templateDescriptor;
        private readonly ITemplateParser _templateParser;

        public DefaultTemplateValueAccessor(TemplateDescriptor templateDescriptor)
        {
            _templateDescriptor = templateDescriptor;
            _templateParser = new TemplateParser(templateDescriptor);
        }

        public IEnumerable<object> GetCollection(string template)
        {
            if (_templateParser.TryParse(template, out var templateModel))
            {
                if (!templateModel.InvokeInCollectionIterator)
                {
                    throw new MemberAccessException($"Cannot get value from {_templateDescriptor.TemplateEntities[templateModel.RootName]} because it is not a collection. Use GetValue method instead");
                }

                var collection = _templateDescriptor.TemplateEntities[templateModel.RootName] as IEnumerable<object>;
                return collection.Select(x => InvokeChain(x, templateModel.PropertiesInvokationChain.Select(p => p.Name)));
                
            }
            throw new ArgumentException($"No appropriate parameter for input tamplate: {template}");
        }

        public object GetValue(string template)
        {
            if(_templateParser.TryParse(template, out var templateModel))
            {
                if (templateModel.InvokeInCollectionIterator)
                {
                    throw new MemberAccessException($"Cannot get value from {_templateDescriptor.TemplateEntities[templateModel.RootName]} because it is a collection. Use GetCollection method instead");
                }

                object currentMember = _templateDescriptor.TemplateEntities[templateModel.RootName];
                currentMember = InvokeChain(currentMember, templateModel.PropertiesInvokationChain.Select(x => x.Name));
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
            return _templateParser.TryParse(template, out var templateModel) && (templateModel.InvokeInCollectionIterator || templateModel.PropertiesInvokationChain.Any(x => x.InvokeInCollectionIterator));

        }

    }
}
