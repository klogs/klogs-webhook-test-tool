@echo off
SET VERSION=1.0
SET OUTPUT_DIR=./publish

echo Cleaning previous builds...
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

echo Building for Windows x64...
dotnet publish -c Release -f net8.0 --sc -r win-x64 -o %OUTPUT_DIR%/webhook_%VERSION%_win-x64

echo Building for Linux x64...
dotnet publish -c Release -f net8.0 --sc -r linux-x64 -o %OUTPUT_DIR%/webhook_%VERSION%_linux-x64

echo Building for macOS x64...
dotnet publish -c Release -f net8.0 --sc -r osx-x64 -o %OUTPUT_DIR%/webhook_%VERSION%_osx-x64

echo Building for macOS ARM64...
dotnet publish -c Release -f net8.0 --sc -r osx-arm64 -o %OUTPUT_DIR%/webhook_%VERSION%_osx-arm64

echo.
echo Build complete! Check the %OUTPUT_DIR% folder.
pause