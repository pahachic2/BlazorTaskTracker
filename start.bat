@echo off
echo Запуск Blazor Task Tracker...
echo.
echo Приложение будет доступно по адресам:
echo HTTP:  http://localhost:5000
echo HTTPS: https://localhost:5001
echo.
echo Нажмите Ctrl+C для остановки приложения
echo.
dotnet run --urls "http://localhost:5000;https://localhost:5001" 