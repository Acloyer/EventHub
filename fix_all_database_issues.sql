-- Комплексный скрипт для исправления всех проблем с базой данных
-- Выполните этот скрипт в pgAdmin

-- 1. Добавляем колонку PreferredLanguage в таблицу AspNetUsers
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

-- 2. Добавляем колонку CreatedAt в таблицу Events (если не существует)
DO $$ 
BEGIN
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

-- 3. Добавляем колонку CreatedAt в таблицу EventComments (если не существует)
DO $$ 
BEGIN
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

-- 4. Обновляем существующие записи в Events (если CreatedAt NULL)
UPDATE "Events" 
SET "CreatedAt" = NOW() 
WHERE "CreatedAt" IS NULL;

-- 5. Обновляем существующие записи в EventComments (если CreatedAt NULL)
UPDATE "EventComments" 
SET "CreatedAt" = NOW() 
WHERE "CreatedAt" IS NULL;

-- 6. Проверяем и обновляем колонку Message в Notifications
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Notifications' 
        AND column_name = 'Message'
    ) THEN
        -- Обновляем пустые сообщения
        UPDATE "Notifications" 
        SET "Message" = 'New notification' 
        WHERE "Message" IS NULL OR "Message" = '';
        RAISE NOTICE 'Обновлены пустые сообщения в Notifications';
    ELSE
        RAISE NOTICE 'Колонка Message не существует в таблице Notifications';
    END IF;
END $$;

-- 7. Проверяем результат
SELECT 
    'AspNetUsers' as table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' 
AND column_name IN ('PreferredLanguage', 'CreatedAt')

UNION ALL

SELECT 
    'Events' as table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Events' 
AND column_name = 'CreatedAt'

UNION ALL

SELECT 
    'EventComments' as table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'EventComments' 
AND column_name = 'CreatedAt'

UNION ALL

SELECT 
    'Notifications' as table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Notifications' 
AND column_name = 'Message'; 