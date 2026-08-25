@echo off
:: Build + Test + Pack
:: Usage: build.cmd [Release|Debug]

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

echo === Build (%CONFIG%) ===
dotnet build Notepad.Avalonia.slnx -c %CONFIG%
if errorlevel 1 exit /b %errorlevel%

echo === Tests ===
dotnet test Notepad.Avalonia.Tests\Notepad.Avalonia.Tests.csproj -c %CONFIG% --no-build
if errorlevel 1 exit /b %errorlevel%

echo === Pack ===
dotnet pack Notepad.Avalonia\Notepad.Avalonia.csproj -c %CONFIG% --no-build -o artifacts\
if errorlevel 1 exit /b %errorlevel%

echo === Done ===
