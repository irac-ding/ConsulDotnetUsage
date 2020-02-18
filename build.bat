cd /d %~dp0
dotnet restore
dotnet build --no-restore
exit