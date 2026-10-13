#!/usr/bin/env bash
# Минимальный подъём: схема 485 + (опционально) справочники.
# Учётка: SM / smpwd
set -euo pipefail
cd "$(dirname "$0")"

CONTAINER="${CONTAINER:-saumiweb-oracle}"
PDB="${PDB:-FREEPDB1}"
SYS_PASS="${ORACLE_PASSWORD:-smpwd}"
SM_USER=SM
SM_PASS=smpwd
SCHEMA_FILE="${SCHEMA_FILE:-saumi485_schema_20260904.sql}"
DATA_FILE="${DATA_FILE:-data/saumi485_dicts_inserts.sql}"
# БД в образе — AL32UTF8; клиент тоже UTF-8
NLS_LANG_IMPORT="${NLS_LANG_IMPORT:-AMERICAN_AMERICA.AL32UTF8}"

[[ -f "$SCHEMA_FILE" ]] || { echo "FAIL: нет $SCHEMA_FILE"; exit 1; }

echo "=== wait Oracle ($CONTAINER) ==="
for i in $(seq 1 90); do
  if docker exec "$CONTAINER" healthcheck.sh >/dev/null 2>&1; then
    echo "Oracle ready"; break
  fi
  sleep 5
  [[ "$i" -eq 90 ]] && { echo "FAIL: Oracle не готов — docker compose up -d"; exit 1; }
done

echo "=== SM / smpwd + CHAR semantics ==="
docker exec -i "$CONTAINER" sqlplus -s "sys/${SYS_PASS}@${PDB} as sysdba" < init/01_prepare_sm.sql
docker exec -i "$CONTAINER" sqlplus -s "sys/${SYS_PASS}@${PDB} as sysdba" <<'SQL'
ALTER SYSTEM SET NLS_LENGTH_SEMANTICS=CHAR SCOPE=BOTH;
ALTER SESSION SET NLS_LENGTH_SEMANTICS=CHAR;
EXIT
SQL

prepare_sql() {
  local src="$1" dst="$2" mode="${3:-data}"
  python3 - "$src" "$dst" "$mode" <<'PY'
import re
import sys
from pathlib import Path
src, dst, mode = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
raw = src.read_bytes()
if raw[:4] == b"SPIS":
    raise SystemExit(
        f"FAIL: {src.name} сжатый экспорт PL/SQL Developer (SPIS).\n"
        "Перевыгрузи БЕЗ галки «Сжать файл», формат SQL Inserts."
    )
text = None
used = None
for enc in ("cp1251", "utf-8", "latin1"):
    try:
        text = raw.decode(enc)
        used = enc
        break
    except Exception:
        pass
if text is None:
    raise SystemExit(f"FAIL: не декодировать {src}")
out = []
for line in text.splitlines():
    s = line.strip().lower()
    if s.startswith("spool "):
        continue
    out.append(line)
body = "\n".join(out) + "\n"
if mode == "schema":
    # WIN1251-схема с BYTE: кириллица в AL32UTF8 не влезает → CHAR
    body = re.sub(r"\bBYTE\b", "CHAR", body, flags=re.IGNORECASE)
dst.write_text(body, encoding="utf-8")
print(f"prepared {dst.name} ({dst.stat().st_size} bytes, {used}->utf-8, mode={mode})")
PY
}

sqlplus_sm() {
  local file="$1"
  # docker cp → uid хоста; chmod от root, иначе oracle не читает
  docker exec -u root "$CONTAINER" chmod a+r "$file"
  docker exec -e NLS_LANG="$NLS_LANG_IMPORT" "$CONTAINER" bash -c "
cat > /tmp/saumi_run.sql <<EOF
ALTER SESSION SET NLS_LENGTH_SEMANTICS=CHAR;
ALTER SESSION SET NLS_LANGUAGE=AMERICAN;
ALTER SESSION SET NLS_TERRITORY=AMERICA;
@$file
EXIT
EOF
sqlplus -s ${SM_USER}/${SM_PASS}@${PDB} @/tmp/saumi_run.sql
"
}

# XXXXXX must be at end of template (macOS mktemp)
WORK_SCHEMA="$(mktemp /tmp/saumi_schema.XXXXXX)"
prepare_sql "$SCHEMA_FILE" "$WORK_SCHEMA" schema
docker cp "$WORK_SCHEMA" "$CONTAINER:/tmp/saumi485_schema.sql"
rm -f "$WORK_SCHEMA"
docker exec "$CONTAINER" test -s /tmp/saumi485_schema.sql \
  || { echo "FAIL: docker cp схемы не сработал"; exit 1; }

echo "=== import SCHEMA as SM ==="
sqlplus_sm /tmp/saumi485_schema.sql | tee import_schema.log | tail -50

if [[ -f "$DATA_FILE" ]]; then
  WORK_DATA="$(mktemp /tmp/saumi_data.XXXXXX)"
  prepare_sql "$DATA_FILE" "$WORK_DATA" data
  docker cp "$WORK_DATA" "$CONTAINER:/tmp/saumi485_data.sql"
  rm -f "$WORK_DATA"
  docker exec "$CONTAINER" test -s /tmp/saumi485_data.sql \
    || { echo "FAIL: docker cp данных не сработал"; exit 1; }
  echo "=== import DATA ==="
  sqlplus_sm /tmp/saumi485_data.sql | tee import_data.log | tail -40
else
  echo "WARN: нет $DATA_FILE — только схема (вход в Oracle возможен; справочники пустые)"
  echo "      положи SQL Inserts без сжатия: $DATA_FILE"
fi

echo "=== check ==="
docker exec -e NLS_LANG="$NLS_LANG_IMPORT" -i "$CONTAINER" sqlplus -s "${SM_USER}/${SM_PASS}@${PDB}" <<'SQL'
SET PAGES 0 FEED OFF VERIFY OFF
SELECT 'user='||USER FROM dual;
SELECT 'charset='||value FROM nls_database_parameters WHERE parameter='NLS_CHARACTERSET';
SELECT 'tables='||COUNT(*) FROM user_tables;
SELECT 'views='||COUNT(*) FROM user_views;
SELECT 'packages='||COUNT(*) FROM user_objects WHERE object_type='PACKAGE';
SELECT 'functions='||COUNT(*) FROM user_procedures WHERE object_type='FUNCTION';
SELECT 'cls_okof='||COUNT(*) FROM cls_okof;
SELECT 'doctypes='||COUNT(*) FROM doctypes;
SELECT 'dictionaries='||COUNT(*) FROM dictionaries;
SELECT 'movetype='||COUNT(*) FROM movetype;
SELECT 'buildtypes='||COUNT(*) FROM buildtypes;
SELECT 'orgforms='||COUNT(*) FROM orgforms;
EXIT
SQL

echo ""
echo "ГОТОВО — подключение для SAUMI / sqlplus:"
echo "  user=SM  password=smpwd  service=localhost:1521/${PDB}"
echo "  tnsnames: FREEPDB1 → localhost:1521 / SERVICE_NAME=FREEPDB1"
