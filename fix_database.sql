-- Исправление базы данных EventHub
-- Выполните этот скрипт в pgAdmin или через psql

-- 1. Добавляем колонку CreatedAt в таблицу Events
DO $$ 
BEGIN
    -- Проверяем, существует ли колонка CreatedAt в Events
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Events' 
        AND column_name = 'CreatedAt'
    ) THEN
        ALTER TABLE "Events" ADD COLUMN "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
        RAISE NOTICE 'Колонка CreatedAt добавлена в таблицу Events';
    ELSE
        RAISE NOTICE 'Колонка CreatedAt уже существует в таблице Events';
    END IF;
END $$;

-- 2. Добавляем колонку CreatedAt в таблицу EventComments (если её нет)
DO $$ 
BEGIN
    -- Проверяем, существует ли колонка CreatedAt в EventComments
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'EventComments' 
        AND column_name = 'CreatedAt'
    ) THEN
        ALTER TABLE "EventComments" ADD COLUMN "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
        RAISE NOTICE 'Колонка CreatedAt добавлена в таблицу EventComments';
    ELSE
        RAISE NOTICE 'Колонка CreatedAt уже существует в таблице EventComments';
    END IF;
END $$;

-- 3. Обновляем существующие записи в Events
UPDATE "Events" SET "CreatedAt" = NOW() WHERE "CreatedAt" IS NULL;

-- 4. Обновляем существующие записи в EventComments
UPDATE "EventComments" SET "CreatedAt" = NOW() WHERE "CreatedAt" IS NULL;

-- 5. Проверяем результат для Events
SELECT 'Events' as table_name, 
       COUNT(*) as total_records, 
       COUNT("CreatedAt") as records_with_created_at
FROM "Events"
UNION ALL
SELECT 'EventComments' as table_name, 
       COUNT(*) as total_records, 
       COUNT("CreatedAt") as records_with_created_at
FROM "EventComments";

-- 6. Показываем несколько примеров записей из Events
SELECT 'Events' as table_name, "Id", "Title" as name, "CreatedAt" 
FROM "Events" 
ORDER BY "CreatedAt" DESC 
LIMIT 3;

-- 7. Показываем несколько примеров записей из EventComments
SELECT 'EventComments' as table_name, "Id", "Comment" as name, "CreatedAt" 
FROM "EventComments" 
ORDER BY "CreatedAt" DESC 
LIMIT 3; 