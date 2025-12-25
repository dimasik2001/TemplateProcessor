using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void Render(Stream documentStream)
        {
            using (var document = SpreadsheetDocument.Open(documentStream, true))
            {
                var workbookPart = document.WorkbookPart;
                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

                foreach (var worksheetPart in workbookPart.WorksheetParts)
                {
                    var sheet = worksheetPart.Worksheet;
                    var sheetData = sheet.GetFirstChild<SheetData>();
                    var rows = worksheetPart.Worksheet.Descendants<Row>();
                    var rowsWithCollectionTemplate = new HashSet<Row>();
                    foreach (var row in rows)
                    {
                        foreach (var cell in row.Elements<Cell>())
                        {
                            string cellText = ExcelHelper.GetCellValue(cell, sharedStringTable);

                            if (cellText != null && _templateValidator.HasTemplates(cellText, out var templates))
                            {


                                var replaced = cellText;
                                foreach (var template in templates.Where(t => !_valueAccessor.IsCollectionTemplate(t)))
                                {
                                    try
                                    {
                                        var value = _valueAccessor.GetValue(template);
                                        if (value != null)
                                        {
                                            replaced = replaced.Replace(template, value.ToString());
                                        }
                                    }
                                    catch (ArgumentException) { }
                                }
                                ExcelHelper.SetCellValue(cell, replaced, workbookPart);

                                if (templates.Any(t => _valueAccessor.IsCollectionTemplate(t)))
                                {
                                    rowsWithCollectionTemplate.Add(row);
                                }


                            }
                        }
                    }

                    foreach (var templateRow in rowsWithCollectionTemplate)
                    {
                        var templatedCellsOfTemplatedRow = new Dictionary<Cell, IEnumerable<string>>();
                        foreach (var cell in templateRow.Elements<Cell>())
                        {
                            string cellText = ExcelHelper.GetCellValue(cell, sharedStringTable);

                            if (cellText != null && _templateValidator.HasTemplates(cellText, out var templates) && templates.Any(t => _valueAccessor.IsCollectionTemplate(t)))
                            {
                                templatedCellsOfTemplatedRow.Add(cell, templates.Where(t => _valueAccessor.IsCollectionTemplate(t)).ToArray());
                            }

                        }
                        var collectionTemplatesInRow = templatedCellsOfTemplatedRow.SelectMany(x => x.Value);
                        var templatePathToCollectionMap = collectionTemplatesInRow.Distinct().Select(x => new KeyValuePair<string, List<object>>(x, GetCollectionOrDefault(x))).Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
                        var countOfNewRows = templatePathToCollectionMap.Max(x => x.Value.Count);
                        var rowsToCollectionTemplate = new List<Row>(countOfNewRows + 1);
                        rowsToCollectionTemplate.Add(templateRow);

                        foreach (var itemIndex in Enumerable.Range(0, countOfNewRows - 1))
                        {
                            var newRow = DuplicateRowExact(worksheetPart, templateRow);
                            rowsToCollectionTemplate.Add(newRow);
                        }

                        for (int i = 0; i < rowsToCollectionTemplate.Count; i++)
                        {
                            foreach (var cell in rowsToCollectionTemplate[i].Elements<Cell>())
                            {
                                string cellText = ExcelHelper.GetCellValue(cell, sharedStringTable);

                                if (cellText != null && _templateValidator.HasTemplates(cellText, out var templates))
                                {
                                    var replaceText = cellText;
                                    foreach (var template in templates.Where(templatePathToCollectionMap.ContainsKey))
                                    {
                                        var itemOfCollection = templatePathToCollectionMap[template][i];

                                        if (itemOfCollection != null)
                                        {
                                            replaceText = replaceText.Replace(template, itemOfCollection.ToString());
                                        }
                                    }

                                    ExcelHelper.SetCellValue(cell, replaceText, workbookPart);
                                }
                            }
                        }
                    }

                }

                workbookPart.Workbook.Save();
            }
        }

        private List<object> GetCollectionOrDefault(string x)
        {
            try
            {
                return _valueAccessor.GetCollection(x).ToList();
            }
            catch
            {
                return null;
            }

        }

        static Row DuplicateRowExact(
     WorksheetPart worksheetPart,
     Row sourceRow)
        {
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            var mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().FirstOrDefault();

            uint sourceRowIndex = sourceRow.RowIndex.Value;
            uint newRowIndex = sourceRowIndex + 1;

            // 1. Сдвигаем строки ниже
            foreach (var row in sheetData.Elements<Row>()
                .Where(r => r.RowIndex.Value >= newRowIndex)
                .OrderByDescending(r => r.RowIndex.Value))
            {
                row.RowIndex.Value++;

                foreach (var cell in row.Elements<Cell>())
                {
                    string col = GetColumnName(cell.CellReference);
                    cell.CellReference = col + row.RowIndex.Value;
                }
            }

            // 2. Сдвигаем merge ниже
            if (mergeCells != null)
            {
                foreach (var merge in mergeCells.Elements<MergeCell>())
                {
                    var (startCol, startRow, endCol, endRow) = ParseMerge(merge.Reference);

                    if (startRow >= newRowIndex)
                    {
                        merge.Reference = $"{startCol}{startRow + 1}:{endCol}{endRow + 1}";
                    }
                }
            }

            // 3. Клонируем строку
            var newRow = (Row)sourceRow.CloneNode(true);
            newRow.RowIndex = newRowIndex;

            foreach (var cell in newRow.Elements<Cell>())
            {
                string col = GetColumnName(cell.CellReference);
                cell.CellReference = col + newRowIndex;
            }

            sheetData.InsertAfter(newRow, sourceRow);

            // 4. Копируем merge исходной строки
            if (mergeCells != null)
            {
                foreach (var merge in mergeCells.Elements<MergeCell>().ToList())
                {
                    var (startCol, startRow, endCol, endRow) = ParseMerge(merge.Reference);

                    if (startRow == sourceRowIndex && endRow == sourceRowIndex)
                    {
                        mergeCells.Append(new MergeCell
                        {
                            Reference = $"{startCol}{newRowIndex}:{endCol}{newRowIndex}"
                        });
                    }
                }
            }

            return newRow;
        }

        static string GetColumnName(StringValue cellReference)
        {
            return new string(cellReference.Value.Where(char.IsLetter).ToArray());
        }

        static (string startCol, uint startRow, string endCol, uint endRow)
            ParseMerge(StringValue reference)
        {
            var parts = reference.Value.Split(':');

            ParseCell(parts[0], out var startCol, out var startRow);
            ParseCell(parts[1], out var endCol, out var endRow);

            return (startCol, startRow, endCol, endRow);
        }

        static void ParseCell(string cellRef, out string column, out uint row)
        {
            column = new string(cellRef.Where(char.IsLetter).ToArray());
            row = uint.Parse(new string(cellRef.Where(char.IsDigit).ToArray()));
        }
    }
}
