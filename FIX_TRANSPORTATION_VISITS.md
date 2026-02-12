# 🔧 إصلاح مشكلة عدم حفظ بيانات المزارات في النقل

## 📋 المشكلة
عند استيراد المزارات من الخطوة 2 (البرنامج اليومي) إلى الخطوة 3 (النقل)، البيانات تظهر في الجدول لكن **لا يتم حفظها في قاعدة البيانات** لأن:
1. اسم المزار ورقم اليوم لا يتم قراءتهم من الجدول في SaveCurrentStepData
2. TripTransportation Entity لا يحتوي على حقول لحفظ اسم المزار ورقم اليوم

## ✅ الحل

### الخطوة 1: تحديث TripTransportation Entity
يجب إضافة حقلين جديدين لحفظ اسم المزار ورقم اليوم:

```csharp
// في ملف: Domain/Entities/TripTransportation.cs
// أضف هذه الخصائص:

/// <summary>
/// اسم المزار المرتبط بهذا النقل (اختياري)
/// </summary>
public string? VisitName { get; set; }

/// <summary>
/// رقم اليوم في البرنامج (اختياري)
/// </summary>
public int? ProgramDayNumber { get; set; }
```

### الخطوة 2: إنشاء Migration
بعد تحديث الـ Entity، اعمل migration جديدة:

```powershell
dotnet ef migrations add AddVisitInfoToTransportation
dotnet ef database update
```

### الخطوة 3: تحديث SaveCurrentStepData في AddEditTripForm.cs

ابحث عن case 2 في دالة SaveCurrentStepData واستبدل الكود الحالي بهذا:

```csharp
case 2: // النقل
    _trip.Transportation.Clear();
    if (_transportationGrid != null)
    {
        Console.WriteLine($"[SaveCurrentStepData - Step 2] Transportation Grid Rows: {_transportationGrid.Rows.Count}");
        
        foreach (DataGridViewRow row in _transportationGrid.Rows)
        {
            if (row.IsNewRow) continue;
            
            // ✅ استخراج اسم المزار ورقم اليوم من الأعمدة
            var visitName = row.Cells["VisitName"].Value?.ToString() ?? "";
            var dayNumber = 0;
            if (int.TryParse(row.Cells["DayNumber"].Value?.ToString(), out var dn))
                dayNumber = dn;
            
            var typeText = row.Cells["Type"].Value?.ToString() ?? "أتوبيس";
            var type = typeText switch
            {
                "أتوبيس" => TransportationType.Bus,
                "ميني باص" => TransportationType.MiniBus,
                "كوستر" => TransportationType.Coaster,
                "هاي أس" => TransportationType.HiAce,
                "ملاكي" => TransportationType.Car,
                "طائرة" => TransportationType.Plane,
                "قطار" => TransportationType.Train,
                _ => TransportationType.Bus
            };
            
            DateTime? transportDate = null;
            if (DateTime.TryParse(row.Cells["TransportDate"].Value?.ToString(), out var dt))
                transportDate = dt;
            
            // ✅ حفظ اسم المزار في حقل Route إذا لم يكن موجوداً
            var route = row.Cells["Route"].Value?.ToString();
            
            // إذا كان المسار فارغ، استخدم اسم المزار
            if (string.IsNullOrWhiteSpace(route) && !string.IsNullOrWhiteSpace(visitName))
            {
                route = $"نقل إلى {visitName}";
            }
            
            var transportation = new TripTransportation
            {
                Type = type,
                TransportDate = transportDate,
                Route = route,
                VehicleModel = row.Cells["VehicleModel"].Value?.ToString(),
                NumberOfVehicles = Convert.ToInt32(row.Cells["NumberOfVehicles"].Value ?? 1),
                SeatsPerVehicle = Convert.ToInt32(row.Cells["SeatsPerVehicle"].Value ?? 50),
                ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0),
                CostPerVehicle = Convert.ToDecimal(row.Cells["CostPerVehicle"].Value ?? 0),
                TourLeaderTip = Convert.ToDecimal(row.Cells["TourLeaderTip"].Value ?? 0),
                DriverTip = Convert.ToDecimal(row.Cells["DriverTip"].Value ?? 0),
                SupplierName = row.Cells["SupplierName"].Value?.ToString(),
                DriverPhone = row.Cells["DriverPhone"].Value?.ToString(),
                
                // ✅ حفظ معلومات المزار والبرنامج
                VisitName = visitName,
                ProgramDayNumber = dayNumber > 0 ? dayNumber : null
            };
            
            _trip.Transportation.Add(transportation);
            
            Console.WriteLine($"[SaveCurrentStepData - Step 2] Added Transportation: VisitName={visitName}, DayNumber={dayNumber}, Route={route}, Cost={transportation.CostPerVehicle}");
        }
        
        Console.WriteLine($"[SaveCurrentStepData - Step 2] Total Transportation Saved: {_trip.Transportation.Count}");
    }
    break;
```

