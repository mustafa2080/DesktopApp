# -*- coding: utf-8 -*-
import sqlite3
import sys
import io

# Force UTF-8 for console
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Starting Flight Bookings table creation/update...")
print("=" * 60)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Check if table exists
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='flightbookings'")
table_exists = cursor.fetchone() is not None

if not table_exists:
    print("Creating flightbookings table...")
    
    cursor.execute("""
        CREATE TABLE flightbookings (
            flightbookingid INTEGER PRIMARY KEY AUTOINCREMENT,
            bookingnumber TEXT NOT NULL,
            issuancedate TEXT NOT NULL,
            traveldate TEXT NOT NULL,
            clientname TEXT NOT NULL,
            clientroute TEXT,
            supplier TEXT NOT NULL,
            system TEXT NOT NULL,
            ticketstatus TEXT NOT NULL,
            paymentmethod TEXT NOT NULL,
            sellingprice REAL NOT NULL,
            netprice REAL NOT NULL,
            ticketcount INTEGER NOT NULL,
            ticketnumbers TEXT,
            mobilenumber TEXT NOT NULL,
            notes TEXT,
            createdbyuserid INTEGER NOT NULL,
            createdat TEXT NOT NULL,
            updatedat TEXT NOT NULL,
            FOREIGN KEY (createdbyuserid) REFERENCES users(userid)
        )
    """)
    
    print("OK: Table created successfully")
    
else:
    print("Table exists, checking for CreatedByUserId column...")
    
    # Check if CreatedByUserId column exists
    cursor.execute("PRAGMA table_info(flightbookings)")
    columns = cursor.fetchall()
    column_names = [col[1].lower() for col in columns]
    
    if 'createdbyuserid' not in column_names:
        print("Adding CreatedByUserId column...")
        
        try:
            # Add the column with default value 1 (admin user)
            cursor.execute("""
                ALTER TABLE flightbookings 
                ADD COLUMN createdbyuserid INTEGER NOT NULL DEFAULT 1
            """)
            
            print("OK: Column added successfully")
            
            # Add foreign key constraint (SQLite doesn't support adding FK to existing table,
            # so we note it for reference)
            print("Note: Foreign key constraint should be: REFERENCES users(userid)")
            
        except sqlite3.OperationalError as e:
            print(f"Error adding column: {e}")
    else:
        print("OK: CreatedByUserId column already exists")

# Commit changes
conn.commit()

# Verify final structure
print("\nFinal table structure:")
print("-" * 60)
cursor.execute("PRAGMA table_info(flightbookings)")
columns = cursor.fetchall()
for col in columns:
    pk = " (PRIMARY KEY)" if col[5] else ""
    notnull = " NOT NULL" if col[3] else ""
    default = f" DEFAULT {col[4]}" if col[4] else ""
    print(f"  {col[1]:20} {col[2]:10}{notnull}{default}{pk}")

conn.close()

print("\n" + "=" * 60)
print("Migration completed successfully!")
