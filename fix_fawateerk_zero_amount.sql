-- ============================================================
-- FIX FAWATEERK PAYMENTS - ZERO AMOUNT ISSUE
-- ============================================================
-- هذا الملف يحل مشكلة الدفعات التي قد يكون مبلغها صفر
-- ============================================================

-- 1. التحقق من وجود دفعات بمبلغ صفر أو سالب
SELECT 
    "Id",
    "Amount",
    "TransferDate",
    "ReferenceNumber",
    "Notes",
    "DestinationBankAccountId"
FROM "BankTransfers"
WHERE "TransferType" = 'FawateerkPayment'
  AND "Amount" <= 0
ORDER BY "TransferDate" DESC;

-- 2. إصلاح الدفعات ذات المبلغ صفر (تحديثها إلى 0.01 كحد أدنى)
-- تحذير: قم بمراجعة هذه الدفعات قبل التحديث
UPDATE "BankTransfers"
SET "Amount" = 0.01
WHERE "TransferType" = 'FawateerkPayment'
  AND "Amount" = 0;

-- 3. حذف الدفعات ذات المبلغ السالب (إذا وُجدت)
-- تحذير: تأكد أن هذه دفعات خاطئة قبل الحذف
-- DELETE FROM "BankTransfers"
-- WHERE "TransferType" = 'FawateerkPayment'
--   AND "Amount" < 0;

-- 4. التحقق من النتيجة النهائية
SELECT 
    COUNT(*) as "إجمالي_دفعات_فواتيرك",
    MIN("Amount") as "أقل_مبلغ",
    MAX("Amount") as "أعلى_مبلغ",
    AVG("Amount") as "متوسط_المبلغ",
    SUM("Amount") as "المجموع_الكلي"
FROM "BankTransfers"
WHERE "TransferType" = 'FawateerkPayment';

-- ============================================================
-- ملاحظات:
-- - المشكلة كانت في واجهة التعديل حيث كان يتم تعيين Value قبل Minimum
-- - تم إصلاح الكود البرمجي
-- - هذا SQL للتأكد من عدم وجود بيانات خاطئة في قاعدة البيانات
-- ============================================================
