# تعديلات مطلوبة في AddEditUmrahPackageForm.cs

## ✅ تم بالفعل:
1. تعديل Entity (UmrahPackage.cs) ✓
2. إنشاء Migration ✓

## التعديلات المطلوبة في الفورم:

### 1. إضافة الحقول في قسم الأسعار (بعد سطر 402 - بعد Fast Train):

```csharp
// Row 5: Buses Count + Bus Price SAR
AddLabel(mainPanel, "عدد الباصات:", 30, yPosition);
_numBusesCount = AddNumericUpDown(mainPanel, 200, yPosition, 0, 50);
_numBusesCount.ValueChanged += (s, e) => CalculateTotals();

AddLabel(mainPanel, "سعر الباص:", 490, yPosition);
_numBusPriceSAR = AddNumericUpDown(mainPanel, 620, yPosition, 0, 100000, 2);
_numBusPriceSAR.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ر.س", 840, yPosition);
_lblBusEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 950, yPosition);
yPosition += 60;

// Row 6: Gifts Price
AddLabel(mainPanel, "سعر الهدايا:", 30, yPosition);
_numGiftsPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
_numGiftsPrice.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
yPosition += 60;

// Row 7: Other Expenses + Notes
AddLabel(mainPanel, "مصروفات أخرى:", 30, yPosition);
_numOtherExpenses = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
_numOtherExpenses.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);

AddLabel(mainPanel, "ملاحظات:", 540, yPosition);
_txtOtherExpensesNotes = AddTextBox(mainPanel, 660, yPosition, 360);
_txtOtherExpensesNotes.PlaceholderText = "تفاصيل المصروفات الأخرى...";
yPosition += 60;

// Row 8: Profit Margin in EGP (not percentage)
AddLabel(mainPanel, "هامش الربح:", 30, yPosition);
_numProfitMarginEGP = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
_numProfitMarginEGP.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
yPosition += 80;
```

### 2. تعديل قسم المشرف (سطر حوالي 415):

**استبدال:**
```csharp
AddLabel(mainPanel, "مصاريف المشرف:", 580, yPosition);
_numSupervisorExpenses = AddNumericUpDown(mainPanel, 750, yPosition, 0, 100000, 2);
_numSupervisorExpenses.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ج.م", 970, yPosition);
```

**بـ:**
```csharp
AddLabel(mainPanel, "مصاريف المشرف:", 580, yPosition);
_numSupervisorExpensesSAR = AddNumericUpDown(mainPanel, 750, yPosition, 0, 100000, 2);
_numSupervisorExpensesSAR.ValueChanged += (s, e) => CalculateTotals();
AddCurrencyLabel(mainPanel, "ر.س", 970, yPosition);
_lblSupervisorEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 1000, yPosition); // إضافة
```

### 3. تعديل CalculateTotals() (حوالي سطر 680):

