@echo off
:: Build + Test + Pack
:: Usage: build.cmd [Release|Debug]

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

echo === Build (%CONFIG%) ===
dotnet build Notes.Avalonia.slnx -c %CONFIG%
if errorlevel 1 exit /b %errorlevel%

echo === Tests ===
dotnet test Notes.Avalonia.Tests\Notes.Avalonia.Tests.csproj -c %CONFIG% --no-build
if errorlevel 1 exit /b %errorlevel%

echo === Pack ===
dotnet pack Notes.Avalonia\Notes.Avalonia.csproj -c %CONFIG% --no-build -o artifacts\
if errorlevel 1 exit /b %errorlevel%

echo === Done ===
