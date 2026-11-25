# Учебные проекты ИСРПО

Студент: Дидык Глеб Николаевич, 23290907/1094.
Участники работы с Git: Дидык Глеб Николаевич и Максимов Виктор Кириллович.

ЛР4: отдельные ветви, коммиты, слияние и разрешение конфликта.
ЛР5: ComplexNumbers (CMake), FaceDetection (OpenCV), TradeMath (NuGet) и PackageDemo.
ЛР6: TradeApp — регистрация и авторизация, SQL Server и Entity Framework, расчёт продажи и отладка.

Сборка .NET: `dotnet build TradeLabs.sln`.
Сервер по умолчанию: `.\SQLEXPRESS`, база: `DidykTradeLabs2026`.
Для другого сервера задайте переменную окружения `TRADELAB_CONNECTION`.
Пароль и ответ на секретный вопрос сохраняются только в виде PBKDF2-хешей с отдельной случайной солью.

Пакет собирается командой `dotnet pack TradeMath -c Release -o packages`.
PackageDemo подключает пакет из локального источника `packages` и запускает проверки.
Для публикации на nuget.org требуется API-ключ владельца аккаунта.

OpenCV 4.11.0 собирается из исходников, минимальный набор модулей: core, imgproc, imgcodecs, objdetect, highgui.
Передайте `OpenCV_DIR` в CMake проекта FaceDetection.

