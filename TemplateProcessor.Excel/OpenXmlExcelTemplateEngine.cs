using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TemplateProcessor.Abstractions;
using TemplateProcessor.Core.Processor;

namespace TemplateProcessor.Excel
{
    public class OpenXmlExcelTemplateEngine
    {
        private readonly ITemplateValueAccessor _valueAccessor;
        private readonly ITemplateValidator _templateValidator;

        public OpenXmlExcelTemplateEngine(ITemplateValueAccessor valueAccessor, ITemplateValidator templateValidator)
        {
            _valueAccessor = valueAccessor;
            _templateValidator = templateValidator;
        }

        public void Render(string templatePath, string outputPath)
        {
            using (var ms = new MemoryStream())
            {

                File.OpenRead(templatePath).CopyTo(ms);

                Render(ms);
                ms.WriteTo(File.Create(outputPath));
            }
        }

        private void Render(Stream documentStream)
        {
            using (var document = SpreadsheetDocument.Open(documentStream, true))
            {
                var sheet = document.WorkbookPart.WorksheetParts.First().Worksheet;
                var sheetData = sheet.GetFirstChild<SheetData>();

                var workbookPart = document.WorkbookPart;
                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

                foreach (var worksheetPart in workbookPart.WorksheetParts)
                {
                    var cells = worksheetPart.Worksheet.Descendants<Cell>();

                    foreach (var cell in cells)
                    {
                        string cellText = ExcelHelper.GetCellValue(cell, sharedStringTable);

                        if (cellText != null && _templateValidator.HasTemplates(cellText, out var templates))
                        {
                            var replaced = cellText;
                            foreach (var template in templates)
                            {
                                replaced = replaced.Replace(template, _valueAccessor.GetValue(template).ToString());
                            }
                            ExcelHelper.SetCellValue(cell, replaced, workbookPart);
                        }
                    }
                }

                workbookPart.Workbook.Save();
            }
        }
    }
}
