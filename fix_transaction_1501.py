import psycopg2
import sys
import codecs

# Fix encoding
if sys.platform == 'win32':
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
    print("  تصليح المعاملة رقم 1501 - تحويلها من مصروف إلى إيراد")
    print("=" * 80)
    
    # Get current data
    c.execute("""
        SELECT transactionid, amount, transactiontype, description, 
               balancebefore, balanceafter, cashboxid
        FROM cashtransactions 
        WHERE vouchernumber = '1501'
    """)
    
    result = c.fetchone()
    
    if not result:
        print("\n❌ المعاملة رقم 1501 غير موجودة!")
    else:
        trans_id, amount, trans_type, desc, balance_before, balance_after, cashbox_id = result
        
        print(f"\n📋 البيانات الحالية:")
        print(f"   ID: {trans_id}")
        print(f"   المبلغ: {amount}")
        print(f"   النوع الحالي: {'إيراد' if trans_type == 0 else 'مصروف'}")
        print(f"   الوصف: {desc}")
        print(f"   الرصيد قبل: {balance_before}")
        print(f"   الرصيد بعد: {balance_after}")
        
        if trans_type == 0:
            print("\n✅ المعاملة بالفعل إيراد - لا يوجد تعديل مطلوب!")
        else:
            print(f"\n⚠️  المعاملة حالياً مصروف - سيتم تحويلها لإيراد")
            
            # تأكيد
            print("\n" + "=" * 80)
            print("  هل أنت متأكد من التصليح؟")
            print("  سيتم:")
            print("  1. تغيير النوع من مصروف إلى إيراد")
            print("  2. إعادة حساب الرصيد")
            print("  3. تحديث رصيد الخزنة")
            print("=" * 80)
            
            confirm = input("\n  اكتب 'نعم' للتأكيد: ")
            
            if confirm.strip() == 'نعم':
                # Get cashbox current balance
                c.execute('SELECT currentbalance FROM cashboxes WHERE cashboxid = %s', (cashbox_id,))
                current_cashbox_balance = c.fetchone()[0]
                
                print(f"\n🔧 جاري التصليح...")
                print(f"   الرصيد الحالي للخزنة: {current_cashbox_balance}")
                
                # عكس تأثير المصروف: الرصيد الحالي + المبلغ (لأننا خصمنا قبل كده)
                reversed_balance = current_cashbox_balance + amount
                print(f"   بعد عكس المصروف: {reversed_balance}")
                
                # تطبيق الإيراد: الرصيد + المبلغ
                new_cashbox_balance = reversed_balance + amount
                print(f"   بعد إضافة الإيراد: {new_cashbox_balance}")
                
                # حساب الأرصدة الجديدة للمعاملة
                new_balance_before = balance_before
                new_balance_after = balance_before + amount
                
                # Update transaction type
                c.execute("""
                    UPDATE cashtransactions 
                    SET transactiontype = 0,
                        balancebefore = %s,
                        balanceafter = %s,
                        updatedat = CURRENT_TIMESTAMP
                    WHERE transactionid = %s
                """, (new_balance_before, new_balance_after, trans_id))
                
                # Update cashbox balance
                c.execute("""
                    UPDATE cashboxes 
                    SET currentbalance = %s,
                        updatedat = CURRENT_TIMESTAMP
                    WHERE cashboxid = %s
                """, (new_cashbox_balance, cashbox_id))
                
                conn.commit()
                
                print(f"\n✅ تم التصليح بنجاح!")
                print(f"   النوع الجديد: إيراد")
                print(f"   الرصيد الجديد للخزنة: {new_cashbox_balance}")
                
            else:
                print("\n❌ تم الإلغاء")
    
    conn.close()
    
except Exception as e:
    print(f"\n❌ خطأ: {e}")
    import traceback
    traceback.print_exc()
    if 'conn' in locals():
        conn.rollback()
