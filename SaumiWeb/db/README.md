# Минимальная БД для входа в SAUMI 4.8.5

## Учётка (как просили)

| | |
|--|--|
| user | `SM` |
| password | `smpwd` |
| host | `localhost` |
| port | `1521` |
| service | `FREEPDB1` |

SYS в контейнере тоже: `smpwd` (пароль образа gvenzl `ORACLE_PASSWORD`).

## Поднять

```bash
# Docker Desktop должен быть запущен
cd Astra1860/SaumiWeb/db
docker compose up -d
./import_schema.sh
```

## Данные справочников

Положить SQL Inserts **без** «Сжать файл» сюда:

`data/saumi485_dicts_inserts.sql`

Сейчас там выгрузка `20260904 -485-data (1).sql` (справочники + часть master-данных).

Импорт: `./import_schema.sh`  
(DDL переводится BYTE→CHAR под AL32UTF8 образа gvenzl; учётка SM/smpwd.)

## Для входа в программу

| | |
|--|--|
| user | `SM` |
| password | `smpwd` |
| host:port | `localhost:1521` |
| service | `FREEPDB1` |

```
FREEPDB1 =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA = (SERVICE_NAME = FREEPDB1))
  )
```

