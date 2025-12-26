// See https://aka.ms/new-console-template for more information
using TemplateProcessor.Core.Processor;
using TemplateProcessor.Core.TemplateStructureModels;
using TemplateProcessor.Excel;

object i = 5.4545345345m;
Console.WriteLine(i.ToString());

//return;
var account = new { Name = "Степанxbr", INN = 12345, Date = 12.1324234234234m};
var pledges = new object[]
{
    new
    {
        Price = 15600,
        Owner = "Tomas",
        Name = "Car",
    },

    new
    {
        Price = 200_000,
        Owner = "Nick",
        Name = "house"
    }
};

var descriptor = new TemplateDescriptor();

descriptor.SingleTemplateEntities["Account"] = account;
//descriptor.SingleTemplateEntities["Report"] = report;
descriptor.CollectionTemplateEntities["Pledge"] = pledges;
descriptor.CollectionTemplateEntities["Pledger"] = Array.Empty<object>();// new[] {"First", "Second" };

descriptor.AddIndexesForAllCollections(i => i + 1);
//descriptor.CollectionTemplateEntities["Items"] = pledge;

var accessor = new DefaultTemplateValueAccessor(descriptor);
//var engine = new EpplusExcelTemplateEngine(accessor);

new OpenXmlExcelTemplateEngine(accessor, accessor.TemplateValidator).Render("Template.xlsx", "result.xlsx");

//Console.WriteLine("Hello, World!");
//Console.WriteLine("Hello, World!");
//var template = "{{obj.Prop1}}";

//var valueAccessor = new DefaultTemplateValueAccessor(new TemplateDescriptor
//{
//    CollectionTemplateEntities = new Dictionary<string, IEnumerable<object>>
//    {
//        ["obj"] = new List<object>
//        {
//            new {Prop1 = "55"}
//        }
//    }
//});

//var value = valueAccessor.GetCollection(template);
//foreach (var item in value)
//{
//    Console.WriteLine(item);
//}
//Console.WriteLine(value);