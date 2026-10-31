-- Фаза 3. CONTRACT
-- ВНИМАНИЕ: выполнять только после полного перехода всех экземпляров приложения
-- на чтение/запись Buildings и успешной проверки резервной копии.
-- Скрипт намеренно останавливается, если остались общежития без BuildingId.

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM "Dormitory" WHERE "BuildingId" IS NULL) THEN
        RAISE EXCEPTION 'Contract отменён: есть Dormitory без BuildingId';
    END IF;
END $$;

BEGIN;

ALTER TABLE "Dormitory"
    ALTER COLUMN "BuildingId" SET NOT NULL;

ALTER TABLE "Dormitory"
    DROP COLUMN IF EXISTS "Address";

COMMIT;