**استبدال الدالة بالكامل:**
```csharp
private void CalculateTotals()
{
    try
    {
        decimal exchangeRate = _numExchangeRate.Value;
        decimal visaSAR = _numVisaPriceSAR.Value;
        decimal fastTrainSAR = _numFastTrainSAR.Value;
        int busesCount = (int)_numBusesCount.Value;
        decimal busPriceSAR = _numBusPriceSAR.Value;
        decimal supervisorExpensesSAR = _numSupervisorExpensesSAR.Value;
        int numberOfPersons = (int)_numPersons.Value;
        
        // Convert SAR to EGP
        decimal visaEGP = visaSAR * exchangeRate;
        decimal fastTrainEGP = fastTrainSAR * exchangeRate;
        decimal busEGP = busPriceSAR * exchangeRate * busesCount;
        decimal supervisorEGP = supervisorExpensesSAR * exchangeRate;
        
        _lblVisaEGP.Text = $"= {visaEGP:N2} ج.م";
        _lblFastTrainEGP.Text = $"= {fastTrainEGP:N2} ج.م";
        _lblBusEGP.Text = $"= {busEGP:N2} ج.م";
        _lblSupervisorEGP.Text = $"= {supervisorEGP:N2} ج.م";
        
        // Total Costs per person
        decimal totalCosts = visaEGP +
                            _numAccommodationTotal.Value +
                            _numBarcodePrice.Value +
                            _numFlightPrice.Value +
                            fastTrainEGP +
                            busEGP +
                            _numGiftsPrice.Value +
                            _numOtherExpenses.Value +
                            _numCommission.Value +
                            supervisorEGP;
        
        _lblTotalCosts.Text = $"{totalCosts:N2} ج.م";
        
        // ✅ حساب سعر البيع بناءً على التكاليف + هامش الربح بالجنيه
        decimal profitMarginEGP = _numProfitMarginEGP.Value;
        decimal sellingPrice = totalCosts + profitMarginEGP;
        _numSellingPrice.Value = sellingPrice;
        
        // Total Revenue
        decimal totalRevenue = sellingPrice * numberOfPersons;
        _lblTotalRevenue.Text = $"{totalRevenue:N2} ج.م";
        
        // Net Profit (Total)
        decimal netProfit = totalRevenue - (totalCosts * numberOfPersons);
        _lblNetProfit.Text = $"{netProfit:N2} ج.م";
        _lblNetProfit.ForeColor = netProfit >= 0 ? ColorScheme.Success : Color.FromArgb(211, 47, 47);
        
        // Profit Margin Percentage
        decimal profitMarginPercent = totalRevenue > 0 ? (netProfit / totalRevenue * 100) : 0;
        _lblProfitMargin.Text = $"{profitMarginPercent:N2} %";
        _lblProfitMargin.ForeColor = profitMarginPercent >= 0 ? ColorScheme.Info : Color.FromArgb(211, 47, 47);
        
        // Net Profit Per Person
        decimal netProfitPerPerson = numberOfPersons > 0 ? (netProfit / numberOfPersons) : 0;
        _lblNetProfitPerPerson.Text = $"{netProfitPerPerson:N2} ج.م";
        _lblNetProfitPerPerson.ForeColor = netProfitPerPerson >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(211, 47, 47);
    }
    catch
    {
        // Silent fail
    }
}
```

### 4. تعديل LoadPackageDataAsync() (حوالي سطر 750):

**إضافة بعد:**
```csharp
_numFlightPrice.Value = package.FlightPrice;
_numFastTrainSAR.Value = package.FastTrainPriceSAR;
```

**الأسطر الجديدة:**
```csharp
_numBusesCount.Value = package.BusesCount;
_numBusPriceSAR.Value = package.BusPriceSAR;
_numGiftsPrice.Value = package.GiftsPrice;
_numOtherExpenses.Value = package.OtherExpenses;
_txtOtherExpensesNotes.Text = package.OtherExpensesNotes ?? "";
_numProfitMarginEGP.Value = package.ProfitMarginEGP;
```

**استبدال:**
```csharp
_numSupervisorExpenses.Value = package.SupervisorExpenses;
```

**بـ:**
```csharp
_numSupervisorExpensesSAR.Value = package.SupervisorExpensesSAR;
```

### 5. تعديل BtnSave_Click() (حوالي سطر 800):

**إضافة في كائن package بعد:**
```csharp
FastTrainPriceSAR = _numFastTrainSAR.Value,
```

**الخصائص الجديدة:**
```csharp
BusesCount = (int)_numBusesCount.Value,
BusPriceSAR = _numBusPriceSAR.Value,
GiftsPrice = _numGiftsPrice.Value,
OtherExpenses = _numOtherExpenses.Value,
OtherExpensesNotes = string.IsNullOrWhiteSpace(_txtOtherExpensesNotes.Text) ? null : _txtOtherExpensesNotes.Text.Trim(),
ProfitMarginEGP = _numProfitMarginEGP.Value,
```

**استبدال:**
```csharp
SupervisorExpenses = _numSupervisorExpenses.Value,
```

**بـ:**
```csharp
SupervisorExpensesSAR = _numSupervisorExpensesSAR.Value,
```

## ملاحظات مهمة:
1. جميع التعديلات يجب أن تكون في الملف: AddEditUmrahPackageForm.cs
2. احذف `_numProfitMarginPercent` واستبدله بـ `_numProfitMarginEGP`
3. جميع المصروفات الجديدة يجب أن تضاف في حساب `totalCosts`
4. سعر البيع الآن = التكاليف + هامش الربح (مش نسبة مئوية)

## الخطوات التالية:
1. ✅ تشغيل Migration:
   ```bash
   dotnet ef database update
   ```
   
2. ✅ اختبار الفورم
3. ✅ التأكد من حفظ البيانات بشكل صحيح
