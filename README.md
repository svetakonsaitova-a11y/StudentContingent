# Модуль управления контингентом студентов

Веб-API на ASP.NET Core (.NET 10) + Entity Framework Core + SQL Server LocalDB.
Решение разбито на слои по принципам DDD:

| Проект | Назначение | Зависит от |
| --- | --- | --- |
| `StudentContingent.Domain` | Классы доменной модели и правила (переходы статуса в `Student.Apply`) | — |
| `StudentContingent.Infrastructure` | `Data/Context.cs` (EF Core), `Repository/*` (репозитории), миграции | Domain |
| `StudentContingent.API` | Контроллеры, `Program.cs`, строка подключения | Domain, Infrastructure |
| `StudentContingent.Tests` | Тесты xUnit: правила домена и репозитории на тестовой БД | Domain, Infrastructure |
| `StudentContingent.Client` | Веб-интерфейс на Blazor WebAssembly, обращается к API | Domain |

## Что нужно

- Visual Studio 2022 с рабочей нагрузкой «ASP.NET и веб-разработка» (в неё входит .NET 10 SDK и SQL Server Express LocalDB).

## Первый запуск

1. Откройте `StudentContingent.sln`. Пакеты NuGet восстановятся сами (или: ПКМ по решению → «Восстановить пакеты NuGet»).
2. Сборка → «Собрать решение» (F6).
3. Создайте базу через миграции: «Средства → Диспетчер пакетов NuGet → Консоль диспетчера пакетов».
   - Запускаемый проект (ПКМ по проекту → «Назначить запускаемым проектом»): `StudentContingent.API`.
   - В консоли «Проект по умолчанию»: `StudentContingent.Infrastructure`.
   - Выполните:
     ```
     Add-Migration InitialCreate
     Update-Database
     ```
   Папка `Migrations` появится в проекте Infrastructure, база `StudentContingent` — в LocalDB.
4. Запустите API (F5). Откроется Swagger, где можно вызывать все методы.

## Запуск веб-интерфейса

Клиент и API — два отдельных приложения, запускать нужно оба:

1. ПКМ по решению → «Настроить запускаемые проекты…» (или «Свойства» → «Общие свойства» → «Запускаемый проект»).
2. Выберите «Несколько запускаемых проектов» и поставьте «Запуск» у `StudentContingent.API` и `StudentContingent.Client`. Профиль у обоих — `https`.
3. F5. Откроются Swagger (https://localhost:7080/swagger) и интерфейс (https://localhost:7090).

Адрес API для клиента задаётся в `StudentContingent.Client/wwwroot/appsettings.json`, а разрешённые адреса клиента для CORS — в `StudentContingent.API/appsettings.json` (раздел `Cors:Origins`).

## Тесты

«Тест → Обозреватель тестов → Запустить все».

- `TestStudentDomain` — правила переходов статуса, база не нужна.
- `TestStudentRepository` — создаёт отдельную базу `StudentContingentTest` в LocalDB и пересоздаёт её перед каждым тестом.

## Основные адреса API

| Метод | Адрес | Что делает |
| --- | --- | --- |
| GET | `/api/students?lastName=&studyGroupId=&status=` | Поиск и фильтрация |
| GET | `/api/students/{id}` | Карточка студента |
| GET | `/api/students/{id}/history` | История движения |
| GET | `/api/students/summary` | Сводка по контингенту |
| POST | `/api/students` | Новая карточка (статус «не зачислен») |
| PUT / DELETE | `/api/students/{id}` | Изменение / удаление карточки |
| POST | `/api/students/{id}/movements` | Событие движения по приказу |
| POST | `/api/orders` | Регистрация приказа |
| POST | `/api/orders/{id}/movements` | Проведение приказа для нескольких студентов (всё или ничего) |
| GET | `/api/groups/{id}/students` | Список группы |
| CRUD | `/api/departments`, `/api/programs`, `/api/benefits`, `/api/groups` | Справочники |

Порядок для проверки в Swagger: подразделение → программа → группа → приказ → студент → `POST /api/students/{id}/movements` с `"eventType": "Enrollment"` и `toGroupId`.

## Если нужна другая версия .NET

Во всех четырёх `.csproj` замените `net10.0` на `net8.0` (или `net6.0`), а версии пакетов `Microsoft.EntityFrameworkCore*` с `10.0.*` на `8.0.*` (или `6.0.*`). Нужна установленная среда выполнения этой версии.