### الخطوة 4: تحديث RestoreStep3Data

ابحث عن دالة RestoreStep3Data وتأكد من استرجاع البيانات الجديدة:

```csharp
private void RestoreStep3Data()
{
    if (_trip == null || _transportationGrid == null) return;
    
    _transportationGrid.Rows.Clear();
    foreach (var transport in _trip.Transportation)
    {
        var typeText = transport.Type switch
        {
            TransportationType.Bus => "أتوبيس",
            TransportationType.MiniBus => "ميني باص",
            TransportationType.Coaster => "كوستر",
            TransportationType.HiAce => "هاي أس",
            TransportationType.Car => "ملاكي",
            TransportationType.Plane => "طائرة",
            TransportationType.Train => "قطار",
            _ => "أتوبيس"
        };
        
        // حساب السعر/فرد مع الإكراميات
        decimal totalCost = transport.CostPerVehicle + transport.TourLeaderTip + transport.DriverTip;
        decimal costPerPerson = transport.ParticipantsCount > 0 
            ? totalCost / transport.ParticipantsCount 
            : 0;
        
        _transportationGrid.Rows.Add(
            transport.VisitName ?? "",                    // ✅ اسم المزار (من الحقل الجديد)
            transport.ProgramDayNumber ?? 0,              // ✅ رقم اليوم (من الحقل الجديد)
            typeText,                                     // النوع
            transport.TransportDate?.ToString("yyyy-MM-dd") ?? "", // التاريخ
            transport.Route ?? "",                        // المسار
            transport.VehicleModel ?? "",                 // الموديل
            transport.SeatsPerVehicle,                    // المقاعد
            transport.NumberOfVehicles,                   // عدد المركبات
            transport.ParticipantsCount,                  // عدد الأفراد
            transport.CostPerVehicle,                     // التكلفة الإجمالية
            transport.TourLeaderTip,                      // إكرامية التور ليدر
            transport.DriverTip,                          // إكرامية السواق
            costPerPerson.ToString("N2"),                 // السعر/فرد
            transport.SupplierName ?? "",                 // المورد
            transport.DriverPhone ?? ""                   // هاتف السائق
        );
    }
}
```

## 📝 ملاحظات مهمة

1. **الترتيب**: يجب تطبيق التعديلات بالترتيب التالي:
   - أولاً: تحديث Entity
   - ثانياً: عمل Migration
   - ثالثاً: تحديث SaveCurrentStepData
   - رابعاً: تحديث RestoreStep3Data

2. **الاختبار**: بعد التطبيق، جرب:
   - إنشاء رحلة جديدة
   - أضف برنامج يومي مع مزارات
   - انتقل للنقل واضغط "تحديث المزارات"
   - احفظ الرحلة
   - أعد فتح الرحلة للتأكد من حفظ البيانات

3. **التوافق**: التعديلات متوافقة مع البيانات الموجودة - الحقول الجديدة nullable

## 🎯 النتيجة المتوقعة

بعد التطبيق:
✅ المزارات تُستورد من الخطوة 2 للخطوة 3
✅ بيانات المزارات تُحفظ في قاعدة البيانات
✅ عند إعادة فتح الرحلة، البيانات تظهر كما هي
✅ يمكن تسعير كل مزار بشكل منفصل
✅ التكاليف تدخل في حسابات الرحلة بشكل صحيح
