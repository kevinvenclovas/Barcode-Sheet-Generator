using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BarcodeSheet
{
    public class BarcodeSheetBuilder
    {
        private const int FILE_DPI = 72;
        private List<string> _codes { get; set; } = null;
        private Rectangle _pageSize { get; set; } = null;
        private SheetMargin _sheetMargin { get; set; } = null;
        private int _column { get; set; } = 0;
        private int _row { get; set; } = 0;
        private float[] _columnPercentWidth { get; set; }
        private float _barcodeHeight { get; set; } = 0;
        private bool _withBarcodeText { get; set; } = true;
        private bool _withBorder { get; set; } = false;
        private float _callPadding { get; set; } = 25;
        private string _filePath { get; set; }

        public BarcodeSheetBuilder(List<string> codes, Rectangle pageSize, int column,int row)
        {
            _codes = codes;
            _pageSize = pageSize;
            _column = column;
            _row = row;
        }

        /// <summary>
        /// Set the cell widths in percent
        /// </summary>
        /// <param name="widths"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder SetColumnPercentWidth(float[] widths)
        {
            _columnPercentWidth = widths;
            return this;
        }

        /// <summary>
        /// Set the page margins in mm
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder SetPageMargins(float left, float right, float top, float bottom)
        {
            _sheetMargin = new SheetMargin(MMToPixel(left), MMToPixel(right), MMToPixel(top), MMToPixel(bottom));
            return this;
        }

        /// <summary>
        /// Set barcode height in mm. Keep in mind if the barcode is higher then the cell the barcode will not be displayed
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder SetBarCodeHeight(float height)
        {
            _barcodeHeight = MMToPixel(height);
            return this;
        }

        /// <summary>
        /// Display the code below the barcode
        /// </summary>
        /// <param name="withText"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder WithBarcodeText(bool withText = true)
        {
            _withBarcodeText = withText;
            return this;
        }

        /// <summary>
        /// Set cellpadding in mm
        /// </summary>
        /// <param name="padding"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder SetCellPadding(float padding)
        {
            _callPadding = MMToPixel(padding);
            return this;
        }
        /// <summary>
        /// Show border around cells
        /// </summary>
        /// <param name="withBorder"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder WithBorder(bool withBorder = true)
        {
            _withBorder = withBorder;
            return this;
        }
        /// <summary>
        /// Save sheet to file after build
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public BarcodeSheetBuilder ToFile(string filepath)
        {
            _filePath = filepath;
            return this;
        }

        public void Validate()
        {
            if (_pageSize == null) throw new BarcodeSheetException("Pagesize cannot be null");
            if (_codes == null || _codes.Count == 0) throw new BarcodeSheetException("Codes count must be greater than 0");
            if (_column < 1) throw new BarcodeSheetException("Column count must be greater than 0");
            if (_row < 1) throw new BarcodeSheetException("Row count must be greater than 0");
            if (_columnPercentWidth != null && _columnPercentWidth.Length != _column) throw new BarcodeSheetException("Column count and percentage distribution of columns must be the same size");
            if (_columnPercentWidth != null && _columnPercentWidth.Sum() != 100) throw new BarcodeSheetException("Sum of column percentage width must be 100");

            if (_sheetMargin != null)
            {
                _sheetMargin.Validate();
            }
            else
            {
                _sheetMargin = new SheetMargin();
            }

            if (_barcodeHeight > 86.5f && _withBarcodeText) _barcodeHeight = 86.5f;

            if (_columnPercentWidth == null)
            {
                float tableWith = 100f / (float)_column;
                _columnPercentWidth = Enumerable.Repeat(tableWith, _column).ToArray();
            }

            _codes.AddRange(Enumerable.Repeat("", _codes.Count % _column).ToArray());

        }

        public byte[] Build()
        {
            Validate();

            Document document = new Document(_pageSize, _sheetMargin.Left, _sheetMargin.Right, _sheetMargin.Top, _sheetMargin.Bottom);
            PdfPTable table = new PdfPTable(_column);

            table.TotalWidth = _pageSize.Width;
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            table.SetWidths(_columnPercentWidth);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                float rowHeight = ((_pageSize.Height - (_sheetMargin.Top + _sheetMargin.Bottom)) / (float)_row) - 0.001f;
                float barHeight = ((rowHeight - (_callPadding * 2)) * (_barcodeHeight / 100f)) / 2f;
               
                foreach (string code in _codes)
                {
                    Barcode128 code128 = new Barcode128();
                    code128.GenerateChecksum = true;
                    if (!_withBarcodeText) code128.Font = null;

                    PdfPCell cell = new PdfPCell();
                    cell.FixedHeight = rowHeight;
                    if(!_withBorder) cell.Border = PdfPCell.NO_BORDER;
                   
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cell.Padding = _callPadding;

                    if (!code.Equals(""))
                    {
                        code128.Code = code;
                        if(_barcodeHeight>0) code128.BarHeight = barHeight;
                        code128.X = 1;
                        Image img = code128.CreateImageWithBarcode(writer.DirectContent, null, null);
                        cell.AddElement(img);
                    }

                    table.AddCell(cell);
                }

                document.Add(table);
                document.Close();

                byte[] pdf = ms.ToArray();
                if (_filePath != null) SaveToFile(_filePath, pdf);

                return pdf;
            }

        }
        

        private float MMToPixel(float mm) 
        {
            return (mm * FILE_DPI) / 25.4f;
        }

        public bool SaveToFile(string filePath, byte[] byteArray)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }
        }
    }

    

    public class SheetMargin
    {
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }

        public SheetMargin()
        {

        }

        public SheetMargin(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public void Validate()
        {
            if (Left < 0) throw new Exception("Sheet-Margin-Left cannot be negative");
            if (Right < 0) throw new Exception("Sheet-Margin-Right cannot be negative");
            if (Top < 0) throw new Exception("Sheet-Margin-Top cannot be negative");
            if (Bottom < 0) throw new Exception("Sheet-Margin-Bottom cannot be negative");
        }
    }

    public class BarcodeSheetException : Exception
    {
        public BarcodeSheetException(string message) : base(message)
        {

        }
    }
}
