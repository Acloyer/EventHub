-- Полное исправление базы данных EventHub
-- Выполните этот скрипт в pgAdmin или через psql

-- 1. Добавляем колонку CreatedAt в таблицу Events
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

-- 2. Добавляем колонку CreatedAt в таблицу EventComments
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
        RAISE NOTICE 'Колонка CreatedAt уже существует в таблицу EventComments';
    END IF;
END $$;

-- 3. Добавляем колонку Message в таблицу Notifications
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Notifications' 
        AND column_name = 'Message'
    ) THEN
        ALTER TABLE "Notifications" ADD COLUMN "Message" TEXT NOT NULL DEFAULT '';
        RAISE NOTICE 'Колонка Message добавлена в таблицу Notifications';
    ELSE
        RAISE NOTICE 'Колонка Message уже существует в таблице Notifications';
    END IF;
END $$;

-- 4. Добавляем колонку MuteUntil в таблицу UserMuteSystems (если таблица существует)
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.tables 
        WHERE table_name = 'UserMuteSystems'
    ) THEN
        IF NOT EXISTS (
            SELECT 1 
            FROM information_schema.columns 
            WHERE table_name = 'UserMuteSystems' 
            AND column_name = 'MuteUntil'
        ) THEN
            ALTER TABLE "UserMuteSystems" ADD COLUMN "MuteUntil" TIMESTAMP WITH TIME ZONE NULL;
            RAISE NOTICE 'Колонка MuteUntil добавлена в таблицу UserMuteSystems';
        ELSE
            RAISE NOTICE 'Колонка MuteUntil уже существует в таблице UserMuteSystems';
        END IF;
    ELSE
        RAISE NOTICE 'Таблица UserMuteSystems не существует, пропускаем';
    END IF;
END $$;

-- 5. Добавляем колонку MuteUntil в таблицу UserMuteEntries (если таблица существует)
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.tables 
        WHERE table_name = 'UserMuteEntries'
    ) THEN
        IF NOT EXISTS (
            SELECT 1 
            FROM information_schema.columns 
            WHERE table_name = 'UserMuteEntries' 
            AND column_name = 'MuteUntil'
        ) THEN
            ALTER TABLE "UserMuteEntries" ADD COLUMN "MuteUntil" TIMESTAMP WITH TIME ZONE NULL;
            RAISE NOTICE 'Колонка MuteUntil добавлена в таблицу UserMuteEntries';
        ELSE
            RAISE NOTICE 'Колонка MuteUntil уже существует в таблице UserMuteEntries';
        END IF;
    ELSE
        RAISE NOTICE 'Таблица UserMuteEntries не существует, пропускаем';
    END IF;
END $$;

-- 6. Обновляем существующие записи в Events
UPDATE "Events" SET "CreatedAt" = NOW() WHERE "CreatedAt" IS NULL;

-- 7. Обновляем существующие записи в EventComments
UPDATE "EventComments" SET "CreatedAt" = NOW() WHERE "CreatedAt" IS NULL;

-- 8. Обновляем существующие записи в Notifications
UPDATE "Notifications" SET "Message" = 'New notification' WHERE "Message" IS NULL OR "Message" = '';

-- 9. Проверяем результат
SELECT 'Events' as table_name, 
       COUNT(*) as total_records, 
       COUNT("CreatedAt") as records_with_created_at
FROM "Events"
UNION ALL
SELECT 'EventComments' as table_name, 
       COUNT(*) as total_records, 
       COUNT("CreatedAt") as records_with_created_at
FROM "EventComments"
UNION ALL
SELECT 'Notifications' as table_name, 
       COUNT(*) as total_records, 
       COUNT("Message") as records_with_message
FROM "Notifications";

-- 10. Показываем структуру таблиц
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name IN ('Events', 'EventComments', 'Notifications')
ORDER BY table_name, ordinal_position; 