import psycopg2
from datetime import datetime
import sys

# Fix encoding for Windows console
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

try:
    conn = psycopg2.connect(
        host="localhost",
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    c = conn.cursor()
    
    print("=" * 80)
    print("  تقرير شامل - التقرير الشهري لجميع الخزن")
    print("=" * 80)
    
    # Get all cashboxes
    c.execute('SELECT cashboxid, cashboxname, currentbalance FROM cashboxes WHERE isdeleted = false ORDER BY cashboxid')
    cashboxes = c.fetchall()
    
    for cashbox in cashboxes:
        cashbox_id, cashbox_name, current_balance = cashbox
        
        print(f"\n{'=' * 80}")
        print(f"  خزنة: {cashbox_name} (ID: {cashbox_id})")
        print(f"  الرصيد الحالي: {current_balance} جنيه")
        print(f"{'=' * 80}")
        
        # Get transactions for Feb 2026
        c.execute("""
            SELECT transactionid, transactiondate, amount, transactiontype, 
                   "TransactionCurrency", description, vouchernumber,
                   category, paymentmethod, instapaycommission
            FROM cashtransactions 
            WHERE cashboxid = %s AND month = 2 AND year = 2026 AND isdeleted = false
            ORDER BY transactiondate, transactionid
        """, (cashbox_id,))
        
        transactions = c.fetchall()
        
        if not transactions:
            print("\n  ⚠️  لا توجد معاملات لشهر فبراير 2026")
            continue
        
        # Calculate totals
        total_income_egp = 0
        total_expense_egp = 0
        total_income_usd = 0
        total_expense_usd = 0
        total_income_eur = 0
        total_expense_eur = 0
        
        income_transactions = []
        expense_transactions = []
        
        for trans in transactions:
            trans_id, trans_date, amount, trans_type, currency, desc, voucher, category, payment_method, commission = trans
            
            currency = currency if currency else "EGP"
            
            if trans_type == 0:  # Income
                income_transactions.append(trans)
                if currency == "EGP":
                    total_income_egp += amount
                elif currency == "USD":
                    total_income_usd += amount
                elif currency == "EUR":
                    total_income_eur += amount
            else:  # Expense
                expense_transactions.append(trans)
                # في حالة InstaPay، المبلغ الفعلي = Amount + Commission
                actual_amount = amount
                if payment_method == 4 and commission:  # InstaPay
                    actual_amount = amount + commission
                
                if currency == "EGP":
                    total_expense_egp += actual_amount
                elif currency == "USD":
                    total_expense_usd += actual_amount
                elif currency == "EUR":
                    total_expense_eur += actual_amount
        
        # Display Income
        print(f"\n  ✅ الإيرادات ({len(income_transactions)} معاملة)")
        print(f"  {'-' * 76}")
        
        if income_transactions:
            for trans in income_transactions:
                trans_id, trans_date, amount, trans_type, currency, desc, voucher, category, payment_method, commission = trans
                currency = currency if currency else "EGP"
                print(f"    [{trans_date.strftime('%Y-%m-%d')}] {voucher or 'N/A'} | {amount:>10.2f} {currency} | {desc}")
        else:
            print(f"    ⚠️  لا توجد إيرادات")
        
        # Display Expenses
        print(f"\n  ❌ المصروفات ({len(expense_transactions)} معاملة)")
        print(f"  {'-' * 76}")
        
        if expense_transactions:
            for trans in expense_transactions:
                trans_id, trans_date, amount, trans_type, currency, desc, voucher, category, payment_method, commission = trans
                currency = currency if currency else "EGP"
                commission_text = f" (عمولة: {commission})" if commission else ""
                print(f"    [{trans_date.strftime('%Y-%m-%d')}] {voucher or 'N/A'} | {amount:>10.2f} {currency}{commission_text} | {desc}")
        else:
            print(f"    ⚠️  لا توجد مصروفات")
        
        # Display Summary
        print(f"\n  📊 الملخص - فبراير 2026")
        print(f"  {'=' * 76}")
        
        print(f"\n  💰 إجمالي الإيرادات (EGP):          {total_income_egp:>15.2f} جنيه")
        print(f"  💸 إجمالي المصروفات (EGP):         {total_expense_egp:>15.2f} جنيه")
        print(f"  📈 صافي الربح/الخسارة (EGP):        {(total_income_egp - total_expense_egp):>15.2f} جنيه")
        
        if total_income_usd > 0 or total_expense_usd > 0:
            print(f"\n  💵 دولار:")
            print(f"     إيرادات: {total_income_usd:>10.2f} USD | مصروفات: {total_expense_usd:>10.2f} USD | صافي: {(total_income_usd - total_expense_usd):>10.2f} USD")
        
        if total_income_eur > 0 or total_expense_eur > 0:
            print(f"\n  💶 يورو:")
            print(f"     إيرادات: {total_income_eur:>10.2f} EUR | مصروفات: {total_expense_eur:>10.2f} EUR | صافي: {(total_income_eur - total_expense_eur):>10.2f} EUR")
    
    conn.close()
    
    print(f"\n{'=' * 80}")
    print(f"  تم إنشاء التقرير بنجاح - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"{'=' * 80}\n")
    
except Exception as e:
    print(f"خطأ: {e}")
    import traceback
    traceback.print_exc()
