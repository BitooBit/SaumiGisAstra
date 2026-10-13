-- Только SELECT. Пароль сюда не писать. Запускает patch_remarks1_diag.sh.

WHENEVER SQLERROR CONTINUE
SET ECHO OFF FEEDBACK ON HEADING ON
SET PAGESIZE 50000 LINESIZE 240
SET TRIMSPOOL ON TRIMOUT ON
SET COLSEP '|'
SET NULL '(null)'
SET NUMWIDTH 14
ALTER SESSION SET NLS_DATE_FORMAT = 'YYYY-MM-DD HH24:MI:SS';
ALTER SESSION SET NLS_LANGUAGE = AMERICAN;
ALTER SESSION SET NLS_TERRITORY = AMERICA;

COLUMN name FORMAT A36
COLUMN ext FORMAT A12
COLUMN banner FORMAT A80
COLUMN db_user FORMAT A30
COLUMN db_name FORMAT A30

PROMPT === A. SESSION ===
SELECT USER AS db_user,
       SYS_CONTEXT('USERENV','DB_NAME') AS db_name,
       SYS_CONTEXT('USERENV','HOST') AS client_host,
       SYSDATE AS db_now
  FROM dual;
SELECT banner FROM v$version;

PROMPT === B. MOVETYPE MAP_COLOR (чёрные строки комбо) ===
SELECT mt.id,
       RTRIM(mt.name) AS name,
       mt.map_color,
       NVL(BITAND(mt.map_color, 255), 0) AS r,
       NVL(BITAND(TRUNC(mt.map_color / 256), 255), 0) AS g,
       NVL(BITAND(TRUNC(mt.map_color / 65536), 255), 0) AS b,
       ROUND(
         0.299 * NVL(BITAND(mt.map_color, 255), 255)
       + 0.587 * NVL(BITAND(TRUNC(mt.map_color / 256), 255), 255)
       + 0.114 * NVL(BITAND(TRUNC(mt.map_color / 65536), 255), 255)
       , 1) AS lum,
       CASE
         WHEN mt.map_color IS NULL THEN 'NULL_WHITE'
         WHEN mt.map_color = 0 THEN 'BLACK0'
         WHEN (
           0.299 * BITAND(mt.map_color, 255)
         + 0.587 * BITAND(TRUNC(mt.map_color / 256), 255)
         + 0.114 * BITAND(TRUNC(mt.map_color / 65536), 255)
         ) < 140 THEN 'DARK'
         ELSE 'LIGHT'
       END AS class,
       CASE
         WHEN mt.map_color IS NULL THEN NULL
         WHEN mt.map_color = 0 THEN NULL
         ELSE (
           LEAST(255, TRUNC(BITAND(mt.map_color, 255) * 0.4 + 255 * 0.6))
         + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 256), 255) * 0.4 + 255 * 0.6)) * 256
         + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 65536), 255) * 0.4 + 255 * 0.6)) * 65536
         )
       END AS proposed_color
  FROM sm.movetype mt
 ORDER BY RTRIM(mt.name);

PROMPT === C. COLORS dict ===
SELECT id, name FROM sm.colors ORDER BY id;

PROMPT === D. DOCUMENTS вложения: расширения (иконка/просмотр) ===
SELECT NVL(UPPER(TRIM(d.file_extention)), '(null)') AS ext,
       COUNT(*) AS cnt,
       COUNT(d.body) AS with_body
  FROM sm.documents d
 GROUP BY NVL(UPPER(TRIM(d.file_extention)), '(null)')
 ORDER BY COUNT(*) DESC;

PROMPT === E. SAMPLE документов с телом (без BLOB) ===
SELECT * FROM (
  SELECT d.id,
         RTRIM(d.docno) AS docno,
         d.docdate,
         UPPER(TRIM(d.file_extention)) AS ext,
         dbms_lob.getlength(d.body) AS body_len
    FROM sm.documents d
   WHERE d.body IS NOT NULL
   ORDER BY d.id DESC
) WHERE ROWNUM <= 20;

PROMPT === F. EZ/скан: FILE_SCAN непустой ===
SELECT COUNT(*) AS docs_total,
       SUM(CASE WHEN file_scan IS NOT NULL AND TRIM(file_scan) IS NOT NULL THEN 1 ELSE 0 END) AS with_file_scan
  FROM sm.documents;

PROMPT === G. DONE ===
EXIT;
