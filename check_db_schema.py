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
print("STEP 1: Check BankTransfers table schema")
print("="*80)

# Get table structure
cursor.execute("""
    SELECT 
        column_name, 
        data_type, 
        character_maximum_length,
        is_nullable,
        column_default
    FROM information_schema.columns
    WHERE table_schema = 'public' 
    AND table_name = 'banktransfers'
    ORDER BY ordinal_position
""")

columns = cursor.fetchall()
print("\nTable: banktransfers")
print("-" * 80)
for col in columns:
    print(f"Column: {col[0]:30s} Type: {col[1]:20s} Nullable: {col[3]:5s} Default: {col[4]}")

print("\n" + "="*80)
print("STEP 2: Check Amount column details")
print("="*80)

cursor.execute("""
    SELECT 
        column_name,
        data_type,
        numeric_precision,
        numeric_scale,
        is_nullable
    FROM information_schema.columns
    WHERE table_name = 'banktransfers' 
    AND column_name = 'amount'
""")

amount_col = cursor.fetchone()
if amount_col:
    print(f"\nAmount Column Details:")
    print(f"  Name: {amount_col[0]}")
    print(f"  Type: {amount_col[1]}")
    print(f"  Precision: {amount_col[2]}")
    print(f"  Scale: {amount_col[3]}")
    print(f"  Nullable: {amount_col[4]}")
else:
    print("ERROR: Amount column not found!")

print("\n" + "="*80)
print("STEP 3: Check constraints on banktransfers table")
print("="*80)

cursor.execute("""
    SELECT
        tc.constraint_name,
        tc.constraint_type,
        cc.check_clause
    FROM information_schema.table_constraints tc
    LEFT JOIN information_schema.check_constraints cc 
        ON tc.constraint_name = cc.constraint_name
    WHERE tc.table_name = 'banktransfers'
    ORDER BY tc.constraint_type
""")

constraints = cursor.fetchall()
if constraints:
    for const in constraints:
        print(f"\nConstraint: {const[0]}")
        print(f"  Type: {const[1]}")
        if const[2]:
            print(f"  Check: {const[2]}")
else:
    print("No constraints found")

print("\n" + "="*80)
print("STEP 4: Check actual data - Fawateerk payments with amount issues")
print("="*80)

cursor.execute("""
    SELECT 
        id,
        amount,
        pg_typeof(amount) as amount_type,
        transferdate,
        referencenumber,
        transfertype
    FROM banktransfers
    WHERE transfertype = 'FawateerkPayment'
    ORDER BY id
    LIMIT 10
""")

payments = cursor.fetchall()
print(f"\nFound {len(payments)} Fawateerk payments:")
print("-" * 80)
for p in payments:
    print(f"ID: {p[0]:4d} | Amount: {p[1]:10} | Type: {p[2]:20s} | Ref: {p[4]}")
    if p[1] is None:
        print("  WARNING: Amount is NULL!")
    elif p[1] == 0:
        print("  WARNING: Amount is ZERO!")
    elif p[1] < 0:
        print("  WARNING: Amount is NEGATIVE!")

print("\n" + "="*80)
print("STEP 5: Statistics")
print("="*80)

cursor.execute("""
    SELECT 
        COUNT(*) as total,
        COUNT(CASE WHEN amount IS NULL THEN 1 END) as null_amounts,
        COUNT(CASE WHEN amount = 0 THEN 1 END) as zero_amounts,
        COUNT(CASE WHEN amount < 0 THEN 1 END) as negative_amounts,
        MIN(amount) as min_amount,
        MAX(amount) as max_amount,
        AVG(amount) as avg_amount
    FROM banktransfers
    WHERE transfertype = 'FawateerkPayment'
""")

stats = cursor.fetchone()
print(f"\nFawateerk Payments Statistics:")
print(f"  Total payments: {stats[0]}")
print(f"  NULL amounts: {stats[1]}")
print(f"  ZERO amounts: {stats[2]}")
print(f"  NEGATIVE amounts: {stats[3]}")
print(f"  Min amount: {stats[4]}")
print(f"  Max amount: {stats[5]}")
print(f"  Avg amount: {stats[6]}")

conn.close()
print("\n" + "="*80)
print("Check completed!")
print("="*80)
