-- Микропатч цветов SM.MOVETYPE.MAP_COLOR (чёрные строки в комбо «Вид движения»).
-- DLL / Wine / режим окон не трогаем. Только данные справочника.
--
-- На Windows тёмный MAP_COLOR рисуется с контрастным текстом.
-- В Wine owner-draw заливает фон цветом справочника, шрифт остаётся чёрным →
-- тёмные пункты выглядят чёрными полосами.
-- NULL уже даёт белый фон (на скрине «Арендный фонд», «Банкротство» читаются).
--
-- Запуск от SM (sqlplus SM/pass@MMPSRV @patch_movetype_colors.sql)
-- Сначала превью, потом UPDATE. COMMIT только после визуальной проверки.

SET PAGESIZE 80 LINESIZE 200
COLUMN name FORMAT A32
COLUMN map_color FORMAT 99999999999
COLUMN r FORMAT 999
COLUMN g FORMAT 999
COLUMN b FORMAT 999
COLUMN lum FORMAT 999.0
COLUMN new_color FORMAT 99999999999

PROMPT === Сейчас: MAP_COLOR и яркость (BGR Delphi TColor) ===

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
       , 1) AS lum
  FROM sm.movetype mt
 ORDER BY RTRIM(mt.name);

PROMPT === Кандидаты: lum < 140 или MAP_COLOR = 0 (чёрный). Смесь 40% исходный + 60% белый ===

SELECT mt.id,
       RTRIM(mt.name) AS name,
       mt.map_color,
       (
         LEAST(255, TRUNC(BITAND(mt.map_color, 255) * 0.4 + 255 * 0.6))
       + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 256), 255) * 0.4 + 255 * 0.6)) * 256
       + LEAST(255, TRUNC(BITAND(TRUNC(mt.map_color / 65536), 255) * 0.4 + 255 * 0.6)) * 65536
       ) AS new_color
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

PROMPT === Снять комментарий ниже, чтобы применить. Потом COMMIT. ===

/*
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

-- COMMIT;
*/

PROMPT Готово. Если превью ок — раскомментировать UPDATE и COMMIT, переоткрыть комбо в SAUMI.
