using iTextSharp.text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace BarcodeSheet.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CodesIsNull()
        {
            Assert.Throws<BarcodeSheetException>(() => new BarcodeSheetBuilder(null, PageSize.A4, 1,1).Build());
        }

        [Test]
        public void ColumnIsNull()
        {
            Assert.Throws<BarcodeSheetException>(() => new BarcodeSheetBuilder(new List<string> { "12345XX789XXX", "12345XX789XXX" }, PageSize.A4, 0, 1).Build());
        }

        [Test]
        public void RowIsNull()
        {
            Assert.Throws<BarcodeSheetException>(() => new BarcodeSheetBuilder(new List<string> { "12345XX789XXX", "12345XX789XXX" }, PageSize.A4, 1, 0).Build());
        }

        [Test]
        public void ColumnPercentWidthMatchNotColumnCount()
        {
            Assert.Throws<BarcodeSheetException>(() => new BarcodeSheetBuilder(new List<string> { "12345XX789XXX", "12345XX789XXX" }, PageSize.A4, 2, 1).SetColumnPercentWidth(new float[] { 50 }).Build());
        }

        [Test]
        public void ColumnPercentWidthSumNot100()
        {
            Assert.Throws<BarcodeSheetException>(() => new BarcodeSheetBuilder(new List<string> { "12345XX789XXX", "12345XX789XXX" }, PageSize.A4, 2, 1).SetColumnPercentWidth(new float[] { 50, 60 }).Build());
        }

        [Test]
        public void BuildPdf()
        {
            Assert.NotNull(new BarcodeSheetBuilder(new List<string> { "12345XX789XXX", "12345XX789XXX" }, PageSize.A4_LANDSCAPE, 1, 1).Build());
        }

        [Test]
        public void BuildPdfToFile()
        {
            string fileName = @"C:\\BarcodeSheet.pdf";

            List<string> codes = new List<string>()
            {
                "0346507217481",
                "3201645466849",
                "2816929509955",
                "1107712517150",
                "5397983903493",
                "6316029131071",
                "1575888627926",
                "0379448783098",
                "9004007865434",
                "4643156545986",
                "8620126369151",
                "4139826881275",
                "2021829181068",
                "6037524548422",
                "0471817608732",
                "0446139002124",
            };

            byte[] pdf = new BarcodeSheetBuilder(codes, PageSize.A4, 2, 6)
                 .SetPageMargins(0, 0, 5, 5)
                 //.SetColumnPercentWidth(new float[] { 50, 50 })
                 .SetCellPadding(5)
                 //.SetBarCodeHeight(30)
                 .WithBarcodeText()
                 //.WithBorder()
                 .ToFile(fileName)
                 .Build();
            Assert.IsTrue(File.Exists(fileName));
        }

       

    }
}