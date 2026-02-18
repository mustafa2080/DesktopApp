import psycopg2

conn = psycopg2.connect(
    host='localhost',
    port=5432,
    database='graceway_accounting',
    user='postgres',
    password='123456'
)
conn.autocommit = True
cursor = conn.cursor()

print("="*80)
print("Testing NumericUpDown behavior with PostgreSQL numeric type")
print("="*80)

# Get the exact payment
cursor.execute("""
    SELECT 
        id,
        amount,
        CAST(amount AS text) as amount_text,
        CAST(amount AS decimal(18,2)) as amount_decimal,
        CAST(amount AS float) as amount_float,
        pg_typeof(amount) as type_info
    FROM banktransfers
    WHERE transfertype = 'FawateerkPayment'
    AND id = 2
""")

payment = cursor.fetchone()

if payment:
    print(f"\nPayment ID: {payment[0]}")
    print(f"Amount (raw): {payment[1]}")
    print(f"Amount (as text): '{payment[2]}'")
    print(f"Amount (as decimal): {payment[3]}")
    print(f"Amount (as float): {payment[4]}")
    print(f"Type: {payment[5]}")
    print(f"\nPython type: {type(payment[1])}")
    print(f"Python value: {repr(payment[1])}")
    
    # Check if it's exactly 0
    if payment[1] == 0:
        print("\nWARNING: Amount equals 0!")
    elif payment[1] < 0.01:
        print(f"\nWARNING: Amount ({payment[1]}) is less than 0.01!")
    else:
        print("\nOK: Amount is valid")
        
    # Test comparison
    print(f"\nAmount > 0: {payment[1] > 0}")
    print(f"Amount >= 0.01: {payment[1] >= 0.01}")
    print(f"Amount == 500: {payment[1] == 500}")
else:
    print("ERROR: Payment not found!")

# Check if there are any OTHER payments that might cause issues
print("\n" + "="*80)
print("Checking for problematic payments in ALL BankTransfers")
print("="*80)

cursor.execute("""
    SELECT 
        id,
        transfertype,
        amount,
        CAST(amount AS text) as amount_text
    FROM banktransfers
    WHERE amount < 0.01 OR amount IS NULL
    LIMIT 20
""")

problematic = cursor.fetchall()
if problematic:
    print(f"\nFound {len(problematic)} payments with amount < 0.01 or NULL:")
    for p in problematic:
        print(f"  ID: {p[0]:4d} | Type: {p[1]:20s} | Amount: {p[2]}")
else:
    print("\nNo problematic payments found!")

conn.close()
