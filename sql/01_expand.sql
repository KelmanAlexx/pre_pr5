-- Фаза 1. EXPAND
-- Добавляем новую таблицу и nullable-ссылку.
-- Старое поле Address пока сохраняется для совместимости со старой версией API.

BEGIN;

CREATE TABLE IF NOT EXISTS "Buildings"
(
    "Id" BIGSERIAL PRIMARY KEY,
    "Address" VARCHAR(300) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE "Dormitory"
    ADD COLUMN IF NOT EXISTS "BuildingId" BIGINT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_Dormitory_Buildings_BuildingId'
    ) THEN
        ALTER TABLE "Dormitory"
            ADD CONSTRAINT "FK_Dormitory_Buildings_BuildingId"
            FOREIGN KEY ("BuildingId") REFERENCES "Buildings"("Id");
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_Dormitory_BuildingId"
    ON "Dormitory" ("BuildingId");

COMMIT;
