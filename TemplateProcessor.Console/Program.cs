// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2021.ExtLinks2021;
using TemplateProcessor.Core.Processor;
using TemplateProcessor.Core.TemplateStructureModels;
using TemplateProcessor.Excel;


var customer = new { Name = "Степан", INN = 12345};
var descriptor = new TemplateDescriptor();

descriptor.SingleTemplateEntities["Customer"] = customer;
//descriptor.SingleTemplateEntities["Report"] = report;

//descriptor.CollectionTemplateEntities["Items"] = items.Cast<object>();

var accessor = new DefaultTemplateValueAccessor(descriptor);
//var engine = new EpplusExcelTemplateEngine(accessor);

new OpenXmlExcelTemplateEngine(accessor, accessor.TemplateValidator).Render("Шаблон_ПКК.xlsx", "result.xlsx");

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