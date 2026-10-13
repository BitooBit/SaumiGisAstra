# Инцидент: домен umk-a8436 (2026-10-08) — ОТКРЫТ

Хост: `umk-a8436.cityhall.krd`  
Учётные: локальный `administrator`, доменный `ochernaja@Cityhall.krd`

## Материалы

| Путь | Что |
|------|-----|
| `Admin/saumi_install.log` | установка (видны только 7 локальных) |
| `Admin/logs.tar.gz` | runtime logs administrator |
| `Admin/extracted/` | распакованные логи admin |
| `User/saumi_install.log` | тот же install-лог |
| `User/logs.tar.gz` | runtime logs доменного (в т.ч. запуск от root) |
| `User/extracted/` | распакованные логи user |
| `screens/` | скрины ошибок |

### Скрины

| Файл | Содержание |
|------|------------|
| `image-6cd37107-…jpg` | Saumi.exe Application Error |
| `image-f64de127-…jpg` | ExplorerWindowProxy TypeInitializationException |
| `image-e09aeac5-…jpg` | терминал: install --users, 7 локальных |
| `image-e1e8c0ec-…jpg` | «Отказано в доступе» на logs у доменного |

## Статус

Не закрыто. Фикс в установщике `INSTALLER_REV=2026-10-08-domain-homes-diag` (скан `/home`, `--for-user`, diag вшит). Ждём переустановку и новые логи.
