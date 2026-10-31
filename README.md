# homework-5 — CAMPUS-108

Cопровождение модуля «Общежития (Заселение)» ИС «Умный кампус».

## Требования

- .NET SDK 8
- PostgreSQL 14 или новее
- Git

## Запуск API

1. Создайте базу PostgreSQL, например `smart_campus`.
2. Установите строку подключения через переменную окружения `ConnectionStrings__CampusDb`.
3. Выполните SQL-скрипт `sql/01_expand.sql` на исходной базе (перед этим проверьте резервную копию).
4. Запустите API:

```bash
cd src/CampusDormitory.Api
dotnet restore
dotnet run
```

Основные маршруты:
- `GET /api/dormitories`
- `POST /api/dormitories`
- `DELETE /api/dormitories/{id}`

Пример тела POST:

```json
{
  "name": "Общежитие № 1",
  "address": "Санкт-Петербург, улица Примерная, дом 10"
}
```

## Миграция

Скрипты выполняются по порядку: `01_expand.sql`, затем `02_migrate.sql`. `03_contract.sql` выполнять только после того, как все экземпляры приложения переведены на новую схему и старое поле больше не используется. Не запускайте Contract одновременно с версией приложения, которая читает или записывает `Dormitory.Address`.

## Тест

Для безопасного тестирования укажите url адрес тестового стенда:

```bash
SANITY_BASE_URL=http://localhost:5080 dotnet test tests/CampusDormitory.Tests/CampusDormitory.Tests.csproj
```

Тест создаёт и удаляет временную запись.

## Утилита обновления

Пример:

```bash
dotnet run --project updater/CampusUpdater.csproj -- backup ./app ./backup
dotnet run --project updater/CampusUpdater.csproj -- install ./release ./app ./backup
dotnet run --project updater/CampusUpdater.csproj -- rollback ./backup ./app
```
