-- فحص كل الأعمدة في جدول umrahpilgrims
SELECT column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'umrahpilgrims' 
ORDER BY ordinal_position;
