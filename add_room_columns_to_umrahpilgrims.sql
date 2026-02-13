-- إضافة عمودي RoomType و SharedRoomNumber إلى جدول umrahpilgrims

-- 1. إضافة عمود roomtype (إذا لم يكن موجوداً)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'umrahpilgrims' 
        AND column_name = 'roomtype'
    ) THEN
        ALTER TABLE umrahpilgrims ADD COLUMN roomtype integer NULL;
        RAISE NOTICE 'تم إضافة عمود roomtype بنجاح!';
    ELSE
        RAISE NOTICE 'عمود roomtype موجود بالفعل';
    END IF;
END $$;

-- 2. إضافة عمود sharedroomnumber (إذا لم يكن موجوداً)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'umrahpilgrims' 
        AND column_name = 'sharedroomnumber'
    ) THEN
        ALTER TABLE umrahpilgrims ADD COLUMN sharedroomnumber character varying(20) NULL;
        RAISE NOTICE 'تم إضافة عمود sharedroomnumber بنجاح!';
    ELSE
        RAISE NOTICE 'عمود sharedroomnumber موجود بالفعل';
    END IF;
END $$;

-- 3. إنشاء فهرس على sharedroomnumber (إذا لم يكن موجوداً)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE tablename = 'umrahpilgrims' 
        AND indexname = 'ix_umrahpilgrims_sharedroomnumber'
    ) THEN
        CREATE INDEX ix_umrahpilgrims_sharedroomnumber ON umrahpilgrims(sharedroomnumber);
        RAISE NOTICE 'تم إنشاء الفهرس بنجاح!';
    ELSE
        RAISE NOTICE 'الفهرس موجود بالفعل';
    END IF;
END $$;

-- 4. التحقق من النتيجة
SELECT column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'umrahpilgrims' 
AND (column_name = 'roomtype' OR column_name = 'sharedroomnumber')
ORDER BY column_name;
