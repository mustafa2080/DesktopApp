-- Script لإصلاح مشكلة RoomType في جدول umrahpilgrims

-- 1. التحقق من اسم العمود الحالي
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'umrahpilgrims' 
        AND column_name = 'RoomType'
    ) THEN
        -- 2. إعادة تسمية العمود من RoomType إلى roomtype
        RAISE NOTICE 'تم العثور على العمود RoomType، سيتم تغيير الاسم إلى roomtype...';
        EXECUTE 'ALTER TABLE umrahpilgrims RENAME COLUMN "RoomType" TO roomtype';
        RAISE NOTICE 'تم تغيير اسم العمود بنجاح!';
    ELSE
        RAISE NOTICE 'العمود RoomType غير موجود أو تم تصحيحه بالفعل';
    END IF;
END $$;

-- 3. التحقق من النتيجة
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'umrahpilgrims' 
AND column_name LIKE '%room%'
ORDER BY column_name;
