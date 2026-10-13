-- Применить осветление SM.MOVETYPE.MAP_COLOR (lum < 140).
-- Превью уже снято remarks1_20260923_075345: 7 тёмных строк.
-- Формула: 40% исходный + 60% белый. NULL не трогаем.
-- Запускать только через patch_apply_movetype_colors.sh (stdin CONNECT).

SET PAGESIZE 80 LINESIZE 200
WHENEVER SQLERROR EXIT 1
COLUMN name FORMAT A32

PROMPT === До UPDATE: тёмные MAP_COLOR ===
SELECT mt.id, RTRIM(mt.name) AS name, mt.map_color
  FROM sm.movetype mt
 WHERE mt.map_color IS NOT NULL
   AND (
         mt.map_color = 0
      OR (
           0.299 * BITAND(mt.map_color, 255)
         + 0.587 * BITAND(TRUNC(mt.map_color / 256), 255)
         + 0.114 * BITAND(TRUNC(mt.map_color / 65536), 255)
         ) < 140
       )
 ORDER BY RTRIM(mt.name);

UPDATE sm.movetype mt
   SET mt.map_color = CASE
         WHEN mt.map_color = 0 THEN NULL
         ELSE (
           LEAST(255, TRUNC(BITAND(mt.map_color, 255) * 0.4 + 255 * 0.6))
         + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 256), 255) * 0.4 + 255 * 0.6)) * 256
         + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 65536), 255) * 0.4 + 255 * 0.6)) * 65536
         )
       END
 WHERE mt.map_color IS NOT NULL
   AND (
         mt.map_color = 0
      OR (
           0.299 * BITAND(mt.map_color, 255)
         + 0.587 * BITAND(TRUNC(mt.map_color / 256), 255)
         + 0.114 * BITAND(TRUNC(mt.map_color / 65536), 255)
         ) < 140
       );

PROMPT === После UPDATE ===
SELECT mt.id, RTRIM(mt.name) AS name, mt.map_color
  FROM sm.movetype mt
 WHERE mt.id IN (1, 2, 3, 5, 6, 7, 10)
 ORDER BY mt.id;

COMMIT;
PROMPT COMMIT done.
