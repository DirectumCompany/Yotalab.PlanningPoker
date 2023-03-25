param([string]$targetName = "planningpoker.tar")

# Очистим решение и опубликуем приложения.
dotnet clean .\Yotalab.PlanningPoker.sln
dotnet publish --configuration Release .\Yotalab.PlanningPoker.Api\Yotalab.PlanningPoker.Api.csproj -p:PublishProfile=FolderProfile
dotnet publish --configuration Release .\Yotalab.PlanningPoker.BlazorServerSide\Yotalab.PlanningPoker.BlazorServerSide.csproj -p:PublishProfile=FolderProfile

$targetDir = ".\dist"
$targetPath = [System.IO.Path]::Join($targetDir, $targetName)

# Удалим старый архив с дистрибутивом, если он есть.
if (Test-Path $targetPath)
{
    Remove-Item $targetPath -Verbose
}

# Создание архива с дистрибутивом.
tar -cf $targetPath --directory $targetDir --exclude obj api web identity storages