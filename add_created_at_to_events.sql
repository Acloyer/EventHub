-- Добавляем колонку CreatedAt в таблицу Events
ALTER TABLE "Events" ADD COLUMN "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();

-- Обновляем существующие записи, устанавливая CreatedAt равным текущему времени
UPDATE "Events" SET "CreatedAt" = NOW() WHERE "CreatedAt" IS NULL; 