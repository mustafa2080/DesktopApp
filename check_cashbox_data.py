import psycopg2

try:
    conn = psycopg2.connect(
        host='localhost',
        port=5432,
        database='graceway_accounting',
        user='postgres',
        password='123456'
    )
    cursor = conn.cursor()
    
    # Check CashBoxes
    print("=== CashBoxes ===")
    cursor.execute('''
        SELECT cashboxid, cashboxname, currentbalance, isactive
        FROM cashboxes 
        WHERE isdeleted = false
        ORDER BY cashboxid
    ''')
    boxes = cursor.fetchall()
    for box in boxes:
        print(f"ID: {box[0]}, Name: {box[1]}, Balance: {box[2]}, Active: {box[3]}")
    
    if boxes:
        cashbox_id = boxes[0][0]
        print(f"\n=== Transactions for CashBox {cashbox_id} ===")
        
        # Check transactions
        cursor.execute('''
            SELECT transactionid, transactiondate, transactiontype, 
                   amount, "TransactionCurrency", category, description
            FROM cashtransactions
            WHERE cashboxid = %s AND isdeleted = false
            ORDER BY transactionid DESC
            LIMIT 10
        ''', (cashbox_id,))
        
        transactions = cursor.fetchall()
        if transactions:
            for trans in transactions:
                ttype = "Income" if trans[2] == 0 else "Expense"
                print(f"ID: {trans[0]}, Date: {trans[1]}, Type: {ttype}, "
                      f"Amount: {trans[3]}, Currency: {trans[4]}, "
                      f"Category: {trans[5]}")
        else:
            print("No transactions found!")
        
        # Check for February 2026
        print(f"\n=== February 2026 Transactions ===")
        cursor.execute('''
            SELECT COUNT(*), 
                   SUM(CASE WHEN transactiontype = 0 THEN amount ELSE 0 END) as income,
                   SUM(CASE WHEN transactiontype = 1 THEN amount ELSE 0 END) as expense
            FROM cashtransactions
            WHERE cashboxid = %s 
              AND month = 2 
              AND year = 2026
              AND isdeleted = false
        ''', (cashbox_id,))
        
        result = cursor.fetchone()
        print(f"Count: {result[0]}, Income: {result[1]}, Expense: {result[2]}")
        
        # Check currencies
        print(f"\n=== Transactions by Currency (Feb 2026) ===")
        cursor.execute('''
            SELECT "TransactionCurrency", COUNT(*), SUM(amount)
            FROM cashtransactions
            WHERE cashboxid = %s 
              AND month = 2 
              AND year = 2026
              AND isdeleted = false
            GROUP BY "TransactionCurrency"
        ''', (cashbox_id,))
        
        currencies = cursor.fetchall()
        if currencies:
            for curr in currencies:
                print(f"Currency: {curr[0] or 'NULL/EGP'}, Count: {curr[1]}, Total: {curr[2]}")
        else:
            print("No currency data found")
    else:
        print("No cashboxes found!")
    
    conn.close()
    print("\nDone!")

except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
