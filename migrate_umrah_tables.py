# -*- coding: utf-8 -*-
import sqlite3
import sys
import io

if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Starting Umrah tables migration...")
print("=" * 60)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# ═══════════════════════════════════════════════════════════
# 1. Create umrahpackages table
# ═══════════════════════════════════════════════════════════

cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='umrahpackages'")
if not cursor.fetchone():
    print("Creating umrahpackages table...")
    
    cursor.execute("""
        CREATE TABLE umrahpackages (
            umrahpackageid INTEGER PRIMARY KEY AUTOINCREMENT,
            packagenumber TEXT NOT NULL,
            date TEXT NOT NULL,
            tripname TEXT NOT NULL,
            numberofpersons INTEGER NOT NULL,
            roomtype INTEGER NOT NULL,
            makkahhotel TEXT NOT NULL,
            makkahnights INTEGER NOT NULL,
            madinahhotel TEXT NOT NULL,
            madinahnights INTEGER NOT NULL,
            transportmethod TEXT NOT NULL,
            sellingprice REAL NOT NULL,
            visapricesar REAL NOT NULL,
            sarexchangerate REAL NOT NULL,
            accommodationtotal REAL NOT NULL,
            barcodeprice REAL DEFAULT 0,
            flightprice REAL DEFAULT 0,
            fasttrainpricesar REAL DEFAULT 0,
            brokername TEXT,
            supervisorname TEXT,
            commission REAL DEFAULT 0,
            supervisorexpenses REAL DEFAULT 0,
            status INTEGER DEFAULT 1,
            isactive INTEGER DEFAULT 1,
            notes TEXT,
            createdby INTEGER NOT NULL,
            createdat TEXT NOT NULL,
            updatedby INTEGER,
            updatedat TEXT NOT NULL,
            FOREIGN KEY (createdby) REFERENCES users(userid),
            FOREIGN KEY (updatedby) REFERENCES users(userid)
        )
    """)
    
    print("OK: umrahpackages table created")
else:
    print("OK: umrahpackages table already exists")

# ═══════════════════════════════════════════════════════════
# 2. Create umrahpilgrims table
# ═══════════════════════════════════════════════════════════

cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='umrahpilgrims'")
if not cursor.fetchone():
    print("Creating umrahpilgrims table...")
    
    cursor.execute("""
        CREATE TABLE umrahpilgrims (
            umrahpilgrimid INTEGER PRIMARY KEY AUTOINCREMENT,
            pilgrimnumber TEXT NOT NULL,
            umrahpackageid INTEGER NOT NULL,
            fullname TEXT NOT NULL,
            phonenumber TEXT NOT NULL,
            identitynumber TEXT NOT NULL,
            age INTEGER NOT NULL,
            totalamount REAL NOT NULL,
            paidamount REAL DEFAULT 0,
            status INTEGER DEFAULT 1,
            notes TEXT,
            registeredat TEXT NOT NULL,
            createdby INTEGER NOT NULL,
            updatedat TEXT NOT NULL,
            FOREIGN KEY (umrahpackageid) REFERENCES umrahpackages(umrahpackageid) ON DELETE CASCADE,
            FOREIGN KEY (createdby) REFERENCES users(userid)
        )
    """)
    
    print("OK: umrahpilgrims table created")
else:
    print("OK: umrahpilgrims table already exists")

# ═══════════════════════════════════════════════════════════
# 3. Create umrahpayments table
# ═══════════════════════════════════════════════════════════

cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='umrahpayments'")
if not cursor.fetchone():
    print("Creating umrahpayments table...")
    
    cursor.execute("""
        CREATE TABLE umrahpayments (
            umrahpaymentid INTEGER PRIMARY KEY AUTOINCREMENT,
            paymentnumber TEXT NOT NULL,
            umrahpilgrimid INTEGER NOT NULL,
            paymentdate TEXT NOT NULL,
            amount REAL NOT NULL,
            paymentmethod INTEGER NOT NULL,
            referencenumber TEXT,
            notes TEXT,
            createdby INTEGER NOT NULL,
            createdat TEXT NOT NULL,
            FOREIGN KEY (umrahpilgrimid) REFERENCES umrahpilgrims(umrahpilgrimid) ON DELETE CASCADE,
            FOREIGN KEY (createdby) REFERENCES users(userid)
        )
    """)
    
    print("OK: umrahpayments table created")
else:
    print("OK: umrahpayments table already exists")

# Commit changes
conn.commit()

# Verify tables
print("\n" + "=" * 60)
print("Verification:")
print("=" * 60)

for table_name in ['umrahpackages', 'umrahpilgrims', 'umrahpayments']:
    cursor.execute(f"PRAGMA table_info({table_name})")
    columns = cursor.fetchall()
    print(f"\n{table_name} ({len(columns)} columns):")
    for col in columns:
        pk = " (PK)" if col[5] else ""
        print(f"  - {col[1]:20} {col[2]:10}{pk}")

conn.close()

print("\n" + "=" * 60)
print("Migration completed successfully!")
print("=" * 60)
