$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
    dotnet pack TradeMath -c Release -o packages
    if($LASTEXITCODE -ne 0) {throw 'Не удалось собрать NuGet-пакет'}
    dotnet restore TradeLabs.sln --no-cache
    if($LASTEXITCODE -ne 0) {throw 'Не удалось восстановить зависимости'}
    dotnet build TradeLabs.sln -c Debug --no-restore
    if($LASTEXITCODE -ne 0) {throw 'Не удалось собрать решение'}
    dotnet run --project PackageDemo --no-build
    if($LASTEXITCODE -ne 0) {throw 'Проверки пакета не пройдены'}
} finally {Pop-Location}
