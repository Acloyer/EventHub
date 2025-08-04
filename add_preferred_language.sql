-- Добавляем поле PreferredLanguage в таблицу Users
-- Выполните этот скрипт в pgAdmin или через psql

-- 1. Добавляем колонку PreferredLanguage в таблицу Users
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'AspNetUsers' 
        AND column_name = 'PreferredLanguage'
    ) THEN
        ALTER TABLE "AspNetUsers" ADD COLUMN "PreferredLanguage" TEXT NOT NULL DEFAULT 'en';
        RAISE NOTICE 'Колонка PreferredLanguage добавлена в таблицу AspNetUsers';
    ELSE
        RAISE NOTICE 'Колонка PreferredLanguage уже существует в таблице AspNetUsers';
    END IF;
END $$;

-- 2. Проверяем результат
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' 
AND column_name = 'PreferredLanguage'; 