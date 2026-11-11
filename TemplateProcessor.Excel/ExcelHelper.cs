using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Linq;

public class ExcelHelper
{  
    public static void ReplaceInExcel(SpreadsheetDocument document, string oldValue, string newValue)
    {
            var workbookPart = document.WorkbookPart;

            // ToList: SetCellValue may add a SharedStringTablePart to the workbook while we iterate.
            foreach (var worksheetPart in workbookPart.WorksheetParts.ToList())
            {
                var cells = worksheetPart.Worksheet.Descendants<Cell>();

                foreach (var cell in cells)
                {
                    string cellText = GetCellValue(cell, workbookPart);

                    if (cellText != null && cellText.Contains(oldValue))
                    {
                        string replaced = cellText.Replace(oldValue, newValue);
                        SetCellValue(cell, replaced, workbookPart);
                    }
                }
            }

            workbookPart.Workbook.Save();
    }

    /// <summary>
    /// Reads the shared string table at call time: it may be created later by <see cref="SetCellValue"/>
    /// (e.g. a workbook that had only inline strings).
    /// </summary>
    public static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        return GetCellValue(cell, workbookPart.SharedStringTablePart?.SharedStringTable);
    }

    public static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        // Inline strings (<c t="inlineStr"><is><t>...</t></is></c>) are written by many generators instead of shared strings.
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText;
        }

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

        // The cell becomes a shared string, so drop the inline value if it had one.
        cell.InlineString = null;
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