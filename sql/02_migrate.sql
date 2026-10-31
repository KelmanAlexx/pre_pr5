-- Фаза 2. MIGRATE
-- Создаём по одной записи Buildings на каждый уникальный адрес.
-- Затем связываем все общежития с записью здания по адресу.

BEGIN;

INSERT INTO "Buildings" ("Address")
SELECT DISTINCT BTRIM(d."Address")
FROM "Dormitory" d
WHERE d."BuildingId" IS NULL
  AND d."Address" IS NOT NULL
  AND BTRIM(d."Address") <> ''
  AND NOT EXISTS (
      SELECT 1
      FROM "Buildings" b
      WHERE b."Address" = BTRIM(d."Address")
  );

UPDATE "Dormitory" d
SET "BuildingId" = b."Id"
FROM "Buildings" b
WHERE d."BuildingId" IS NULL
  AND BTRIM(d."Address") = b."Address";

-- Контрольный запрос должен вернуть 0.
-- SELECT COUNT(*) FROM "Dormitory" WHERE "BuildingId" IS NULL;

COMMIT;
