$ErrorActionPreference='Stop'
if([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {throw 'Задайте NUGET_API_KEY локально. Ключ не следует отправлять в чат.'}
$package=Join-Path $PSScriptRoot 'packages\Didyk.Gleb.TradeMath.20261125.1.0.0.nupkg'
dotnet nuget push $package --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
if($LASTEXITCODE -ne 0) {throw 'Публикация пакета не выполнена'}
