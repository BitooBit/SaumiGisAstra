-- =============================================================================
-- SAUMI 4.8.5 — выгрузка справочников (по замерам с объекта 04.09.2026)
-- Все таблицы маленькие: max CLS_OKOF=11042, LEASEPURPOSE=1691, PROPNAMES=3375
-- Итого ~17k строк — можно INSERT или expdp DATA_ONLY
-- =============================================================================
-- Запускать от SM. Результат положить в:
--   Astra1860/SaumiWeb/db/data/
-- =============================================================================

-- Быстрый вариант: один скрипт INSERT для всех справочников (PL/SQL Developer)
-- Меню: Tools → Export Tables → выбрать таблицы ниже → SQL Inserts
-- Или выполнить блок ниже в SQL*Plus / SQL Window со spool.

/*
Таблицы и num_rows (prod):
  ACTIVITYTYPES     0
  ATO               1
  KFXNAMES          1
  MONUMENTTYPES     8
  OPERTYPE         11
  BUILDTYPES       32
  MOVETYPE         41
  SECTIONS         44
  BUILDMATERIALS   47
  ORGFORMS         49
  DICTIONARIES    110
  REPORTSTREE     112
  PROVINCEAREA    147
  KFXVALUES       404
  DOCTYPES        417
  LEASEPURPOSE   1691
  PROPNAMES      3375
  CLS_OKOF      11042
  CLIENTTYPES   — не было в результате (пусто/нет в схеме или другое имя)
  CURRENCYUNIT  — не было в результате
*/

-- --- Рекомендуемый путь: Data Pump (один файл) ---
/*
CREATE OR REPLACE DIRECTORY SAUMI_EXP AS 'C:\Temp\saumi_exp';  -- путь на сервере БД
GRANT READ, WRITE ON DIRECTORY SAUMI_EXP TO SM;

expdp SM/<password>@<tns> \
  DIRECTORY=SAUMI_EXP \
  DUMPFILE=saumi485_dicts.dmp \
  LOGFILE=saumi485_dicts_exp.log \
  TABLES=SM.ACTIVITYTYPES,SM.ATO,SM.KFXNAMES,SM.MONUMENTTYPES,SM.OPERTYPE,\
SM.BUILDTYPES,SM.MOVETYPE,SM.SECTIONS,SM.BUILDMATERIALS,SM.ORGFORMS,\
SM.DICTIONARIES,SM.REPORTSTREE,SM.PROVINCEAREA,SM.KFXVALUES,SM.DOCTYPES,\
SM.LEASEPURPOSE,SM.PROPNAMES,SM.CLS_OKOF \
  CONTENT=DATA_ONLY
*/

-- --- Альтернатива в PL/SQL Developer ---
-- 1. Правой кнопкой на таблицу / Tools → Export User Objects / Export Tables
-- 2. Отметить 18 таблиц из списка выше
-- 3. Format: Insert statements (или SQL)
-- 4. Сохранить как saumi485_dicts_inserts.sql и отдать сюда
