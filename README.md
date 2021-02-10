# Yotalab.PlanningPoker
Инструмент agile-покер планирования

# Стек технологий
Сервер - Orleans https://github.com/dotnet/orleans

Веб-клиент - Blazor Server Side https://docs.microsoft.com/ru-ru/aspnet/core/blazor/hosting-models?view=aspnetcore-3.1

UI Framework - Bootstrap https://getbootstrap.com/

# База данных по умолчанию
https://mariadb.org/download/ - строка подключения в \Yotalab.PlanningPoker.BlazorServerSide\appsettings.Development.json

# Скрипты инициализации хранилища Orleans
\Yotalab.PlanningPoker.Api\OrleansSql\MySQL-Main.sql - основные таблицы Orleans, выполнить в первую очередь

\Yotalab.PlanningPoker.Api\OrleansSql\MySQL-Persistence.sql - хранилище состояния Grain

