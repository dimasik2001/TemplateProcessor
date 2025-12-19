using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Linq;

public class ExcelHelper
{  
    public static void ReplaceInExcel(SpreadsheetDocument document, string oldValue, string newValue)
    {
            var workbookPart = document.WorkbookPart;
            var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

            foreach (var worksheetPart in workbookPart.WorksheetParts)
            {
                var cells = worksheetPart.Worksheet.Descendants<Cell>();

                foreach (var cell in cells)
                {
                    string cellText = GetCellValue(cell, sharedStringTable);

                    if (cellText != null && cellText.Contains(oldValue))
                    {
                        string replaced = cellText.Replace(oldValue, newValue);
                        SetCellValue(cell, replaced, workbookPart);
                    }
                }
            }

            workbookPart.Workbook.Save();
    }

    public static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell.CellValue == null)
            return null;

        string value = cell.CellValue.InnerText;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            return sharedStringTable
                .Elements<SharedStringItem>()
                .ElementAt(int.Parse(value))
                .InnerText;
        }

        return value;
    }

    public static void SetCellValue(Cell cell, string text, WorkbookPart workbookPart)
    {
        var sharedStringTablePart = workbookPart.SharedStringTablePart
            ?? workbookPart.AddNewPart<SharedStringTablePart>();

        if (sharedStringTablePart.SharedStringTable == null)
            sharedStringTablePart.SharedStringTable = new SharedStringTable();

        int index = InsertSharedStringItem(text, sharedStringTablePart);

        cell.CellValue = new CellValue(index.ToString());
        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
    }

    static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
    {
        int i = 0;

        foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
        {
            if (item.InnerText == text)
                return i;
            i++;
        }

        shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
        shareStringPart.SharedStringTable.Save();

        return i;
    }
}