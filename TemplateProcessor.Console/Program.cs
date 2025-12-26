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
var random = new Random();


var people = new List<Person>
{
    new Person { Name = "Іван Петренко", Status = "Активний",   Number = GenerateIpn(random), Date = new DateTime(2025, 1, 1) },
    new Person { Name = "Олена Ковальчук", Status = "Неактивний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 2) },
    new Person { Name = "Андрій Шевченко", Status = "В очікуванні", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 3) },
    new Person { Name = "Марія Бондар", Status = "Активний",   Number = GenerateIpn(random), Date = new DateTime(2025, 1, 4) },
    new Person { Name = "Олексій Мельник", Status = "Активний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 5) },
    new Person { Name = "Наталія Савчук", Status = "Неактивний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 6) },
    new Person { Name = "Дмитро Ткаченко", Status = "В очікуванні", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 7) },
    new Person { Name = "Ірина Романюк", Status = "Активний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 8) },
    new Person { Name = "Віктор Лисенко", Status = "Неактивний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 9) },
    new Person { Name = "Світлана Мороз", Status = "В очікуванні", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 10) },
    new Person { Name = "Юрій Кравченко", Status = "Активний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 11) },
    new Person { Name = "Тетяна Поліщук", Status = "Активний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 12) },
    new Person { Name = "Роман Гриценко", Status = "Неактивний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 13) },
    new Person { Name = "Оксана Дяченко", Status = "В очікуванні", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 14) },
    new Person { Name = "Богдан Сидоренко", Status = "Активний", Number = GenerateIpn(random), Date = new DateTime(2025, 1, 15) }
};

var descriptor = new TemplateDescriptor();
descriptor.CollectionTemplateEntities["person"] = people;



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







static string GenerateIpn(Random random)
{
    return random.Next(100000000, 999999999).ToString() +
           random.Next(0, 9).ToString();
}
public class Person
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Number { get; set; } // ІПН
    public DateTime Date { get; set; }
}
