using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using System.Text.Json;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IAuthService? _authService;

    public AppDbContext(DbContextOptions<AppDbContext> options, IAuthService? authService = null) : base(options)
    {
        _authService = authService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        if (optionsBuilder.IsConfigured)
        {
            // إضافة command timeout ل 30 ثانية
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await LogChangesAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task LogChangesAsync()
    {
        var currentUser = _authService?.CurrentUser;
        if (currentUser == null) return;

        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || 
                       e.State == EntityState.Modified || 
                       e.State == EntityState.Deleted)
            .Where(e => e.Entity.GetType() != typeof(AuditLog)) // تجنب تسجيل AuditLog نفسه
            .ToList();

        foreach (var entry in entries)
        {
            var auditLog = new AuditLog
            {
                UserId = currentUser.UserId,
                Username = currentUser.Username,
                UserFullName = currentUser.FullName,
                EntityType = entry.Entity.GetType().Name,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditLog.Action = AuditAction.Create;
                    auditLog.Description = $"إضافة {GetEntityDisplayName(entry.Entity)}";
                    auditLog.EntityId = GetEntityId(entry.Entity);
                    auditLog.EntityName = GetEntityName(entry.Entity);
                    auditLog.NewValues = JsonSerializer.Serialize(GetEntityValues(entry));
                    break;

                case EntityState.Modified:
                    auditLog.Action = AuditAction.Update;
                    auditLog.Description = $"تعديل {GetEntityDisplayName(entry.Entity)}";
                    auditLog.EntityId = GetEntityId(entry.Entity);
                    auditLog.EntityName = GetEntityName(entry.Entity);
                    auditLog.OldValues = JsonSerializer.Serialize(GetOriginalValues(entry));
                    auditLog.NewValues = JsonSerializer.Serialize(GetEntityValues(entry));
                    break;

                case EntityState.Deleted:
                    auditLog.Action = AuditAction.Delete;
                    auditLog.Description = $"حذف {GetEntityDisplayName(entry.Entity)}";
                    auditLog.EntityId = GetEntityId(entry.Entity);
                    auditLog.EntityName = GetEntityName(entry.Entity);
                    auditLog.OldValues = JsonSerializer.Serialize(GetEntityValues(entry));
                    break;
            }

            AuditLogs.Add(auditLog);
        }

        await Task.CompletedTask;
    }

    private int? GetEntityId(object entity)
    {
        var idProperty = entity.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id") && p.PropertyType == typeof(int));
        return idProperty?.GetValue(entity) as int?;
    }

    private string GetEntityName(object entity)
    {
        // محاولة الحصول على اسم مناسب للعرض
        var type = entity.GetType();
        
        if (type.GetProperty("Name")?.GetValue(entity) is string name && !string.IsNullOrEmpty(name))
            return name;
        
        if (type.GetProperty("TripName")?.GetValue(entity) is string tripName && !string.IsNullOrEmpty(tripName))
            return tripName;
        
        if (type.GetProperty("CustomerName")?.GetValue(entity) is string customerName && !string.IsNullOrEmpty(customerName))
            return customerName;
        
        if (type.GetProperty("SupplierName")?.GetValue(entity) is string supplierName && !string.IsNullOrEmpty(supplierName))
            return supplierName;

        if (type.GetProperty("FullName")?.GetValue(entity) is string fullName && !string.IsNullOrEmpty(fullName))
            return fullName;

        if (type.GetProperty("InvoiceNumber")?.GetValue(entity) is string invoiceNumber && !string.IsNullOrEmpty(invoiceNumber))
            return invoiceNumber;

        if (type.GetProperty("BookingNumber")?.GetValue(entity) is string bookingNumber && !string.IsNullOrEmpty(bookingNumber))
            return bookingNumber;

        return GetEntityId(entity)?.ToString() ?? "Unknown";
    }

    private string GetEntityDisplayName(object entity)
    {
        var entityName = entity.GetType().Name;
        return entityName switch
        {
            "Trip" => "رحلة",
            "TripBooking" => "حجز رحلة",
            "Customer" => "عميل",
            "Supplier" => "مورد",
            "SalesInvoice" => "فاتورة مبيعات",
            "PurchaseInvoice" => "فاتورة مشتريات",
            "CashTransaction" => "حركة مالية",
            "Reservation" => "حجز",
            "FlightBooking" => "حجز طيران",
            "UmrahPackage" => "باكدج عمرة",
            "UmrahTrip" => "رحلة عمرة",
            "UmrahPilgrim" => "معتمر",
            "TripProgram" => "برنامج رحلة",
            "TripAccommodation" => "إقامة رحلة",
            "TripTransportation" => "مواصلات رحلة",
            "Account" => "حساب",
            "User" => "مستخدم",
            "BankTransfer" => "تحويل بنكي",
            _ => entityName
        };
    }

    private Dictionary<string, object?> GetEntityValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();
        foreach (var property in entry.CurrentValues.Properties)
        {
            var value = entry.CurrentValues[property];
            // تجنب القيم الكبيرة جداً
            if (value is string str && str.Length > 500)
                values[property.Name] = str.Substring(0, 500) + "...";
            else
                values[property.Name] = value;
        }
        return values;
    }

    private Dictionary<string, object?> GetOriginalValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();
        foreach (var property in entry.OriginalValues.Properties)
        {
            var value = entry.OriginalValues[property];
            // تجنب القيم الكبيرة جداً
            if (value is string str && str.Length > 500)
                values[property.Name] = str.Substring(0, 500) + "...";
            else
                values[property.Name] = value;
        }
        return values;
    }

    // Core Accounting Tables
    public DbSet<Account> Accounts { get; set; }
    public DbSet<CompanySetting> CompanySettings { get; set; }
    public DbSet<InvoiceSetting> InvoiceSettings { get; set; }
    public DbSet<FiscalYearSetting> FiscalYearSettings { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }

    // Cash Management Tables
    public DbSet<CashBox> CashBoxes { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<BankTransfer> BankTransfers { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Customers and Suppliers Tables
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }

    // Invoices Tables
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
    public DbSet<InvoicePayment> InvoicePayments { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    
    // Flight Bookings Tables
    public DbSet<FlightBooking> FlightBookings { get; set; }

    // Journal Tables
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }

    // Trips System Tables
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripProgram> TripPrograms { get; set; }
    public DbSet<TripAccommodation> TripAccommodations { get; set; }
    public DbSet<TripTransportation> TripTransportations { get; set; }
    public DbSet<TripGuide> TripGuides { get; set; }
    public DbSet<TripOptionalTour> TripOptionalTours { get; set; }
    
    // Backup System
    public DbSet<DatabaseBackup> DatabaseBackups { get; set; }
    public DbSet<TripOptionalTourBooking> TripOptionalTourBookings { get; set; }
    public DbSet<TripExpense> TripExpenses { get; set; }
    public DbSet<TripSupplier> TripSuppliers { get; set; }
    public DbSet<TripBooking> TripBookings { get; set; }
    public DbSet<TripBookingPayment> TripBookingPayments { get; set; }

    // Umrah System Tables
    public DbSet<UmrahPackage> UmrahPackages { get; set; }
    public DbSet<UmrahTrip> UmrahTrips { get; set; }
    public DbSet<UmrahPilgrim> UmrahPilgrims { get; set; }
    public DbSet<UmrahPayment> UmrahPayments { get; set; }

    // Security Tables
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    // Audit Trail
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // PostgreSQL DateTime configuration - store as UTC timestamp
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));
            
            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name)
                    .Property(property.Name)
                    .HasColumnType("timestamp with time zone")
                    .HasConversion(property.PropertyType == typeof(DateTime?) ? nullableDateTimeConverter : dateTimeConverter);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // ACCOUNT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.AccountId);
            
            entity.Property(e => e.AccountId).HasColumnName("accountid");
            entity.Property(e => e.AccountCode).HasColumnName("accountcode");
            entity.Property(e => e.AccountName).HasColumnName("accountname");
            entity.Property(e => e.AccountNameEn).HasColumnName("accountnameen");
            entity.Property(e => e.ParentAccountId).HasColumnName("parentaccountid");
            entity.Property(e => e.AccountType).HasColumnName("accounttype");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.IsParent).HasColumnName("isparent");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.OpeningBalance).HasColumnName("openingbalance");
            entity.Property(e => e.CurrentBalance).HasColumnName("currentbalance");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");

            // Self-referencing relationship
            entity.HasOne(e => e.ParentAccount)
                .WithMany(e => e.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ParentAccountId);
        });

        // ════════════════════════════════════════════════════════════════
        // COMPANY SETTINGS ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<CompanySetting>(entity =>
        {
            entity.ToTable("companysettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("settingid");
            entity.Property(e => e.CompanyName).HasColumnName("companyname");
            entity.Property(e => e.CompanyNameEnglish).HasColumnName("companynameen");
            entity.Property(e => e.LogoPath).HasColumnName("logopath");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Website).HasColumnName("website");
            entity.Property(e => e.TaxRegistrationNumber).HasColumnName("taxregistrationnumber");
            entity.Property(e => e.CommercialRegistrationNumber).HasColumnName("commercialregistrationnumber");
            entity.Property(e => e.BankName).HasColumnName("bankname");
            entity.Property(e => e.BankAccountNumber).HasColumnName("bankaccountnumber");
            entity.Property(e => e.BankIBAN).HasColumnName("bankiban");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate");
            entity.Property(e => e.LastModifiedDate).HasColumnName("lastmodifieddate");
            entity.Property(e => e.LastModifiedByUserId).HasColumnName("lastmodifiedbyuserid");

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ════════════════════════════════════════════════════════════════
        // INVOICE SETTING ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<InvoiceSetting>(entity =>
        {
            entity.ToTable("invoicesettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("settingid");
            entity.Property(e => e.EnableTax).HasColumnName("enabletax");
            entity.Property(e => e.DefaultTaxRate).HasColumnName("defaulttaxrate");
            entity.Property(e => e.TaxLabel).HasColumnName("taxlabel");
            entity.Property(e => e.AutoNumbering).HasColumnName("autonumbering");
            entity.Property(e => e.SalesInvoicePrefix).HasColumnName("salesinvoiceprefix");
            entity.Property(e => e.PurchaseInvoicePrefix).HasColumnName("purchaseinvoiceprefix");
            entity.Property(e => e.NextSalesInvoiceNumber).HasColumnName("nextsalesinvoicenumber");
            entity.Property(e => e.NextPurchaseInvoiceNumber).HasColumnName("nextpurchaseinvoicenumber");
            entity.Property(e => e.InvoiceNumberLength).HasColumnName("invoicenumberlength");
            entity.Property(e => e.InvoiceFooterText).HasColumnName("invoicefootertext");
            entity.Property(e => e.PaymentTerms).HasColumnName("paymentterms");
            entity.Property(e => e.BankDetails).HasColumnName("bankdetails");
            entity.Property(e => e.NotesTemplate).HasColumnName("notestemplate");
            entity.Property(e => e.ShowCompanyLogo).HasColumnName("showcompanylogo");
            entity.Property(e => e.ShowTaxNumber).HasColumnName("showtaxnumber");
            entity.Property(e => e.ShowBankDetails).HasColumnName("showbankdetails");
            entity.Property(e => e.ShowPaymentTerms).HasColumnName("showpaymentterms");
            entity.Property(e => e.PaperSize).HasColumnName("papersize");
            entity.Property(e => e.PrintInColor).HasColumnName("printincolor");
            entity.Property(e => e.PrintDuplicateCopy).HasColumnName("printduplicatecopy");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate");
            entity.Property(e => e.LastModifiedDate).HasColumnName("lastmodifieddate");
            entity.Property(e => e.LastModifiedByUserId).HasColumnName("lastmodifiedbyuserid");

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ════════════════════════════════════════════════════════════════
        // FISCAL YEAR SETTING ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<FiscalYearSetting>(entity =>
        {
            entity.ToTable("fiscalyearsettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("fiscalyearid");
            entity.Property(e => e.FiscalYearStart).HasColumnName("fiscalyearstart");
            entity.Property(e => e.FiscalYearEnd).HasColumnName("fiscalyearend");
            entity.Property(e => e.IsCurrentYear).HasColumnName("iscurrentyear");
            entity.Property(e => e.IsClosed).HasColumnName("isclosed");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate");
            entity.Property(e => e.ModifiedBy).HasColumnName("modifiedby");
        });

        // ════════════════════════════════════════════════════════════════
        // CURRENCY ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");
            entity.HasKey(e => e.CurrencyId);
            
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.Code).HasColumnName("currencycode").HasMaxLength(3);
            entity.Property(e => e.Name).HasColumnName("currencyname").HasMaxLength(50);
            entity.Property(e => e.Symbol).HasColumnName("symbol").HasMaxLength(10);
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.IsBaseCurrency).HasColumnName("isbasecurrency");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");
        });

        // ════════════════════════════════════════════════════════════════
        // CURRENCY EXCHANGE RATE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<CurrencyExchangeRate>(entity =>
        {
            entity.ToTable("currencyexchangerates");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("exchangerateid");
            entity.Property(e => e.FromCurrency).HasColumnName("fromcurrency");
            entity.Property(e => e.ToCurrency).HasColumnName("tocurrency");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.EffectiveDate).HasColumnName("effectivedate");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate");
            entity.Property(e => e.ModifiedBy).HasColumnName("modifiedby");
        });

        // ════════════════════════════════════════════════════════════════
        // CASHBOX ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<CashBox>(entity =>
        {
            entity.ToTable("cashboxes");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("cashboxid");
            entity.Property(e => e.CashBoxCode).HasColumnName("cashboxcode");
            entity.Property(e => e.Name).HasColumnName("cashboxname");
            entity.Property(e => e.Type).HasColumnName("cashboxtype");
            entity.Property(e => e.AccountNumber).HasColumnName("accountnumber");
            entity.Property(e => e.Iban).HasColumnName("iban");
            entity.Property(e => e.AccountId).HasColumnName("accountid");
            entity.Property(e => e.BankName).HasColumnName("bankname");
            entity.Property(e => e.OpeningBalance).HasColumnName("openingbalance");
            entity.Property(e => e.CurrentBalance).HasColumnName("currentbalance");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.IsDeleted).HasColumnName("isdeleted");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");

            entity.Ignore(e => e.Transactions);
        });

        // ════════════════════════════════════════════════════════════════
        // CASH TRANSACTION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<CashTransaction>(entity =>
        {
            entity.ToTable("cashtransactions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("transactionid");
            entity.Property(e => e.VoucherNumber).HasColumnName("vouchernumber");
            entity.Property(e => e.Type).HasColumnName("transactiontype").HasConversion<int>();
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.Amount).HasColumnName("amount");
            
            // ✅ Currency fields - PascalCase كما هو في DB
            entity.Property(e => e.TransactionCurrency).HasColumnName("TransactionCurrency");
            entity.Property(e => e.ExchangeRateUsed).HasColumnName("ExchangeRateUsed");
            entity.Property(e => e.OriginalAmount).HasColumnName("OriginalAmount");
            
            entity.Property(e => e.TransactionDate).HasColumnName("transactiondate");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PartyName).HasColumnName("partyname");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod").HasConversion<int>();
            entity.Property(e => e.InstaPayCommission).HasColumnName("instapaycommission");
            entity.Property(e => e.ReferenceNumber).HasColumnName("referencenumber");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.BalanceBefore).HasColumnName("balancebefore");
            entity.Property(e => e.BalanceAfter).HasColumnName("balanceafter");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");
            
            // ✅ Update fields
            entity.Property(e => e.UpdatedBy).HasColumnName("UpdatedBy");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");
            
            entity.Property(e => e.IsDeleted).HasColumnName("isdeleted");
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");

            entity.HasOne(e => e.CashBox)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reservation)
                .WithMany()
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // ✅ Navigation properties for tracking
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Updater)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CashBoxId);
            entity.HasIndex(e => e.ReservationId);
        });

        // ════════════════════════════════════════════════════════════════
        // BANK ACCOUNT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("bankaccounts");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankName).HasColumnName("bankname").HasMaxLength(200).IsRequired();
            entity.Property(e => e.AccountNumber).HasColumnName("accountnumber").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AccountType).HasColumnName("accounttype").HasMaxLength(50);
            entity.Property(e => e.Balance).HasColumnName("balance").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(e => e.Branch).HasColumnName("branch").HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.ModifiedBy).HasColumnName("modifiedby");
            
            entity.HasIndex(e => e.AccountNumber).IsUnique();
        });

        // ════════════════════════════════════════════════════════════════
        // BANK TRANSFER ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<BankTransfer>(entity =>
        {
            entity.ToTable("banktransfers");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceBankAccountId).HasColumnName("sourcebankaccountid");
            entity.Property(e => e.SourceCashBoxId).HasColumnName("sourcecashboxid");
            entity.Property(e => e.DestinationBankAccountId).HasColumnName("destinationbankaccountid");
            entity.Property(e => e.DestinationCashBoxId).HasColumnName("destinationcashboxid");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.TransferType).HasColumnName("transfertype");
            entity.Property(e => e.TransferDate).HasColumnName("transferdate");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.ReferenceNumber).HasColumnName("referencenumber");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate");

            entity.HasOne(e => e.SourceBankAccount)
                .WithMany()
                .HasForeignKey(e => e.SourceBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationBankAccount)
                .WithMany()
                .HasForeignKey(e => e.DestinationBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SourceCashBox)
                .WithMany()
                .HasForeignKey(e => e.SourceCashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationCashBox)
                .WithMany()
                .HasForeignKey(e => e.DestinationCashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SourceBankAccountId);
            entity.HasIndex(e => e.DestinationBankAccountId);
            entity.HasIndex(e => e.SourceCashBoxId);
            entity.HasIndex(e => e.DestinationCashBoxId);
        });

        // ════════════════════════════════════════════════════════════════
        // PAYMENT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId);
            
            entity.Property(e => e.PaymentId).HasColumnName("paymentid");
            entity.Property(e => e.PaymentNumber).HasColumnName("paymentnumber");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentdate");
            entity.Property(e => e.PaymentType).HasColumnName("paymenttype");
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod");
            entity.Property(e => e.ReferenceType).HasColumnName("referencetype");
            entity.Property(e => e.ReferenceId).HasColumnName("referenceid");
            entity.Property(e => e.CheckNumber).HasColumnName("checknumber");
            entity.Property(e => e.CheckDate).HasColumnName("checkdate");
            entity.Property(e => e.BankName).HasColumnName("bankname");
            entity.Property(e => e.JournalEntryId).HasColumnName("journalentryid");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CashBoxId);
            entity.HasIndex(e => e.CurrencyId);
        });

        // ════════════════════════════════════════════════════════════════
        // CUSTOMER ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.CustomerId);
            
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.CustomerCode).HasColumnName("customercode");
            entity.Property(e => e.CustomerName).HasColumnName("customername");
            entity.Property(e => e.CustomerNameEn).HasColumnName("customernameen");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.TaxNumber).HasColumnName("taxnumber");
            entity.Property(e => e.CreditLimit).HasColumnName("creditlimit");
            entity.Property(e => e.PaymentTermDays).HasColumnName("paymenttermdays");
            entity.Property(e => e.AccountId).HasColumnName("accountid");
            entity.Property(e => e.OpeningBalance).HasColumnName("openingbalance");
            entity.Property(e => e.CurrentBalance).HasColumnName("currentbalance");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.Reservations);
            entity.Ignore(e => e.SalesInvoices);
            entity.Ignore(e => e.TripBookings);

            entity.HasIndex(e => e.AccountId);
        });

        // ════════════════════════════════════════════════════════════════
        // SUPPLIER ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.SupplierId);
            
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.SupplierCode).HasColumnName("suppliercode");
            entity.Property(e => e.SupplierName).HasColumnName("suppliername");
            entity.Property(e => e.SupplierNameEn).HasColumnName("suppliernameen");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.TaxNumber).HasColumnName("taxnumber");
            entity.Property(e => e.CreditLimit).HasColumnName("creditlimit");
            entity.Property(e => e.PaymentTermDays).HasColumnName("paymenttermdays");
            entity.Property(e => e.AccountId).HasColumnName("accountid");
            entity.Property(e => e.OpeningBalance).HasColumnName("openingbalance");
            entity.Property(e => e.CurrentBalance).HasColumnName("currentbalance");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.PurchaseInvoices);
            entity.Ignore(e => e.Reservations);
            entity.Ignore(e => e.TripAccommodations);
            entity.Ignore(e => e.TripTransportations);
            entity.Ignore(e => e.TripExpenses);
            entity.Ignore(e => e.TripSuppliers);

            entity.HasIndex(e => e.AccountId);
        });

        // ════════════════════════════════════════════════════════════════
        // SERVICE TYPE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.ToTable("servicetypes");
            entity.HasKey(e => e.ServiceTypeId);
            
            entity.Property(e => e.ServiceTypeId).HasColumnName("servicetypeid");
            entity.Property(e => e.ServiceTypeName).HasColumnName("servicetypename");
            entity.Property(e => e.ServiceTypeNameEn).HasColumnName("servicetypenameen");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
        });

        // ════════════════════════════════════════════════════════════════
        // RESERVATION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(e => e.ReservationId);
            
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");
            entity.Property(e => e.ReservationNumber).HasColumnName("reservationnumber");
            entity.Property(e => e.ReservationDate).HasColumnName("reservationdate");
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.ServiceTypeId).HasColumnName("servicetypeid");
            entity.Property(e => e.ServiceDescription).HasColumnName("servicedescription");
            entity.Property(e => e.TravelDate).HasColumnName("traveldate");
            entity.Property(e => e.ReturnDate).HasColumnName("returndate");
            entity.Property(e => e.NumberOfPeople).HasColumnName("numberofpeople");
            entity.Property(e => e.SellingPrice).HasColumnName("sellingprice");
            entity.Property(e => e.CostPrice).HasColumnName("costprice");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.SupplierCost).HasColumnName("suppliercost");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("UpdatedAt");
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.CashTransactionId).HasColumnName("cashtransactionid");
            entity.Property(e => e.TripId).HasColumnName("tripid");

            entity.Ignore(e => e.ProfitAmount);
            entity.Ignore(e => e.ProfitPercentage);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ServiceType)
                .WithMany(s => s.Reservations)
                .HasForeignKey(e => e.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Reservations)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashTransaction)
                .WithMany()
                .HasForeignKey(e => e.CashTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Reservations)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ServiceTypeId);
            entity.HasIndex(e => e.CurrencyId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.CashBoxId);
            entity.HasIndex(e => e.CashTransactionId);
            entity.HasIndex(e => e.TripId);
        });

        // ════════════════════════════════════════════════════════════════
        // SALES INVOICE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.ToTable("salesinvoices");
            entity.HasKey(e => e.SalesInvoiceId);
            
            entity.Property(e => e.SalesInvoiceId).HasColumnName("salesinvoiceid");
            entity.Property(e => e.InvoiceNumber).HasColumnName("invoicenumber");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoicedate");
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");
            entity.Property(e => e.SubTotal).HasColumnName("subtotal");
            entity.Property(e => e.TaxRate).HasColumnName("taxrate");
            entity.Property(e => e.TaxAmount).HasColumnName("taxamount");
            entity.Property(e => e.TotalAmount).HasColumnName("totalamount");
            entity.Property(e => e.PaidAmount).HasColumnName("paidamount");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.JournalEntryId).HasColumnName("journalentryid");
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.Ignore(e => e.RemainingAmount);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.SalesInvoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reservation)
                .WithMany()
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.JournalEntry)
                .WithMany()
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.Items);
            entity.Ignore(e => e.Payments);
            entity.Ignore(e => e.TripBookings);

            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ReservationId);
            entity.HasIndex(e => e.CurrencyId);
            entity.HasIndex(e => e.CashBoxId);
        });

        // ════════════════════════════════════════════════════════════════
        // SALES INVOICE ITEM ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<SalesInvoiceItem>(entity =>
        {
            entity.ToTable("salesinvoiceitems");
            entity.HasKey(e => e.ItemId);
            
            entity.Property(e => e.ItemId).HasColumnName("itemid");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("salesinvoiceid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");

            entity.Ignore(e => e.TotalPrice);

            entity.HasOne(e => e.SalesInvoice)
                .WithMany(si => si.Items)
                .HasForeignKey(e => e.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SalesInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // PURCHASE INVOICE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.ToTable("purchaseinvoices");
            entity.HasKey(e => e.PurchaseInvoiceId);
            
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");
            entity.Property(e => e.InvoiceNumber).HasColumnName("invoicenumber");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoicedate");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");
            entity.Property(e => e.SubTotal).HasColumnName("subtotal");
            entity.Property(e => e.TaxRate).HasColumnName("taxrate");
            entity.Property(e => e.TaxAmount).HasColumnName("taxamount");
            entity.Property(e => e.TotalAmount).HasColumnName("totalamount");
            entity.Property(e => e.PaidAmount).HasColumnName("paidamount");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.JournalEntryId).HasColumnName("journalentryid");
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.Ignore(e => e.RemainingAmount);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PurchaseInvoices)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reservation)
                .WithMany()
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.JournalEntry)
                .WithMany()
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.Items);
            entity.Ignore(e => e.Payments);
            entity.Ignore(e => e.TripExpenses);
            entity.Ignore(e => e.TripAccommodations);
            entity.Ignore(e => e.TripTransportations);
            entity.Ignore(e => e.TripSuppliers);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.ReservationId);
            entity.HasIndex(e => e.CurrencyId);
            entity.HasIndex(e => e.CashBoxId);
        });

        // ════════════════════════════════════════════════════════════════
        // PURCHASE INVOICE ITEM ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("purchaseinvoiceitems");
            entity.HasKey(e => e.ItemId);
            
            entity.Property(e => e.ItemId).HasColumnName("itemid");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");

            entity.Ignore(e => e.TotalPrice);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.Items)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PurchaseInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // INVOICE PAYMENT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.ToTable("invoicepayments");
            entity.HasKey(e => e.PaymentId);
            
            entity.Property(e => e.PaymentId).HasColumnName("paymentid");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("salesinvoiceid");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.CashTransactionId).HasColumnName("cashtransactionid");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentdate");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod");
            entity.Property(e => e.ReferenceNumber).HasColumnName("referencenumber");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.HasOne(e => e.SalesInvoice)
                .WithMany(si => si.Payments)
                .HasForeignKey(e => e.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.Payments)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashTransaction)
                .WithMany()
                .HasForeignKey(e => e.CashTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SalesInvoiceId);
            entity.HasIndex(e => e.PurchaseInvoiceId);
            entity.HasIndex(e => e.CashBoxId);
            entity.HasIndex(e => e.CashTransactionId);
        });

        // ════════════════════════════════════════════════════════════════
        // FLIGHT BOOKING ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<FlightBooking>(entity =>
        {
            entity.ToTable("flightbookings");
            entity.HasKey(e => e.FlightBookingId);
            
            entity.Property(e => e.FlightBookingId).HasColumnName("flightbookingid");
            entity.Property(e => e.BookingNumber).HasColumnName("bookingnumber");
            entity.Property(e => e.IssuanceDate).HasColumnName("issuancedate");
            entity.Property(e => e.TravelDate).HasColumnName("traveldate");
            entity.Property(e => e.ClientName).HasColumnName("clientname");
            entity.Property(e => e.ClientRoute).HasColumnName("clientroute");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.System).HasColumnName("system");
            entity.Property(e => e.TicketStatus).HasColumnName("ticketstatus");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod");
            entity.Property(e => e.SellingPrice).HasColumnName("sellingprice");
            entity.Property(e => e.NetPrice).HasColumnName("netprice");
            entity.Property(e => e.TicketCount).HasColumnName("ticketcount");
            entity.Property(e => e.TicketNumbers).HasColumnName("ticketnumbers");
            entity.Property(e => e.MobileNumber).HasColumnName("mobilenumber");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedByUserId).HasColumnName("createdbyuserid");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.Ignore(e => e.Profit);
            
            // Foreign Key Relationship
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.CreatedByUserId);
        });

        // ════════════════════════════════════════════════════════════════
        // JOURNAL ENTRY ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journalentries");
            entity.HasKey(e => e.JournalEntryId);
            
            entity.Property(e => e.JournalEntryId).HasColumnName("journalentryid");
            entity.Property(e => e.EntryNumber).HasColumnName("entrynumber");
            entity.Property(e => e.EntryDate).HasColumnName("entrydate");
            entity.Property(e => e.EntryType).HasColumnName("entrytype");
            entity.Property(e => e.ReferenceType).HasColumnName("referencetype");
            entity.Property(e => e.ReferenceId).HasColumnName("referenceid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.TotalDebit).HasColumnName("totaldebit");
            entity.Property(e => e.TotalCredit).HasColumnName("totalcredit");
            entity.Property(e => e.IsPosted).HasColumnName("isposted");
            entity.Property(e => e.PostedAt).HasColumnName("postedat");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
        });

        // ════════════════════════════════════════════════════════════════
        // JOURNAL ENTRY LINE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("journalentrylines");
            entity.HasKey(e => e.LineId);
            
            entity.Property(e => e.LineId).HasColumnName("lineid");
            entity.Property(e => e.JournalEntryId).HasColumnName("journalentryid");
            entity.Property(e => e.AccountId).HasColumnName("accountid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DebitAmount).HasColumnName("debitamount");
            entity.Property(e => e.CreditAmount).HasColumnName("creditamount");
            entity.Property(e => e.LineOrder).HasColumnName("lineorder");

            entity.HasOne(e => e.JournalEntry)
                .WithMany(je => je.Lines)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.JournalEntryId);
            entity.HasIndex(e => e.AccountId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("trips");
            entity.HasKey(e => e.TripId);
            
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.TripNumber).HasColumnName("tripnumber").HasMaxLength(50);
            entity.Property(e => e.TripCode).HasColumnName("tripcode").HasMaxLength(50);
            entity.Property(e => e.TripName).HasColumnName("tripname").HasMaxLength(200);
            entity.Property(e => e.Destination).HasColumnName("destination").HasMaxLength(200);
            entity.Property(e => e.TripType).HasColumnName("triptype").HasConversion<int>();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartDate).HasColumnName("startdate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.EndDate).HasColumnName("enddate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.TotalCapacity).HasColumnName("totalcapacity");
            entity.Property(e => e.AdultCount).HasColumnName("adultcount");
            entity.Property(e => e.ChildCount).HasColumnName("childcount");
            entity.Property(e => e.GuideCost).HasColumnName("guidecost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.DriverTip).HasColumnName("drivertip").HasColumnType("decimal(18,2)");
            entity.Property(e => e.BookedSeats).HasColumnName("bookedseats");
            entity.Property(e => e.SellingPricePerPerson).HasColumnName("sellingpriceperperson");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyid");
            entity.Property(e => e.ExchangeRate).HasColumnName("exchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.IsPublished).HasColumnName("ispublished");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedby");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.Ignore(e => e.TotalDays);
            entity.Ignore(e => e.AvailableSeats);
            entity.Ignore(e => e.OccupancyRate);
            entity.Ignore(e => e.SellingPricePerPersonInEGP);
            entity.Ignore(e => e.ExpectedRevenue);
            entity.Ignore(e => e.TotalRevenue);
            entity.Ignore(e => e.NetProfit);
            entity.Ignore(e => e.ActualProfit);
            entity.Ignore(e => e.ProfitMargin);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Updater)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CurrencyId);
            entity.HasIndex(e => e.CreatedBy);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP PROGRAM ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripProgram>(entity =>
        {
            entity.ToTable("tripprograms");
            entity.HasKey(e => e.TripProgramId);
            
            entity.Property(e => e.TripProgramId).HasColumnName("tripprogramid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.DayNumber).HasColumnName("daynumber");
            entity.Property(e => e.DayTitle).HasColumnName("daytitle").HasMaxLength(200);
            entity.Property(e => e.DayDate).HasColumnName("daydate");
            entity.Property(e => e.Activities).HasColumnName("activities");
            entity.Property(e => e.Visits).HasColumnName("visits");
            entity.Property(e => e.MealsIncluded).HasColumnName("mealsincluded").HasMaxLength(100);
            entity.Property(e => e.VisitsCost).HasColumnName("visitscost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.GuideCost).HasColumnName("guidecost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.ParticipantsCount).HasColumnName("participantscount");
            entity.Property(e => e.DriverTip).HasColumnName("drivertip").HasColumnType("decimal(18,2)");
            entity.Property(e => e.BookingType).HasColumnName("bookingtype").HasMaxLength(10);
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Programs)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TripId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP ACCOMMODATION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripAccommodation>(entity =>
        {
            entity.ToTable("tripaccommodations");
            entity.HasKey(e => e.TripAccommodationId);
            
            entity.Property(e => e.TripAccommodationId).HasColumnName("tripaccommodationid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.Type).HasColumnName("accommodationtype").HasConversion<int>();
            entity.Property(e => e.HotelName).HasColumnName("hotelname").HasMaxLength(200);
            entity.Property(e => e.Rating).HasColumnName("rating").HasConversion<int?>();
            entity.Property(e => e.CruiseLevel).HasColumnName("CruiseLevel").HasConversion<int?>();
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(200);
            entity.Property(e => e.NumberOfRooms).HasColumnName("numberofrooms");
            entity.Property(e => e.RoomType).HasColumnName("roomtype").HasConversion<int>();
            entity.Property(e => e.NumberOfNights).HasColumnName("numberofnights");
            entity.Property(e => e.CheckInDate).HasColumnName("checkindate");
            entity.Property(e => e.CheckOutDate).HasColumnName("checkoutdate");
            entity.Property(e => e.CostPerRoomPerNight).HasColumnName("costperroompernight");
            entity.Property(e => e.GuideCost).HasColumnName("guidecost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.DriverTip).HasColumnName("drivertip").HasColumnType("decimal(18,2)");
            entity.Property(e => e.ParticipantsCount).HasColumnName("participantscount");
            entity.Property(e => e.MealPlan).HasColumnName("mealplan").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");

            entity.Ignore(e => e.TotalCost);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Accommodations)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.TripAccommodations)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.TripAccommodations)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.PurchaseInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP TRANSPORTATION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripTransportation>(entity =>
        {
            entity.ToTable("triptransportations");
            entity.HasKey(e => e.TripTransportationId);
            
            entity.Property(e => e.TripTransportationId).HasColumnName("triptransportationid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.Type).HasColumnName("transportationtype").HasConversion<int>();
            entity.Property(e => e.VehicleModel).HasColumnName("vehiclemodel").HasMaxLength(100);
            entity.Property(e => e.NumberOfVehicles).HasColumnName("numberofvehicles");
            entity.Property(e => e.SeatsPerVehicle).HasColumnName("seatspervehicle");
            entity.Property(e => e.CostPerVehicle).HasColumnName("costpervehicle");
            entity.Property(e => e.SupplierName).HasColumnName("suppliername").HasMaxLength(200);
            entity.Property(e => e.DriverName).HasColumnName("drivername").HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasColumnName("driverphone").HasMaxLength(20);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");
            
            // الأعمدة الجديدة
            entity.Property(e => e.TransportDate).HasColumnName("transportdate");
            entity.Property(e => e.Route).HasColumnName("route").HasMaxLength(300);
            entity.Property(e => e.ParticipantsCount).HasColumnName("participantscount");
            entity.Property(e => e.TourLeaderTip).HasColumnName("tourleadertip").HasColumnType("decimal(18,2)");
            entity.Property(e => e.DriverTip).HasColumnName("drivertip").HasColumnType("decimal(18,2)");

            entity.Ignore(e => e.TotalSeats);
            entity.Ignore(e => e.TotalCost);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Transportation)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.TripTransportations)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.TripTransportations)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.PurchaseInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP GUIDE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripGuide>(entity =>
        {
            entity.ToTable("tripguides");
            entity.HasKey(e => e.TripGuideId);
            
            entity.Property(e => e.TripGuideId).HasColumnName("tripguideid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.GuideName).HasColumnName("guidename").HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Languages).HasColumnName("languages").HasMaxLength(100);
            entity.Property(e => e.BaseFee).HasColumnName("basefee");
            entity.Property(e => e.CommissionPercentage).HasColumnName("commissionpercentage");
            entity.Property(e => e.CommissionAmount).HasColumnName("commissionamount");
            entity.Property(e => e.DriverTip).HasColumnName("drivertip").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Ignore(e => e.TotalCost);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Guides)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TripId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP OPTIONAL TOUR ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripOptionalTour>(entity =>
        {
            entity.ToTable("tripoptionaltours");
            entity.HasKey(e => e.TripOptionalTourId);
            
            entity.Property(e => e.TripOptionalTourId).HasColumnName("tripoptionaltourid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.TourName).HasColumnName("tourname").HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.SellingPrice).HasColumnName("sellingprice");
            entity.Property(e => e.PurchasePrice).HasColumnName("purchaseprice");
            entity.Property(e => e.GuideCommission).HasColumnName("guidecommission");
            entity.Property(e => e.SalesRepCommission).HasColumnName("salesrepcommission");
            entity.Property(e => e.ParticipantsCount).HasColumnName("participantscount");

            entity.Ignore(e => e.Profit);
            entity.Ignore(e => e.TotalRevenue);
            entity.Ignore(e => e.TotalPurchaseCost);
            entity.Ignore(e => e.TotalCost);
            entity.Ignore(e => e.NetProfit);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.OptionalTours)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TripId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP OPTIONAL TOUR BOOKING ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripOptionalTourBooking>(entity =>
        {
            entity.ToTable("tripoptionaltourbookings");
            entity.HasKey(e => e.TripOptionalTourBookingId);
            
            entity.Property(e => e.TripOptionalTourBookingId).HasColumnName("tripoptionaltourbookingid");
            entity.Property(e => e.TripBookingId).HasColumnName("tripbookingid");
            entity.Property(e => e.TripOptionalTourId).HasColumnName("tripoptionaltourid");
            entity.Property(e => e.NumberOfParticipants).HasColumnName("numberofparticipants");
            entity.Property(e => e.PricePerPerson).HasColumnName("priceperperson");
            entity.Property(e => e.BookingDate).HasColumnName("bookingdate");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Ignore(e => e.TotalAmount);

            entity.HasOne(e => e.TripBooking)
                .WithMany(tb => tb.OptionalTourBookings)
                .HasForeignKey(e => e.TripBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OptionalTour)
                .WithMany(ot => ot.Bookings)
                .HasForeignKey(e => e.TripOptionalTourId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripBookingId);
            entity.HasIndex(e => e.TripOptionalTourId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP EXPENSE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripExpense>(entity =>
        {
            entity.ToTable("tripexpenses");
            entity.HasKey(e => e.TripExpenseId);
            
            entity.Property(e => e.TripExpenseId).HasColumnName("tripexpenseid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.ExpenseType).HasColumnName("expensetype").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.ExpenseDate).HasColumnName("expensedate");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod").HasMaxLength(50);
            entity.Property(e => e.ReceiptNumber).HasColumnName("receiptnumber").HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.TripExpenses)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.TripExpenses)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.PurchaseInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP SUPPLIER ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripSupplier>(entity =>
        {
            entity.ToTable("tripsuppliers");
            entity.HasKey(e => e.TripSupplierId);
            
            entity.Property(e => e.TripSupplierId).HasColumnName("tripsupplierid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.SupplierId).HasColumnName("supplierid");
            entity.Property(e => e.SupplierRole).HasColumnName("supplierrole").HasMaxLength(100);
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("purchaseinvoiceid");
            entity.Property(e => e.PaymentStatus).HasColumnName("paymentstatus").HasMaxLength(50);
            entity.Property(e => e.PaidAmount).HasColumnName("paidamount");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdat");

            entity.Ignore(e => e.RemainingAmount);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.TripSuppliers)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.TripSuppliers)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.TripSuppliers)
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.PurchaseInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP BOOKING ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripBooking>(entity =>
        {
            entity.ToTable("tripbookings");
            entity.HasKey(e => e.TripBookingId);
            
            entity.Property(e => e.TripBookingId).HasColumnName("tripbookingid");
            entity.Property(e => e.TripId).HasColumnName("tripid");
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.BookingNumber).HasColumnName("bookingnumber").HasMaxLength(50);
            entity.Property(e => e.BookingDate).HasColumnName("bookingdate");
            entity.Property(e => e.NumberOfPersons).HasColumnName("numberofpersons");
            entity.Property(e => e.PricePerPerson).HasColumnName("priceperperson");
            entity.Property(e => e.PaidAmount).HasColumnName("paidamount");
            entity.Property(e => e.PaymentStatus).HasColumnName("paymentstatus").HasConversion<int>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.SpecialRequests).HasColumnName("specialrequests");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("salesinvoiceid");

            entity.Ignore(e => e.TotalAmount);
            entity.Ignore(e => e.RemainingAmount);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.TripBookings)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalesInvoice)
                .WithMany(si => si.TripBookings)
                .HasForeignKey(e => e.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.SalesInvoiceId);
        });

        // ════════════════════════════════════════════════════════════════
        // TRIP BOOKING PAYMENT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<TripBookingPayment>(entity =>
        {
            entity.ToTable("tripbookingpayments");
            entity.HasKey(e => e.TripBookingPaymentId);
            
            entity.Property(e => e.TripBookingPaymentId).HasColumnName("tripbookingpaymentid");
            entity.Property(e => e.TripBookingId).HasColumnName("tripbookingid");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentdate");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod").HasConversion<int>();
            entity.Property(e => e.ReferenceNumber).HasColumnName("referencenumber").HasMaxLength(100);
            entity.Property(e => e.CashBoxId).HasColumnName("cashboxid");
            entity.Property(e => e.CashTransactionId).HasColumnName("cashtransactionid");
            entity.Property(e => e.InstaPayCommission).HasColumnName("instapaycommission");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.Ignore(e => e.NetAmount);

            entity.HasOne(e => e.Booking)
                .WithMany(tb => tb.Payments)
                .HasForeignKey(e => e.TripBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CashBox)
                .WithMany()
                .HasForeignKey(e => e.CashBoxId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashTransaction)
                .WithMany()
                .HasForeignKey(e => e.CashTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TripBookingId);
            entity.HasIndex(e => e.CashBoxId);
            entity.HasIndex(e => e.CashTransactionId);
        });

        // ════════════════════════════════════════════════════════════════
        // UMRAH PACKAGE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<UmrahPackage>(entity =>
        {
            entity.ToTable("umrahpackages");
            entity.HasKey(e => e.UmrahPackageId);
            
            entity.Property(e => e.UmrahPackageId).HasColumnName("umrahpackageid");
            entity.Property(e => e.PackageNumber).HasColumnName("packagenumber").HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TripName).HasColumnName("tripname").HasMaxLength(200);
            entity.Property(e => e.NumberOfPersons).HasColumnName("numberofpersons");
            entity.Property(e => e.RoomType).HasColumnName("roomtype").HasConversion<int>();
            entity.Property(e => e.MakkahHotel).HasColumnName("makkahhotel").HasMaxLength(200);
            entity.Property(e => e.MakkahNights).HasColumnName("makkahnights");
            entity.Property(e => e.MadinahHotel).HasColumnName("madinahhotel").HasMaxLength(200);
            entity.Property(e => e.MadinahNights).HasColumnName("madinahnights");
            entity.Property(e => e.TransportMethod).HasColumnName("transportmethod").HasMaxLength(100);
            entity.Property(e => e.SellingPrice).HasColumnName("sellingprice");
            entity.Property(e => e.VisaPriceSAR).HasColumnName("visapricesar");
            entity.Property(e => e.SARExchangeRate).HasColumnName("sarexchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.AccommodationTotal).HasColumnName("accommodationtotal");
            entity.Property(e => e.BarcodePrice).HasColumnName("barcodeprice");
            entity.Property(e => e.FlightPrice).HasColumnName("flightprice");
            entity.Property(e => e.FastTrainPriceSAR).HasColumnName("fasttrainpricesar");
            entity.Property(e => e.BrokerName).HasColumnName("brokername").HasMaxLength(200);
            entity.Property(e => e.SupervisorName).HasColumnName("supervisorname").HasMaxLength(200);
            entity.Property(e => e.Commission).HasColumnName("commission");
            entity.Property(e => e.SupervisorExpenses).HasColumnName("supervisorexpenses");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedby");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.Ignore(e => e.VisaPriceEGP);
            entity.Ignore(e => e.FastTrainPriceEGP);
            entity.Ignore(e => e.TotalCosts);
            entity.Ignore(e => e.TotalRevenue);
            entity.Ignore(e => e.NetProfit);
            entity.Ignore(e => e.ProfitMargin);
            entity.Ignore(e => e.NetProfitPerPerson);
            entity.Ignore(e => e.TotalNights);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Updater)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CreatedBy);
        });

        // ════════════════════════════════════════════════════════════════
        // UMRAH TRIP ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<UmrahTrip>(entity =>
        {
            entity.ToTable("umrahtrips");
            entity.HasKey(e => e.UmrahTripId);
            
            entity.Property(e => e.UmrahTripId).HasColumnName("umrahtripid");
            entity.Property(e => e.TripNumber).HasColumnName("tripnumber").HasMaxLength(50);
            entity.Property(e => e.TripName).HasColumnName("tripname").HasMaxLength(200);
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.TotalPilgrims).HasColumnName("totalpilgrims");
            entity.Property(e => e.RoomType).HasColumnName("roomtype").HasConversion<int>();
            entity.Property(e => e.MakkahHotel).HasColumnName("makkahhotel").HasMaxLength(200);
            entity.Property(e => e.MakkahNights).HasColumnName("makkahnights");
            entity.Property(e => e.MadinahHotel).HasColumnName("madinahhotel").HasMaxLength(200);
            entity.Property(e => e.MadinahNights).HasColumnName("madinahnights");
            entity.Property(e => e.TransportMethod).HasColumnName("transportmethod").HasMaxLength(100);
            entity.Property(e => e.PricePerPerson).HasColumnName("priceperperson");
            entity.Property(e => e.VisaPriceSAR).HasColumnName("visapricesar");
            entity.Property(e => e.SARExchangeRate).HasColumnName("sarexchangerate").HasColumnType("decimal(18,6)");
            entity.Property(e => e.AccommodationCost).HasColumnName("accommodationcost");
            entity.Property(e => e.BarcodeCost).HasColumnName("barcodecost");
            entity.Property(e => e.FlightCost).HasColumnName("flightcost");
            entity.Property(e => e.FastTrainPriceSAR).HasColumnName("fasttrainpricesar");
            entity.Property(e => e.BrokerName).HasColumnName("brokername").HasMaxLength(200);
            entity.Property(e => e.SupervisorName).HasColumnName("supervisorname").HasMaxLength(200);
            entity.Property(e => e.BrokerCommission).HasColumnName("brokercommission");
            entity.Property(e => e.SupervisorExpenses).HasColumnName("supervisorexpenses");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedByUserId).HasColumnName("createdbyuserid");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedby");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.Ignore(e => e.VisaPriceEGP);
            entity.Ignore(e => e.FastTrainPriceEGP);
            entity.Ignore(e => e.TotalCostPerPerson);
            entity.Ignore(e => e.TotalTripCost);
            entity.Ignore(e => e.ExpectedRevenue);
            entity.Ignore(e => e.ExpectedProfit);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Updater)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CreatedByUserId);
        });

        // ════════════════════════════════════════════════════════════════
        // UMRAH PILGRIM ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<UmrahPilgrim>(entity =>
        {
            entity.ToTable("umrahpilgrims");
            entity.HasKey(e => e.UmrahPilgrimId);
            
            entity.Property(e => e.UmrahPilgrimId).HasColumnName("umrahpilgrimid");
            entity.Property(e => e.PilgrimNumber).HasColumnName("pilgrimnumber").HasMaxLength(50);
            entity.Property(e => e.UmrahPackageId).HasColumnName("umrahpackageid");
            entity.Property(e => e.FullName).HasColumnName("fullname").HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasColumnName("phonenumber").HasMaxLength(20);
            entity.Property(e => e.IdentityNumber).HasColumnName("identitynumber").HasMaxLength(50);
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.TotalAmount).HasColumnName("totalamount");
            entity.Property(e => e.PaidAmount).HasColumnName("paidamount");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RegisteredAt).HasColumnName("registeredat");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.Ignore(e => e.RemainingAmount);
            entity.Ignore(e => e.PaymentStatus);
            entity.Ignore(e => e.PaymentPercentage);

            entity.HasOne(e => e.Package)
                .WithMany(t => t.Pilgrims)
                .HasForeignKey(e => e.UmrahPackageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UmrahPackageId);
            entity.HasIndex(e => e.CreatedBy);
        });

        // ════════════════════════════════════════════════════════════════
        // UMRAH PAYMENT ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<UmrahPayment>(entity =>
        {
            entity.ToTable("umrahpayments");
            entity.HasKey(e => e.UmrahPaymentId);
            
            entity.Property(e => e.UmrahPaymentId).HasColumnName("umrahpaymentid");
            entity.Property(e => e.PaymentNumber).HasColumnName("paymentnumber").HasMaxLength(50);
            entity.Property(e => e.UmrahPilgrimId).HasColumnName("umrahpilgrimid");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentdate");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentmethod").HasConversion<int>();
            entity.Property(e => e.ReferenceNumber).HasColumnName("referencenumber").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("createdby");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");

            entity.HasOne(e => e.Pilgrim)
                .WithMany(p => p.Payments)
                .HasForeignKey(e => e.UmrahPilgrimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UmrahPilgrimId);
            entity.HasIndex(e => e.CreatedBy);
        });

        // ════════════════════════════════════════════════════════════════
        // USER ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.PasswordHash).HasColumnName("passwordhash");
            entity.Property(e => e.FullName).HasColumnName("fullname");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("roleid");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedat");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.RoleId);
        });

        // ════════════════════════════════════════════════════════════════
        // ROLE ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleId);
            
            entity.Property(e => e.RoleId).HasColumnName("roleid");
            entity.Property(e => e.RoleName).HasColumnName("rolename");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // ════════════════════════════════════════════════════════════════
        // PERMISSION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionId);
            
            entity.Property(e => e.PermissionId).HasColumnName("permissionid");
            entity.Property(e => e.PermissionName).HasColumnName("permissionname");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // ════════════════════════════════════════════════════════════════
        // ROLE PERMISSION ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("rolepermissions");
            entity.HasKey(e => e.RolePermissionId);
            
            entity.Property(e => e.RolePermissionId).HasColumnName("rolepermissionid");
            entity.Property(e => e.RoleId).HasColumnName("roleid");
            entity.Property(e => e.PermissionId).HasColumnName("permissionid");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.PermissionId);
        });

        // ════════════════════════════════════════════════════════════════
        // AUDIT LOG ENTITY CONFIGURATION
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("auditlogs");
            entity.HasKey(e => e.AuditLogId);
            
            entity.Property(e => e.AuditLogId).HasColumnName("auditlogid");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.UserFullName).HasColumnName("userfullname");
            entity.Property(e => e.Action).HasColumnName("action").HasConversion<int>();
            entity.Property(e => e.EntityType).HasColumnName("entitytype");
            entity.Property(e => e.EntityId).HasColumnName("entityid");
            entity.Property(e => e.EntityName).HasColumnName("entityname");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OldValues).HasColumnName("oldvalues");
            entity.Property(e => e.NewValues).HasColumnName("newvalues");
            entity.Property(e => e.Timestamp).HasColumnType("timestamp with time zone").HasColumnName("timestamp");
            entity.Property(e => e.IpAddress).HasColumnName("ipaddress");
            entity.Property(e => e.MachineName).HasColumnName("machinename");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
