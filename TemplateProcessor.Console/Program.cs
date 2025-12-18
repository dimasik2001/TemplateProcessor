// See https://aka.ms/new-console-template for more information
using TemplateProcessor.Core.Processor;
using TemplateProcessor.Core.TemplateStructureModels;

Console.WriteLine("Hello, World!");
Console.WriteLine("Hello, World!");
var template = "{{obj.Prop1}}";

var valueAccessor = new DefaultTemplateValueAccessor(new TemplateDescriptor
{
    CollectionTemplateEntities = new Dictionary<string, IEnumerable<object>>
    {
        ["obj"] = new List<object>
        {
            new {Prop1 = "55"}
        }
    }
});

var value = valueAccessor.GetCollection(template);
foreach (var item in value)
{
    Console.WriteLine(item);
}
Console.WriteLine(value);