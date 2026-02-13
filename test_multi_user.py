"""
Multi-User Testing Script
Tests concurrent access and data integrity
"""

import psycopg2
import threading
import time
from datetime import datetime

# Database connection settings
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'database': 'graceway_accounting',
    'user': 'postgres',
    'password': '123456'
}

def get_connection():
    """Create a new database connection"""
    return psycopg2.connect(**DB_CONFIG)

def test_concurrent_updates():
    """Test concurrent updates to the same cashbox"""
    print("\n=== Testing Concurrent Updates ===")
    
    def update_cashbox(user_id, delay=0):
        try:
            time.sleep(delay)
            conn = get_connection()
            cur = conn.cursor()
            
            # Get current balance
            cur.execute("""
                SELECT currentbalance, xmin 
                FROM "CashBoxes" 
                WHERE cashboxid = 1 AND isdeleted = false
            """)
            result = cur.fetchone()
            if not result:
                print(f"User {user_id}: CashBox not found")
                return
            
            current_balance, xmin = result
            print(f"User {user_id}: Current balance = {current_balance}, xmin = {xmin}")
            
            # Simulate some work
            time.sleep(0.5)
            
            # Try to update
            new_balance = current_balance + 100
            cur.execute("""
                UPDATE "CashBoxes" 
                SET currentbalance = %s, updatedat = NOW()
                WHERE cashboxid = 1 AND xmin = %s
            """, (new_balance, xmin))
            
            if cur.rowcount == 0:
                print(f"User {user_id}: ❌ Update failed - concurrent modification detected")
                conn.rollback()
            else:
                conn.commit()
                print(f"User {user_id}: ✅ Updated balance to {new_balance}")
            
            cur.close()
            conn.close()
            
        except Exception as e:
            print(f"User {user_id}: ❌ Error - {e}")
    
    # Create multiple threads to simulate concurrent users
    threads = []
    for i in range(3):
        t = threading.Thread(target=update_cashbox, args=(i+1, i*0.1))
        threads.append(t)
        t.start()
    
    # Wait for all threads to complete
    for t in threads:
        t.join()
    
    print("=== Concurrent Update Test Complete ===\n")

def test_connection_pooling():
    """Test connection pool behavior"""
    print("\n=== Testing Connection Pooling ===")
    
    connections = []
    try:
        # Create multiple connections
        for i in range(15):
            conn = get_connection()
            connections.append(conn)
            print(f"Created connection {i+1}")
        
        print(f"✅ Successfully created {len(connections)} connections")
        
    except Exception as e:
        print(f"❌ Connection pool error: {e}")
    
    finally:
        # Close all connections
        for conn in connections:
            conn.close()
        print("All connections closed")
    
    print("=== Connection Pooling Test Complete ===\n")

def test_transaction_isolation():
    """Test transaction isolation levels"""
    print("\n=== Testing Transaction Isolation ===")
    
    def read_uncommitted_data(user_id):
        try:
            conn = get_connection()
            conn.set_isolation_level(psycopg2.extensions.ISOLATION_LEVEL_READ_COMMITTED)
            cur = conn.cursor()
            
            cur.execute("""
                SELECT currentbalance 
                FROM "CashBoxes" 
                WHERE cashboxid = 1
            """)
            balance = cur.fetchone()[0]
            print(f"User {user_id} sees balance: {balance}")
            
            cur.close()
            conn.close()
            
        except Exception as e:
            print(f"User {user_id}: Error - {e}")
    
    def modify_and_rollback():
        try:
            conn = get_connection()
            cur = conn.cursor()
            
            # Start transaction
            cur.execute("BEGIN")
            
            # Update balance
            cur.execute("""
                UPDATE "CashBoxes" 
                SET currentbalance = currentbalance + 1000 
                WHERE cashboxid = 1
            """)
            print("Modified balance (uncommitted)")
            
            # Give other thread time to read
            time.sleep(1)
            
            # Rollback
            conn.rollback()
            print("Rolled back transaction")
            
            cur.close()
            conn.close()
            
        except Exception as e:
            print(f"Modifier: Error - {e}")
    
    # Create threads
    t1 = threading.Thread(target=modify_and_rollback)
    t2 = threading.Thread(target=read_uncommitted_data, args=(1,))
    
    t1.start()
    time.sleep(0.5)  # Ensure t1 starts first
    t2.start()
    
    t1.join()
    t2.join()
    
    print("=== Transaction Isolation Test Complete ===\n")

def test_session_tracking():
    """Test session management"""
    print("\n=== Testing Session Tracking ===")
    
    try:
        conn = get_connection()
        cur = conn.cursor()
        
        # Check active sessions count
        cur.execute("""
            SELECT COUNT(*) 
            FROM pg_stat_activity 
            WHERE datname = 'graceway_accounting'
        """)
        active_sessions = cur.fetchone()[0]
        print(f"Active database sessions: {active_sessions}")
        
        # Check connection pool stats
        cur.execute("""
            SELECT 
                state, 
                COUNT(*) 
            FROM pg_stat_activity 
            WHERE datname = 'graceway_accounting'
            GROUP BY state
        """)
        
        print("\nConnection states:")
        for state, count in cur.fetchall():
            print(f"  {state or 'idle'}: {count}")
        
        cur.close()
        conn.close()
        
    except Exception as e:
        print(f"Error: {e}")
    
    print("=== Session Tracking Test Complete ===\n")

