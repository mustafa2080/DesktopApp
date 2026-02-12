using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// أنواع الصلاحيات المتاحة في النظام
/// </summary>
public enum PermissionType
{
    // ============================================
    // قسم الرحلات (Trips)
    // ============================================
    ViewTrips = 1,
    CreateTrip = 2,
    EditTrip = 3,
    DeleteTrip = 4,
    CloseTrip = 5,
    ManageTripBookings = 6,
    
    // ============================================
    // قسم الطيران (Aviation/Flight Bookings)
    // ============================================
    ViewFlightBookings = 10,
    CreateFlightBooking = 11,
    EditFlightBooking = 12,
    DeleteFlightBooking = 13,
    ManageFlightPayments = 14,
    
    // ============================================
    // قسم العمرة (Umrah)
    // ============================================
    ViewUmrahPackages = 20,
    CreateUmrahPackage = 21,
    EditUmrahPackage = 22,
    DeleteUmrahPackage = 23,
    ViewUmrahTrips = 24,
    CreateUmrahTrip = 25,
    EditUmrahTrip = 26,
    DeleteUmrahTrip = 27,
    ManageUmrahPilgrims = 28,
    ManageUmrahPayments = 29,
    
    // ============================================
    // الآلة الحاسبة (Calculator)
    // ============================================
    UseCalculator = 30,
    
    // ============================================
    // قسم العملاء (Customers)
    // ============================================
    ViewCustomers = 40,
    CreateCustomer = 41,
    EditCustomer = 42,
    DeleteCustomer = 43,
    ViewCustomerStatement = 44,
    
    // ============================================
    // قسم الموردين (Suppliers)
    // ============================================
    ViewSuppliers = 50,
    CreateSupplier = 51,
    EditSupplier = 52,
    DeleteSupplier = 53,
    ViewSupplierStatement = 54,
    
    // ============================================
    // قسم الفواتير (Invoices)
    // ============================================
    ViewInvoices = 60,
    CreateSalesInvoice = 61,
    EditSalesInvoice = 62,
    DeleteSalesInvoice = 63,
    CreatePurchaseInvoice = 64,
    EditPurchaseInvoice = 65,
    DeletePurchaseInvoice = 66,
    ApproveInvoice = 67,
    
    // ============================================
    // قسم الحجوزات العامة (Reservations)
    // ============================================
    ViewReservations = 70,
    CreateReservation = 71,
    EditReservation = 72,
    DeleteReservation = 73,
    
    // ============================================
    // قسم الخزنة والبنوك (Cash & Banks)
    // ============================================
    ViewCashBox = 80,
    CreateCashTransaction = 81,
    EditCashTransaction = 82,
    DeleteCashTransaction = 83,
    ViewBankAccounts = 84,
    CreateBankTransaction = 85,
    EditBankTransaction = 86,
    DeleteBankTransaction = 87,
    ManageBankTransfers = 88,
    
    // ============================================
    // قسم القيود اليومية (Journal Entries)
    // ============================================
    ViewJournalEntries = 90,
    CreateJournalEntry = 91,
    EditJournalEntry = 92,
    DeleteJournalEntry = 93,
    EditClosedPeriod = 94,
    
    // ============================================
    // قسم الحسابات (Chart of Accounts)
    // ============================================
    ViewChartOfAccounts = 100,
    CreateAccount = 101,
    EditAccount = 102,
    DeleteAccount = 103,
    
    // ============================================
    // قسم التقارير (Reports)
    // ============================================
    ViewReports = 110,
    ViewFinancialReports = 111,
    ViewTrialBalance = 112,
    ViewIncomeStatement = 113,
    ViewBalanceSheet = 114,
    ViewCashFlowStatement = 115,
    ViewTripReports = 116,
    ViewFlightReports = 117,
    ViewUmrahReports = 118,
    ViewProfitMargins = 119,
    ExportReports = 120,
    PrintReports = 121,
    
    // ============================================
    // قسم الإعدادات (Settings)
    // ============================================
    ViewSettings = 130,
    EditCompanySettings = 131,
    EditInvoiceSettings = 132,
    EditFiscalYearSettings = 133,
    ManageCurrencies = 134,
    ManageServiceTypes = 135,
    
    // ============================================
    // قسم إدارة النظام (System Administration)
    // ============================================
    ManageUsers = 140,
    ManageRoles = 141,
    ManagePermissions = 142,
    ViewAuditLogs = 143,
    ViewSystemLogs = 144,
    BackupDatabase = 145,
    RestoreDatabase = 146,
    ManageSessions = 147
}

/// <summary>
/// الصلاحيات المتاحة في النظام
/// </summary>
public class Permission
{
    [Key]
    public int PermissionId { get; set; }
    
    public PermissionType PermissionType { get; set; }
    
    [MaxLength(100)]
    public string PermissionName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Module { get; set; } = string.Empty; // "Trips", "Aviation", "Umrah", "Accounting", "System"
    
    public bool IsSystemPermission { get; set; } = false; // الصلاحيات الأساسية للنظام
    
    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
