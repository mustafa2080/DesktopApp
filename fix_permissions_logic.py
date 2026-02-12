import os
import time

sidebar_file = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\SidebarControl.cs'

# Read the file
with open(sidebar_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace ApplyPermissionsSync method
old_apply = '''    private void ApplyPermissionsSync(Dictionary<string, List<PermissionType>> permissionsByModule)
    {
        Console.WriteLine("\\n🔧 ApplyPermissionsSync started");
        Console.WriteLine($"   Modules received: {string.Join(", ", permissionsByModule.Keys)}");
        
        // Check if user is admin (has System module permissions)
        bool isAdmin = permissionsByModule.ContainsKey("System");
        
        // Check module permissions
        bool hasAviation = permissionsByModule.ContainsKey("Aviation");
        bool hasUmrah = permissionsByModule.ContainsKey("Umrah");
        bool hasTrips = permissionsByModule.ContainsKey("Trips");
        bool hasReports = permissionsByModule.ContainsKey("Reports");
        bool hasAccounting = permissionsByModule.ContainsKey("Accounting");
        bool hasCalculator = permissionsByModule.Values.Any(list => list.Contains(PermissionType.UseCalculator));'''

new_apply = '''    private void ApplyPermissionsSync(Dictionary<string, List<PermissionType>> permissionsByModule)
    {
        Console.WriteLine("\\n🔧 ApplyPermissionsSync started");
        Console.WriteLine($"   Modules received: {string.Join(", ", permissionsByModule.Keys)}");
        
        // Check if user is admin (has System module permissions)
        bool isAdmin = permissionsByModule.ContainsKey("System");
        
        // Check module permissions
        bool hasAviation = permissionsByModule.ContainsKey("Aviation");
        bool hasUmrah = permissionsByModule.ContainsKey("Umrah");
        bool hasTrips = permissionsByModule.ContainsKey("Trips");
        bool hasReports = permissionsByModule.ContainsKey("Reports");
        bool hasAccounting = permissionsByModule.ContainsKey("Accounting");
        bool hasOperations = permissionsByModule.ContainsKey("Operations");
        bool hasCalculator = permissionsByModule.Values.Any(list => list.Contains(PermissionType.UseCalculator));'''

content = content.replace(old_apply, new_apply)

# Replace the entire permissions application logic
old_permissions_logic = '''        Console.WriteLine($"   isAdmin: {isAdmin}");
        Console.WriteLine($"   hasAviation: {hasAviation}");
        Console.WriteLine($"   hasUmrah: {hasUmrah}");
        Console.WriteLine($"   hasTrips: {hasTrips}");
        Console.WriteLine($"   hasReports: {hasReports}");
        Console.WriteLine($"   hasAccounting: {hasAccounting}");
        Console.WriteLine($"   hasCalculator: {hasCalculator}");
        
        if (isAdmin)
        {
            // Admin sees everything
            Console.WriteLine("👑 User is Admin - showing all menu items");
            SetMenuItemVisibility("dashboard", true);
            SetMenuItemVisibility("settings", true);
            SetMenuItemVisibility("users", true);
            SetMenuItemVisibility("accounts", true);
            SetMenuItemVisibility("customers", true);
            SetMenuItemVisibility("suppliers", true);
            SetMenuItemVisibility("reservations", true);
            SetMenuItemVisibility("flights", true);
            SetMenuItemVisibility("trips", true);
            SetMenuItemVisibility("umrah", true);
            SetMenuItemVisibility("invoices", true);
            SetMenuItemVisibility("cashbox", true);
            SetMenuItemVisibility("banks", true);
            SetMenuItemVisibility("journals", true);
            SetMenuItemVisibility("reports", true);
            SetMenuItemVisibility("accounting_reports", true);
            SetMenuItemVisibility("calculator", true);
        }
        else
        {
            // Non-admin: apply permissions based on modules
            Console.WriteLine("👤 Regular user - applying permission filters");
            
            // Hide all sections first (except dashboard which is always visible)
            Console.WriteLine("   Hiding all sections first...");
            SetMenuItemVisibility("dashboard", true); // Always visible
            SetMenuItemVisibility("settings", false);
            SetMenuItemVisibility("users", false);
            SetMenuItemVisibility("accounts", false);
            SetMenuItemVisibility("customers", false);
            SetMenuItemVisibility("suppliers", false);
            SetMenuItemVisibility("reservations", false);
            SetMenuItemVisibility("flights", false);
            SetMenuItemVisibility("trips", false);
            SetMenuItemVisibility("umrah", false);
            SetMenuItemVisibility("invoices", false);
            SetMenuItemVisibility("cashbox", false);
            SetMenuItemVisibility("banks", false);
            SetMenuItemVisibility("journals", false);
            SetMenuItemVisibility("reports", false);
            SetMenuItemVisibility("accounting_reports", false);
            SetMenuItemVisibility("calculator", false);
            
            // Show sections based on permissions (EACH MODULE IS INDEPENDENT)
            
            // Operations Department (has Trips module)
            if (hasTrips)
            {
                Console.WriteLine("🚌 User has Trips module - showing Trips");
                SetMenuItemVisibility("trips", true);
            }
            
            // Aviation Department (has Aviation module)
            if (hasAviation)
            {
                Console.WriteLine("✈️ User has Aviation module - showing Flights");
                SetMenuItemVisibility("flights", true);
            }
            
            // Umrah Department (has Umrah module)
            if (hasUmrah)
            {
                Console.WriteLine("🕌 User has Umrah module - showing Umrah");
                SetMenuItemVisibility("umrah", true);
            }
            
            // Reports (has Reports module)
            if (hasReports)
            {
                Console.WriteLine("📊 User has Reports module - showing Reports");
                SetMenuItemVisibility("reports", true);
            }
            
            // Accounting sections (has Accounting module)
            if (hasAccounting)
            {
                Console.WriteLine("💼 User has Accounting module - showing accounting sections");
                
                // Check specific accounting permissions
                var accountingPerms = permissionsByModule["Accounting"];
                
                if (accountingPerms.Any(p => p == PermissionType.ViewCustomers))
                    SetMenuItemVisibility("customers", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewSuppliers))
                    SetMenuItemVisibility("suppliers", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewInvoices))
                    SetMenuItemVisibility("invoices", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewCashBox))
                    SetMenuItemVisibility("cashbox", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewBankAccounts))
                    SetMenuItemVisibility("banks", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewJournalEntries))
                    SetMenuItemVisibility("journals", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewChartOfAccounts))
                    SetMenuItemVisibility("accounts", true);
                
                if (accountingPerms.Any(p => p == PermissionType.ViewFinancialReports))
                    SetMenuItemVisibility("accounting_reports", true);
            }
            
            // Operations module (has Operations module for reservations)
            if (permissionsByModule.ContainsKey("Operations"))
            {
                Console.WriteLine("📋 User has Operations module - showing Reservations");
                SetMenuItemVisibility("reservations", true);
            }
            
            // Calculator - show if user has calculator permission in ANY module
            if (hasCalculator)
            {
                Console.WriteLine("🧮 User has Calculator permission - showing Calculator");
                SetMenuItemVisibility("calculator", true);
            }
            else
            {
                Console.WriteLine("⚠️ User does NOT have Calculator permission - hiding Calculator");
                SetMenuItemVisibility("calculator", false);
            }
        }
        
        Console.WriteLine("🔧 ApplyPermissionsSync completed\\n");
    }

    private void SetMenuItemVisibility(string menuId, bool visible)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuId);
        if (menuItem != null)
        {
            menuItem.Visible = visible;
            Console.WriteLine($"   {menuId}: {(visible ? "✓ VISIBLE" : "✗ HIDDEN")}");
        }
        else
        {
            Console.WriteLine($"   ⚠️ Menu item '{menuId}' NOT FOUND!");
        }
    }'''

new_permissions_logic = '''        Console.WriteLine($"   isAdmin: {isAdmin}");
        Console.WriteLine($"   hasAviation: {hasAviation}");
        Console.WriteLine($"   hasUmrah: {hasUmrah}");
        Console.WriteLine($"   hasTrips: {hasTrips}");
        Console.WriteLine($"   hasReports: {hasReports}");
        Console.WriteLine($"   hasAccounting: {hasAccounting}");
        Console.WriteLine($"   hasOperations: {hasOperations}");
        Console.WriteLine($"   hasCalculator: {hasCalculator}");
        
        if (isAdmin)
        {
            // Admin has access to EVERYTHING - enable all
            Console.WriteLine("👑 User is Admin - enabling ALL menu items");
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("settings", true, true);
            SetMenuItemEnabled("users", true, true);
            SetMenuItemEnabled("accounts", true, true);
            SetMenuItemEnabled("customers", true, true);
            SetMenuItemEnabled("suppliers", true, true);
            SetMenuItemEnabled("reservations", true, true);
            SetMenuItemEnabled("flights", true, true);
            SetMenuItemEnabled("trips", true, true);
            SetMenuItemEnabled("umrah", true, true);
            SetMenuItemEnabled("invoices", true, true);
            SetMenuItemEnabled("cashbox", true, true);
            SetMenuItemEnabled("banks", true, true);
            SetMenuItemEnabled("journals", true, true);
            SetMenuItemEnabled("reports", true, true);
            SetMenuItemEnabled("accounting_reports", true, true);
            SetMenuItemEnabled("calculator", true, true);
        }
        else
        {
            // Non-admin: SHOW ALL sections but DISABLE those without permissions
            Console.WriteLine("👤 Regular user - showing all, disabling restricted sections");
            
            // Dashboard is always enabled
            SetMenuItemEnabled("dashboard", true, true);
            
            // Admin-only sections - always disabled for non-admin
            SetMenuItemEnabled("settings", true, false);
            SetMenuItemEnabled("users", true, false);
            
            // Aviation module sections
            SetMenuItemEnabled("flights", true, hasAviation);
            SetMenuItemEnabled("reservations", true, hasAviation);
            
            // Umrah module sections
            SetMenuItemEnabled("umrah", true, hasUmrah);
            
            // Operations/Trips module sections
            SetMenuItemEnabled("trips", true, hasTrips || hasOperations);
            
            // Calculator - available for Aviation and Operations
            SetMenuItemEnabled("calculator", true, hasCalculator || hasAviation || hasOperations);
            
            // Accounting sections - check specific permissions
            if (hasAccounting)
            {
                var accountingPerms = permissionsByModule["Accounting"];
                
                SetMenuItemEnabled("customers", true, accountingPerms.Any(p => p == PermissionType.ViewCustomers));
                SetMenuItemEnabled("suppliers", true, accountingPerms.Any(p => p == PermissionType.ViewSuppliers));
                SetMenuItemEnabled("invoices", true, accountingPerms.Any(p => p == PermissionType.ViewInvoices));
                SetMenuItemEnabled("cashbox", true, accountingPerms.Any(p => p == PermissionType.ViewCashBox));
                SetMenuItemEnabled("banks", true, accountingPerms.Any(p => p == PermissionType.ViewBankAccounts));
                SetMenuItemEnabled("journals", true, accountingPerms.Any(p => p == PermissionType.ViewJournalEntries));
                SetMenuItemEnabled("accounts", true, accountingPerms.Any(p => p == PermissionType.ViewChartOfAccounts));
                SetMenuItemEnabled("accounting_reports", true, accountingPerms.Any(p => p == PermissionType.ViewFinancialReports));
            }
            else
            {
                // No accounting access - disable all accounting sections
                SetMenuItemEnabled("customers", true, false);
                SetMenuItemEnabled("suppliers", true, false);
                SetMenuItemEnabled("invoices", true, false);
                SetMenuItemEnabled("cashbox", true, false);
                SetMenuItemEnabled("banks", true, false);
                SetMenuItemEnabled("journals", true, false);
                SetMenuItemEnabled("accounts", true, false);
                SetMenuItemEnabled("accounting_reports", true, false);
            }
            
            // Reports section
            SetMenuItemEnabled("reports", true, hasReports);
        }
        
        Console.WriteLine("🔧 ApplyPermissionsSync completed\\n");
    }

    private void SetMenuItemEnabled(string menuId, bool visible, bool enabled)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuId);
        if (menuItem != null)
        {
            menuItem.Visible = visible;
            menuItem.SetEnabled(enabled);
            
            string status = visible ? (enabled ? "✓ ENABLED" : "⊘ DISABLED") : "✗ HIDDEN";
            Console.WriteLine($"   {menuId}: {status}");
        }
        else
        {
            Console.WriteLine($"   ⚠️ Menu item '{menuId}' NOT FOUND!");
        }
    }'''

content = content.replace(old_permissions_logic, new_permissions_logic)

# Write the modified content
with open(sidebar_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("Success! Permissions logic updated")
print("Now all sections are VISIBLE but DISABLED when no permissions")
print("Admin: Everything enabled")
print("Aviation user: Flights, Reservations, Calculator enabled")
print("Operations user: Trips, Calculator enabled")
print("Others disabled with warning message")
