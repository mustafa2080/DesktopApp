import psycopg2

try:
    conn = psycopg2.connect(
        host="localhost",
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    c = conn.cursor()
    
    # Check transactions for February 2026
    query = """
        SELECT transactionid, amount, transactiontype, "TransactionCurrency", month, year, description, cashboxid
        FROM cashtransactions 
        WHERE month = 2 AND year = 2026 AND isdeleted = false
        ORDER BY transactionid
    """
    
    c.execute(query)
    rows = c.fetchall()
    
    print(f"=== Transactions for February 2026 ({len(rows)} found) ===\n")
    for r in rows:
        trans_id, amount, trans_type, trans_currency, month, year, desc, cashbox_id = r
        currency_display = trans_currency if trans_currency else "NULL"
        type_display = "Income" if trans_type == 0 else "Expense"
        print(f"ID={trans_id}, CashBox={cashbox_id}, Amount={amount}, Type={type_display}, Currency=[{currency_display}], Desc={desc}")
    
    # Check unique currency values
    print("\n=== Unique Currency Values in Database ===")
    c.execute('SELECT DISTINCT "TransactionCurrency" FROM cashtransactions WHERE isdeleted = false')
    currencies = c.fetchall()
    for curr in currencies:
        val = curr[0] if curr[0] else "NULL"
        print(f"  [{val}]")
    
    # Count by currency for Feb 2026
    print("\n=== February 2026 Transactions by Currency & Type ===")
    c.execute("""
        SELECT "TransactionCurrency", transactiontype, COUNT(*), SUM(amount)
        FROM cashtransactions
        WHERE month = 2 AND year = 2026 AND isdeleted = false
        GROUP BY "TransactionCurrency", transactiontype
        ORDER BY transactiontype, "TransactionCurrency"
    """)
    stats = c.fetchall()
    for s in stats:
        curr = s[0] if s[0] else "NULL"
        trans_type = "Income" if s[1] == 0 else "Expense"
        count = s[2]
        total = s[3]
        print(f"  Type={trans_type}, Currency=[{curr}]: {count} transactions, Total Amount: {total}")
    
    conn.close()
    print("\nDone!")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
