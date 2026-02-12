using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("================================================");
Console.WriteLine("        إضافة بيانات تجريبية للموردين");
Console.WriteLine("================================================");
Console.WriteLine();

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456");

using var context = new AppDbContext(optionsBuilder.Options);

Console.WriteLine("✅ تم الاتصال بقاعدة البيانات");
Console.WriteLine();

// حذف الموردين التجريبيين القدامى إن وجدوا
var existingSuppliers = await context.Suppliers
    .Where(s => s.SupplierCode.StartsWith("SUP"))
    .ToListAsync();

if (existingSuppliers.Any())
{
    Console.WriteLine($"🗑️  حذف {existingSuppliers.Count} مورد تجريبي قديم...");
    context.Suppliers.RemoveRange(existingSuppliers);
    await context.SaveChangesAsync();
}

Console.WriteLine("جاري إضافة الموردين الجدد...");
Console.WriteLine();

var suppliers = new List<Supplier>
{
    new Supplier
    {
        SupplierCode = "SUP001",
        SupplierName = "شركة النور للمستلزمات الفندقية",
        SupplierNameEn = "Al-Nour Hotel Supplies Co.",
        Phone = "0223456789",
        Mobile = "01012345678",
        Email = "info@alnour-supplies.com",
        Address = "15 شارع الجمهورية، وسط البلد",
        City = "القاهرة",
        Country = "مصر",
        TaxNumber = "123-456-789",
        CreditLimit = 50000.00m,
        PaymentTermDays = 30,
        OpeningBalance = 15000.00m,
        CurrentBalance = 18500.00m,
        IsActive = true,
        Notes = "مورد رئيسي للمستلزمات الفندقية والبياضات",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP002",
        SupplierName = "مؤسسة الأهرام للنقل السياحي",
        SupplierNameEn = "Al-Ahram Tourist Transportation",
        Phone = "0233445566",
        Mobile = "01098765432",
        Email = "booking@ahram-transport.com",
        Address = "89 شارع الهرم، الجيزة",
        City = "الجيزة",
        Country = "مصر",
        TaxNumber = "234-567-890",
        CreditLimit = 75000.00m,
        PaymentTermDays = 15,
        OpeningBalance = 25000.00m,
        CurrentBalance = 32000.00m,
        IsActive = true,
        Notes = "شركة نقل سياحي - باصات مكيفة",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP003",
        SupplierName = "فندق الزهراء",
        SupplierNameEn = "Al-Zahraa Hotel",
        Phone = "0653332211",
        Mobile = "01155443322",
        Email = "reservations@zahraa-hotel.com",
        Address = "طريق المدينة - مكة المكرمة",
        City = "مكة المكرمة",
        Country = "السعودية",
        TaxNumber = "345-678-901",
        CreditLimit = 100000.00m,
        PaymentTermDays = 45,
        OpeningBalance = 0.00m,
        CurrentBalance = 12000.00m,
        IsActive = true,
        Notes = "فندق 4 نجوم قريب من الحرم",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP004",
        SupplierName = "شركة البركة للمطاعم والتموين",
        SupplierNameEn = "Al-Baraka Catering Services",
        Phone = "0244556677",
        Mobile = "01122334455",
        Email = "sales@baraka-catering.com",
        Address = "45 شارع فيصل، الهرم",
        City = "الجيزة",
        Country = "مصر",
        TaxNumber = "456-789-012",
        CreditLimit = 30000.00m,
        PaymentTermDays = 7,
        OpeningBalance = 8000.00m,
        CurrentBalance = 9500.00m,
        IsActive = true,
        Notes = "تقديم وجبات للرحلات السياحية",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP005",
        SupplierName = "مكتب الرحمة للسياحة والعمرة",
        SupplierNameEn = "Al-Rahma Tourism Office",
        Phone = "0658887766",
        Mobile = "01199887766",
        Email = "info@rahma-tourism.sa",
        Address = "حي العزيزية - المدينة المنورة",
        City = "المدينة المنورة",
        Country = "السعودية",
        TaxNumber = "567-890-123",
        CreditLimit = 60000.00m,
        PaymentTermDays = 30,
        OpeningBalance = 20000.00m,
        CurrentBalance = 25000.00m,
        IsActive = true,
        Notes = "منسق رحلات العمرة في السعودية",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP006",
        SupplierName = "شركة المروج لخدمات الطيران",
        SupplierNameEn = "Al-Morouj Aviation Services",
        Phone = "0211122334",
        Mobile = "01166554433",
        Email = "flights@morouj-aviation.com",
        Address = "مطار القاهرة الدولي، المبنى 3",
        City = "القاهرة",
        Country = "مصر",
        TaxNumber = "678-901-234",
        CreditLimit = 150000.00m,
        PaymentTermDays = 60,
        OpeningBalance = 45000.00m,
        CurrentBalance = 52000.00m,
        IsActive = true,
        Notes = "حجز تذاكر طيران بالجملة",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP007",
        SupplierName = "محلات الهدى للهدايا والتذكارات",
        SupplierNameEn = "Al-Huda Gifts & Souvenirs",
        Phone = "0244778899",
        Mobile = "01077665544",
        Email = "sales@huda-gifts.com",
        Address = "23 شارع خان الخليلي",
        City = "القاهرة",
        Country = "مصر",
        TaxNumber = "789-012-345",
        CreditLimit = 15000.00m,
        PaymentTermDays = 0,
        OpeningBalance = 3000.00m,
        CurrentBalance = 2800.00m,
        IsActive = true,
        Notes = "توريد هدايا وتذكارات للسياح - دفع فوري",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP008",
        SupplierName = "شركة التقوى للحافلات",
        SupplierNameEn = "Al-Taqwa Bus Company",
        Phone = "0266554433",
        Mobile = "01044332211",
        Email = "info@taqwa-buses.com",
        Address = "67 طريق الإسكندرية الصحراوي",
        City = "الإسكندرية",
        Country = "مصر",
        TaxNumber = "890-123-456",
        CreditLimit = 40000.00m,
        PaymentTermDays = 30,
        OpeningBalance = 12000.00m,
        CurrentBalance = -5000.00m,
        IsActive = false,
        Notes = "مورد سابق - تم إيقاف التعامل لتأخر في السداد",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP009",
        SupplierName = "فندق دار الإيمان",
        SupplierNameEn = "Dar Al-Iman Hotel",
        Phone = "0654445566",
        Mobile = "01188776655",
        Email = "booking@dariman-hotel.sa",
        Address = "شارع الملك فهد - المدينة المنورة",
        City = "المدينة المنورة",
        Country = "السعودية",
        TaxNumber = "901-234-567",
        CreditLimit = 80000.00m,
        PaymentTermDays = 30,
        OpeningBalance = 0.00m,
        CurrentBalance = 16500.00m,
        IsActive = true,
        Notes = "فندق 5 نجوم قريب من الحرم النبوي",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP010",
        SupplierName = "مكتب الخير للتأشيرات",
        SupplierNameEn = "Al-Khair Visa Services",
        Phone = "0233221100",
        Mobile = "01055667788",
        Email = "visas@khair-services.com",
        Address = "12 شارع التحرير، الدقي",
        City = "الجيزة",
        Country = "مصر",
        TaxNumber = "012-345-678",
        CreditLimit = 25000.00m,
        PaymentTermDays = 15,
        OpeningBalance = 7500.00m,
        CurrentBalance = 8900.00m,
        IsActive = true,
        Notes = "استخراج تأشيرات سياحية وعمرة",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP011",
        SupplierName = "مطعم السلطان للمأكولات",
        SupplierNameEn = "Al-Sultan Restaurant",
        Phone = "0255443322",
        Mobile = "01133445566",
        Email = "orders@sultan-restaurant.com",
        Address = "78 شارع البحر، الإسكندرية",
        City = "الإسكندرية",
        Country = "مصر",
        TaxNumber = "123-654-789",
        CreditLimit = 20000.00m,
        PaymentTermDays = 7,
        OpeningBalance = 4500.00m,
        CurrentBalance = 5200.00m,
        IsActive = true,
        Notes = "تقديم وجبات للأفواج السياحية",
        CreatedAt = DateTime.UtcNow
    },
    new Supplier
    {
        SupplierCode = "SUP012",
        SupplierName = "شركة الفجر للتأمين الطبي",
        SupplierNameEn = "Al-Fajr Medical Insurance",
        Phone = "0266778899",
        Mobile = "01099112233",
        Email = "insurance@fajr-insurance.com",
        Address = "45 شارع رمسيس، وسط القاهرة",
        City = "القاهرة",
        Country = "مصر",
        TaxNumber = "234-765-890",
        CreditLimit = 35000.00m,
        PaymentTermDays = 45,
        OpeningBalance = 10000.00m,
        CurrentBalance = 11200.00m,
        IsActive = true,
        Notes = "تأمين طبي للحجاج والمعتمرين",
        CreatedAt = DateTime.UtcNow
    }
};

await context.Suppliers.AddRangeAsync(suppliers);
await context.SaveChangesAsync();

Console.WriteLine($"✅ تم إضافة {suppliers.Count} مورد بنجاح!");
Console.WriteLine();
Console.WriteLine("📊 ملخص الموردين:");
Console.WriteLine("════════════════════════════════════════");

foreach (var supplier in suppliers)
{
    var status = supplier.IsActive ? "✅" : "⛔";
    Console.WriteLine($"{status} {supplier.SupplierCode} - {supplier.SupplierName}");
    Console.WriteLine($"   📍 {supplier.City} | 💰 {supplier.CurrentBalance:N2} ج.م");
    Console.WriteLine();
}

Console.WriteLine("════════════════════════════════════════");
Console.WriteLine("✅ العملية اكتملت بنجاح!");
Console.WriteLine();
Console.WriteLine("يمكنك الآن تشغيل البرنامج وفتح قسم الموردين");
Console.WriteLine();
Console.WriteLine("اضغط أي مفتاح للخروج...");
Console.ReadKey();
