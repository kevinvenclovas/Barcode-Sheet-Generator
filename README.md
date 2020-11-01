# Barcode-Sheet Generator

Easy generate Barcode-Sheets for print or online

![Barcode-Sheets](https://i.imgur.com/rux312K.png)

## Usage
### Best pratices
Generate Barcode-Sheet with simple Settings and try every settings


### Generate Simple Barcode-Sheet
```c#
byte[] sheet = new BarcodeSheetBuilder(new List<string> { "code1", "code2" }, PageSize.A4, 2, 1).Build();
```

### Sheet with special column widths 
```c#
//Sheet columns  must match cellWidths.Count() 
//Error on cellWidths.Count() != columns 

int columns = 3;
int rows = 1;
float cellWidths = new float[] { 50, 30, 20 };

byte[] sheet = new BarcodeSheetBuilder(new List<string> { "code1", "code2", "code3" }, PageSize.A4, columns, rows)
.SetCellPercentWidth(cellWidths)
.Build();

```

### Generate Barcode-Sheet
```c#
byte[] sheet = new BarcodeSheetBuilder(new List<string> { "code1", "code2", "code3" }, PageSize.A4, 2, 1)
.SetPageMargins(0, 0, 5, 5)
.SetColumnPercentWidth(new float[] { 50, 30, 20 })
.SetCellPadding(25)
.SetBarCodeHeight(80) // !Read Troubleshoot!
.WithBarcodeText()
.WithBorder()
.Build();
```


## Troubleshoot
Caution with the .SetBarCodeHeight(). When the Generated Barcode is higher than the cell height the barcode will not be show!
