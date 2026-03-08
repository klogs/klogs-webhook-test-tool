@echo off
SET VERSION=1.0
SET OUTPUT_DIR=./publish

echo Cleaning previous builds...
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

echo Building for Current Runtime...
dotnet publish -c Release -f net8.0 --sc --use-current-runtime -o %OUTPUT_DIR%/webhook_%VERSION%

echo.
echo Build complete! Check the %OUTPUT_DIR% folder.
pause