def test_concurrent_transactions():
    """Test multiple concurrent transactions"""
    print("\n=== Testing Concurrent Transactions ===")
    
    def add_transaction(user_id, amount, delay=0):
        try:
            time.sleep(delay)
            conn = get_connection()
            cur = conn.cursor()
            
            # Start transaction
            cur.execute("BEGIN ISOLATION LEVEL SERIALIZABLE")
            
            # Get current balance
            cur.execute("""
                SELECT currentbalance 
                FROM "CashBoxes" 
                WHERE cashboxid = 1 AND isdeleted = false
                FOR UPDATE
            """)
            current_balance = cur.fetchone()[0]
            
            # Calculate new balance
            new_balance = current_balance + amount
            
            # Simulate processing time
            time.sleep(0.3)
            
            # Update balance
            cur.execute("""
                UPDATE "CashBoxes" 
                SET currentbalance = %s, updatedat = NOW()
                WHERE cashboxid = 1
            """, (new_balance,))
            
            # Insert transaction record
            cur.execute("""
                INSERT INTO "CashTransactions" 
                (cashboxid, type, amount, balancebefore, balanceafter, 
                 transactiondate, notes, month, year, createdat, createdby)
                VALUES (1, 0, %s, %s, %s, NOW(), %s, %s, %s, NOW(), 1)
            """, (amount, current_balance, new_balance, 
                  f'Test transaction from user {user_id}', 
                  datetime.now().month, datetime.now().year))
            
            conn.commit()
            print(f"User {user_id}: ✅ Added {amount}, new balance: {new_balance}")
            
            cur.close()
            conn.close()
            
        except Exception as e:
            print(f"User {user_id}: ❌ Error - {e}")
    
    # Create multiple threads
    threads = []
    amounts = [100, 200, 150, 50, 300]
    
    for i, amount in enumerate(amounts):
        t = threading.Thread(target=add_transaction, args=(i+1, amount, i*0.1))
        threads.append(t)
        t.start()
    
    # Wait for all threads
    for t in threads:
        t.join()
    
    print("=== Concurrent Transactions Test Complete ===\n")

def verify_data_integrity():
    """Verify data integrity after concurrent operations"""
    print("\n=== Verifying Data Integrity ===")
    
    try:
        conn = get_connection()
        cur = conn.cursor()
        
        # Get final balance
        cur.execute("""
            SELECT currentbalance 
            FROM "CashBoxes" 
            WHERE cashboxid = 1
        """)
        final_balance = cur.fetchone()[0]
        print(f"Final CashBox balance: {final_balance}")
        
        # Count transactions
        cur.execute("""
            SELECT COUNT(*), SUM(amount) 
            FROM "CashTransactions" 
            WHERE cashboxid = 1 AND createdat > NOW() - INTERVAL '1 hour'
        """)
        count, total = cur.fetchone()
        print(f"Recent transactions: {count}, Total amount: {total or 0}")
        
        # Check for balance consistency
        cur.execute("""
            SELECT 
                balancebefore, 
                amount, 
                balanceafter 
            FROM "CashTransactions" 
            WHERE cashboxid = 1 
            ORDER BY transactionid DESC 
            LIMIT 10
        """)
        
        print("\nLast 10 transactions:")
        inconsistent = False
        for before, amount, after in cur.fetchall():
            expected = before + amount
            status = "✅" if abs(expected - after) < 0.01 else "❌"
            if status == "❌":
                inconsistent = True
            print(f"  {status} Before: {before}, Amount: {amount}, After: {after} (Expected: {expected})")
        
        if not inconsistent:
            print("\n✅ All transactions are consistent!")
        else:
            print("\n❌ Found inconsistent transactions!")
        
        cur.close()
        conn.close()
        
    except Exception as e:
        print(f"Error: {e}")
    
    print("=== Data Integrity Check Complete ===\n")

def main():
    """Run all tests"""
    print("\n" + "="*60)
    print("MULTI-USER TESTING SUITE")
    print("="*60)
    
    tests = [
        ("Connection Pooling", test_connection_pooling),
        ("Session Tracking", test_session_tracking),
        ("Concurrent Updates", test_concurrent_updates),
        ("Transaction Isolation", test_transaction_isolation),
        ("Concurrent Transactions", test_concurrent_transactions),
        ("Data Integrity", verify_data_integrity),
    ]
    
    for name, test_func in tests:
        try:
            print(f"\nRunning: {name}")
            test_func()
            time.sleep(1)
        except Exception as e:
            print(f"❌ Test '{name}' failed: {e}")
    
    print("\n" + "="*60)
    print("ALL TESTS COMPLETE")
    print("="*60 + "\n")

if __name__ == "__main__":
    main()
