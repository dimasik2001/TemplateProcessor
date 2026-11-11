using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace TemplateProcessor.Excel
{
    /// <summary>
    /// Converts a legacy binary .xls (BIFF8) workbook to .xlsx in memory.
    /// Copies cell values, formulas, styles, fonts, number formats, merged regions, column widths and row heights.
    /// Images, charts, comments and hyperlinks are not converted.
    /// </summary>
    public static class XlsToXlsxConverter
    {
        // OLE2 compound document signature used by .xls files.
        private static readonly byte[] Ole2Signature = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        // HSSF "automatic" color index.
        private const short AutomaticColorIndex = 64;

        /// <summary>
        /// Checks the stream header for the OLE2 signature. The stream position is preserved.
        /// </summary>
        public static bool IsXls(Stream stream)
        {
            var position = stream.Position;
            try
            {
                stream.Position = 0;
                var header = new byte[Ole2Signature.Length];
                var read = 0;
                while (read < header.Length)
                {
                    var n = stream.Read(header, read, header.Length - read);
                    if (n == 0) return false;
                    read += n;
                }

                for (int i = 0; i < header.Length; i++)
                {
                    if (header[i] != Ole2Signature[i]) return false;
                }
                return true;
            }
            finally
            {
                stream.Position = position;
            }
        }

        /// <summary>
        /// Reads an .xls workbook from <paramref name="xlsStream"/> and returns a new stream with the .xlsx content, positioned at 0.
        /// </summary>
        public static MemoryStream Convert(Stream xlsStream)
        {
            // NPOI closes the stream it reads from, so read from a copy to keep the caller's stream usable.
            var copy = new MemoryStream();
            xlsStream.Position = 0;
            xlsStream.CopyTo(copy);
            copy.Position = 0;

            using (var source = new HSSFWorkbook(copy))
            using (var target = new XSSFWorkbook())
            {
                new Converter(source, target).Run();

                var result = new MemoryStream();
                target.Write(result, true);
                result.Position = 0;
                return result;
            }
        }

        private class Converter
        {
            private readonly HSSFWorkbook _source;
            private readonly XSSFWorkbook _target;
            private readonly HSSFPalette _palette;
            private readonly IDataFormat _targetFormat;
            private readonly Dictionary<short, ICellStyle> _styles = new Dictionary<short, ICellStyle>();
            private readonly Dictionary<short, IFont> _fonts = new Dictionary<short, IFont>();

            public Converter(HSSFWorkbook source, XSSFWorkbook target)
            {
                _source = source;
                _target = target;
                _palette = source.GetCustomPalette();
                _targetFormat = target.CreateDataFormat();
            }

            public void Run()
            {
                for (int s = 0; s < _source.NumberOfSheets; s++)
                {
                    var sourceSheet = _source.GetSheetAt(s);
                    var targetSheet = _target.CreateSheet(sourceSheet.SheetName);
                    CopySheet(sourceSheet, targetSheet);
                }

                if (_source.NumberOfSheets > 0)
                {
                    _target.SetActiveSheet(_source.ActiveSheetIndex);
                }
            }

            private void CopySheet(ISheet source, ISheet target)
            {
                target.DefaultRowHeight = source.DefaultRowHeight;
                target.DefaultColumnWidth = source.DefaultColumnWidth;
                target.DisplayGridlines = source.DisplayGridlines;

                var maxColumn = 0;
                for (int r = source.FirstRowNum; r <= source.LastRowNum; r++)
                {
                    var sourceRow = source.GetRow(r);
                    if (sourceRow == null)
                        continue;

                    var targetRow = target.CreateRow(r);
                    if (sourceRow.Height != source.DefaultRowHeight)
                    {
                        targetRow.Height = sourceRow.Height;
                    }
                    targetRow.ZeroHeight = sourceRow.ZeroHeight;
                    if (sourceRow.RowStyle != null)
                    {
                        targetRow.RowStyle = GetStyle(sourceRow.RowStyle);
                    }

                    foreach (var sourceCell in sourceRow.Cells)
                    {
                        var targetCell = targetRow.CreateCell(sourceCell.ColumnIndex);
                        CopyCell(sourceCell, targetCell);
                    }

                    maxColumn = Math.Max(maxColumn, sourceRow.LastCellNum);
                }

                for (int c = 0; c < maxColumn; c++)
                {
                    target.SetColumnWidth(c, source.GetColumnWidth(c));
                    target.SetColumnHidden(c, source.IsColumnHidden(c));
                    var columnStyle = source.GetColumnStyle(c);
                    if (columnStyle != null)
                    {
                        target.SetDefaultColumnStyle(c, GetStyle(columnStyle));
                    }
                }

                for (int i = 0; i < source.NumMergedRegions; i++)
                {
                    target.AddMergedRegion(source.GetMergedRegion(i));
                }

                var pane = source.PaneInformation;
                if (pane != null && pane.IsFreezePane())
                {
                    target.CreateFreezePane(pane.VerticalSplitPosition, pane.HorizontalSplitPosition,
                        pane.VerticalSplitLeftColumn, pane.HorizontalSplitTopRow);
                }
            }

            private void CopyCell(ICell source, ICell target)
            {
                target.CellStyle = GetStyle(source.CellStyle);

                switch (source.CellType)
                {
                    case CellType.String:
                        target.SetCellValue(source.StringCellValue);
                        break;
                    case CellType.Numeric:
                        target.SetCellValue(source.NumericCellValue);
                        break;
                    case CellType.Boolean:
                        target.SetCellValue(source.BooleanCellValue);
                        break;
                    case CellType.Error:
                        target.SetCellErrorValue(source.ErrorCellValue);
                        break;
                    case CellType.Formula:
                        CopyFormula(source, target);
                        break;
                }
            }

            private static void CopyFormula(ICell source, ICell target)
            {
                try
                {
                    target.SetCellFormula(source.CellFormula);
                }
                catch (Exception)
                {
                    // Formula cannot be represented (e.g. external references): keep its cached value.
                    switch (source.CachedFormulaResultType)
                    {
                        case CellType.String:
                            target.SetCellValue(source.StringCellValue);
                            break;
                        case CellType.Numeric:
                            target.SetCellValue(source.NumericCellValue);
                            break;
                        case CellType.Boolean:
                            target.SetCellValue(source.BooleanCellValue);
                            break;
                    }
                }
            }

            private ICellStyle GetStyle(ICellStyle source)
            {
                if (source == null)
                    return null;

                if (_styles.TryGetValue(source.Index, out var existing))
                    return existing;

                var style = (XSSFCellStyle)_target.CreateCellStyle();
                style.Alignment = source.Alignment;
                style.VerticalAlignment = source.VerticalAlignment;
                style.WrapText = source.WrapText;
                style.Indention = source.Indention;
                style.Rotation = source.Rotation;
                style.ShrinkToFit = source.ShrinkToFit;
                style.IsLocked = source.IsLocked;
                style.IsHidden = source.IsHidden;

                style.DataFormat = _targetFormat.GetFormat(source.GetDataFormatString());

                style.BorderTop = source.BorderTop;
                style.BorderBottom = source.BorderBottom;
                style.BorderLeft = source.BorderLeft;
                style.BorderRight = source.BorderRight;
                SetColor(source.TopBorderColor, c => style.SetTopBorderColor(c));
                SetColor(source.BottomBorderColor, c => style.SetBottomBorderColor(c));
                SetColor(source.LeftBorderColor, c => style.SetLeftBorderColor(c));
                SetColor(source.RightBorderColor, c => style.SetRightBorderColor(c));

                style.FillPattern = source.FillPattern;
                SetColor(source.FillForegroundColor, c => style.SetFillForegroundColor(c));
                SetColor(source.FillBackgroundColor, c => style.SetFillBackgroundColor(c));

                style.SetFont(GetFont(source.GetFont(_source)));

                _styles[source.Index] = style;
                return style;
            }

            private IFont GetFont(IFont source)
            {
                if (_fonts.TryGetValue(source.Index, out var existing))
                    return existing;

                var font = (XSSFFont)_target.CreateFont();
                font.FontName = source.FontName;
                font.FontHeight = source.FontHeight;
                font.IsBold = source.IsBold;
                font.IsItalic = source.IsItalic;
                font.IsStrikeout = source.IsStrikeout;
                font.Underline = source.Underline;
                font.TypeOffset = source.TypeOffset;
                font.Charset = source.Charset;
                SetColor(source.Color, c => font.SetColor(c));

                _fonts[source.Index] = font;
                return font;
            }

            private void SetColor(short index, Action<XSSFColor> apply)
            {
                if (index == AutomaticColorIndex)
                    return;

                var color = _palette.GetColor(index);
                if (color == null)
                    return;

                var rgb = color.RGB;
                // Opaque ARGB: a 3-byte RGB is stored with alpha 00.
                apply(new XSSFColor(new byte[] { 0xFF, rgb[0], rgb[1], rgb[2] }));
            }
        }
    }
}
