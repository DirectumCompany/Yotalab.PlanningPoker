# Yotalab.PlanningPoker
Инструмент agile-покер планирования

# Стек технологий
Сервер - Orleans https://github.com/dotnet/orleans

Веб-клиент - Blazor Server Side https://docs.microsoft.com/ru-ru/aspnet/core/blazor/hosting-models?view=aspnetcore-6.0#blazor-server

UI Framework - MudBlazor https://github.com/MudBlazor/MudBlazor

# База данных по умолчанию
https://mariadb.org/download/ - строка подключения в \Yotalab.PlanningPoker.BlazorServerSide\appsettings.Development.json

Команда для запуска в докере:

```docker run --detach --name mariadb-local --restart=always -p 3306:3306 --env MARIADB_ROOT_PASSWORD=[password] mariadb:latest```

# Скрипты инициализации хранилища Orleans
\Yotalab.PlanningPoker.Api\OrleansSql\MySQL-Main.sql - основные таблицы Orleans, выполнить в первую очередь

\Yotalab.PlanningPoker.Api\OrleansSql\MySQL-Persistence.sql - хранилище состояния Grain

\Yotalab.PlanningPoker.Api\OrleansSql\MySQL-Clustering.sql - хранилище для поддержки кластеризации, требуется только если Silo запускается отдельным одним или более экземпляров процессов.
