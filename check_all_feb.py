import psycopg2

try:
    conn = psycopg2.connect(
        host="localhost",
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    c = conn.cursor()
    
    # Get column names for cashboxes
    c.execute("""
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'cashboxes'
        ORDER BY ordinal_position
    """)
    cols = c.fetchall()
    print("Cashboxes columns:", [c[0] for c in cols])
    
    # Get all cashboxes
    print("\n=== All CashBoxes ===")
    c.execute('SELECT cashboxid, cashboxname, currentbalance FROM cashboxes WHERE isdeleted = false')
    cashboxes = c.fetchall()
    for cb in cashboxes:
        print(f"  CashBox ID={cb[0]}: {cb[1]} - Balance: {cb[2]}")
    
    # Check ALL transactions for February 2026
    print("\n=== ALL Transactions for February 2026 ===")
    query = """
        SELECT transactionid, cashboxid, amount, transactiontype, "TransactionCurrency", description
        FROM cashtransactions 
        WHERE month = 2 AND year = 2026 AND isdeleted = false
        ORDER BY cashboxid, transactionid
    """
    
    c.execute(query)
    rows = c.fetchall()
    
    print(f"Total: {len(rows)} transactions\n")
    for r in rows:
        trans_id, cashbox_id, amount, trans_type, trans_currency, desc = r
        currency_display = trans_currency if trans_currency else "EGP"
        type_display = "INCOME" if trans_type == 0 else "EXPENSE"
        print(f"[{type_display}] ID={trans_id}, CashBox={cashbox_id}, Amount={amount}, Currency={currency_display}")
    
    # Summary by CashBox
    print("\n=== Summary by CashBox for February 2026 ===")
    c.execute("""
        SELECT cashboxid, transactiontype, "TransactionCurrency", COUNT(*), SUM(amount)
        FROM cashtransactions
        WHERE month = 2 AND year = 2026 AND isdeleted = false
        GROUP BY cashboxid, transactiontype, "TransactionCurrency"
        ORDER BY cashboxid, transactiontype
    """)
    stats = c.fetchall()
    for s in stats:
        cashbox_id, trans_type, curr, count, total = s
        currency_display = curr if curr else "EGP"
        type_display = "INCOME" if trans_type == 0 else "EXPENSE"
        print(f"  CashBox={cashbox_id}, {type_display}, {currency_display}: {count} trans, Total={total}")
    
    conn.close()
    print("\nDone!")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
