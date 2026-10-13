#!/usr/bin/env bash
# Диагностика замечаний SAUMI 186 / папка Замечания/1.
# Патч Sumatra / UPDATE цветов НЕ ставит. Только снимает клиент + Oracle в лог.
#
# Поставка: копировать папку install/pathes → /var/install/saumi485/pathes
# На АРМ (не root, prefix уже есть):
#   cd /var/install/saumi485/pathes/4
#   SAUMI_PREFIX=$HOME/Saumi bash patch_remarks1_diag.sh
#
# Спросит пользователя/пароль Oracle в терминале (пароль не эхо и не пишется в лог).
# Готовый архив отдай обратно — по нему будет патч по задачам 1/2/3/5.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
SQL_SRC=""
for f in "${SCRIPT_DIR}/remarks1_diag.sql" \
         "${SCRIPT_DIR}/../src/document_handlers/remarks1_diag.sql"; do
  [[ -f "$f" ]] && SQL_SRC="$f" && break
done

[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет ${PREFIX}/startSAUMI.sh"; exit 1; }
[[ -n "$SQL_SRC" ]] || { echo "FAIL: нет remarks1_diag.sql рядом со скриптом"; exit 1; }

stamp="$(date +%Y%m%d_%H%M%S)"
OUT="${PREFIX}/logs/remarks1_${stamp}"
mkdir -p "$OUT"

{
  echo "=== remarks1 diag $stamp ==="
  echo "user=$(id -un) host=$(hostname) home=$HOME"
  echo "prefix=$PREFIX"
  date
  uname -a
  echo "--- wine ---"
  WINE_BIN="${PREFIX}/runtime/wine-11.0/bin/wine"
  if [[ -x "$WINE_BIN" ]]; then
    echo "wine_bin=$WINE_BIN"
    env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINE_BIN" --version 2>&1 || true
  else
    echo "NO wine at $WINE_BIN"
    command -v wine; wine --version 2>&1 || true
  fi
} >"$OUT/00_host.txt" 2>&1

{
  echo "=== 1-2 PDF/ассоциации/Sumatra (без установки) ==="
  echo "--- files ---"
  ls -la "${PREFIX}/drive_c/SAUMI/components/SumatraPDF/" 2>&1 || echo "NO Sumatra dir"
  ls -la "${PREFIX}/drive_c/SAUMI/"*.pdf "${PREFIX}/drive_c/SAUMI/"EZPdf.dll \
    "${PREFIX}/drive_c/SAUMI/"Eztwain3.dll \
    "${PREFIX}/drive_c/SAUMI/ExplorerWindow.exe" 2>&1 || true
  echo "--- HKCR .pdf / Sumatra / Acro / Foxit in system.reg ---"
  if [[ -f "${PREFIX}/system.reg" ]]; then
    grep -n -i -E '\\\\.pdf|SumatraPDF|AcroRd|Acrobat|Foxit|PDF.Document|application/pdf' \
      "${PREFIX}/system.reg" | head -n 80 || echo "(no pdf keys)"
  else
    echo "NO system.reg"
  fi
  echo "--- user.reg pdf ---"
  if [[ -f "${PREFIX}/user.reg" ]]; then
    grep -n -i -E '\\\\.pdf|SumatraPDF|AcroRd|Foxit|FileExts' \
      "${PREFIX}/user.reg" | head -n 40 || echo "(no pdf in user.reg)"
  fi
  echo "--- wine assoc/ftype (10с) ---"
  WINE_BIN="${PREFIX}/runtime/wine-11.0/bin/wine"
  if [[ -x "$WINE_BIN" ]]; then
    timeout 10 env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINE_BIN" cmd /c assoc .pdf 2>&1 || true
    timeout 10 env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINE_BIN" cmd /c ftype SumatraPDF.pdf 2>&1 || true
  fi
  echo "--- SHGetFileInfo in last saumi log ---"
  grep -n 'SHGetFileInfo' "${PREFIX}/logs/saumi_latest.log" 2>/dev/null | tail -n 20 || true
} >"$OUT/01_pdf_assoc.txt" 2>&1

{
  echo "=== 5 TWAIN / SANE / сканер ==="
  echo "--- twain in prefix ---"
  ls -la "${PREFIX}/drive_c/windows/twain_32/" 2>&1 || true
  ls -la "${PREFIX}/drive_c/windows/system32/sane.ds" \
         "${PREFIX}/drive_c/windows/twain_32/sane.ds" \
         "${PREFIX}/drive_c/windows/twain_32/gphoto2.ds" \
         "${PREFIX}/drive_c/windows/system32/twain_32.dll" 2>&1 || true
  echo "--- wine unix sane.so ---"
  ls -la "${PREFIX}/runtime/wine-11.0/lib/wine/i386-unix/sane.so" 2>&1 || true
  echo "--- host sane packages ---"
  (dpkg -l 'libsane*' 'sane-*' 'libsane1*' 2>/dev/null | awk 'NR==1 || /^ii/' ) || true
  echo "--- scanimage ---"
  command -v scanimage || echo "NO scanimage"
  scanimage -V 2>&1 || true
  echo "--- scanimage -L (до 90с) ---"
  timeout 90 scanimage -L 2>&1 || echo "scanimage -L fail/timeout"
  echo "--- i386 sane backends ---"
  ls /usr/lib/i386-linux-gnu/sane /lib32/i386-linux-gnu/sane 2>&1 | head -n 40 || true
  echo "--- amd64 sane backends (airscan/hpaio/net) ---"
  ls /usr/lib/x86_64-linux-gnu/sane 2>/dev/null | grep -iE 'airscan|hpaio|net' || ls /usr/lib/x86_64-linux-gnu/sane 2>/dev/null | head
  echo "--- sane-find-scanner ---"
  command -v sane-find-scanner >/dev/null && timeout 20 sane-find-scanner -q 2>&1 || echo "no sane-find-scanner"
  echo "--- groups ---"
  id
  echo "--- sane.d ---"
  ls /etc/sane.d 2>&1 | head || true
  echo "--- ldd sane.so (i386) ---"
  if [[ -f "${PREFIX}/runtime/wine-11.0/lib/wine/i386-unix/sane.so" ]]; then
    ldd "${PREFIX}/runtime/wine-11.0/lib/wine/i386-unix/sane.so" 2>&1 || true
  fi
} >"$OUT/05_scanner.txt" 2>&1

echo
echo "Oracle для замечаний 1–3 (цвета MOVETYPE, расширения документов)."
echo "Пароль в лог не пишется. Пустой пользователь = SM, пустой TNS = MMPSRV."
printf "Oracle user [SM]: "
read -r ORA_USER
ORA_USER="${ORA_USER:-SM}"
printf "TNS [MMPSRV]: "
read -r ORA_TNS
ORA_TNS="${ORA_TNS:-MMPSRV}"
printf "Oracle password for %s@%s: " "$ORA_USER" "$ORA_TNS"
read -r -s ORA_PASS
echo
[[ -n "$ORA_PASS" ]] || { echo "FAIL: пустой пароль"; exit 1; }

SQL_WIN="${PREFIX}/drive_c/SAUMI/logs/_remarks1_diag.sql"
mkdir -p "${PREFIX}/drive_c/SAUMI/logs"
cp -f "$SQL_SRC" "$SQL_WIN"

sql_log="$OUT/03_oracle.txt"
{
  echo "=== Oracle connect user=${ORA_USER} tns=${ORA_TNS} (пароль скрыт) ==="
  echo "sql_src=$SQL_SRC"
} >"$sql_log"

run_ok=0
WINE_BIN="${PREFIX}/runtime/wine-11.0/bin/wine"
WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
SQLPLUS_EXE="${PREFIX}/drive_c/oracle/bin/sqlplus.exe"

# connect user/"pass"@tns — кавычки, если в пароле спецсимволы кроме "
ora_quote() {
  printf '%s' "$1" | sed 's/"/""/g'
}
CONN="(stdin CONNECT, пароль не в argv)"
TNS_UNIX="${PREFIX}/drive_c/oracle/NETWORK/ADMIN"
{
  echo "--- tnsnames (без паролей) ---"
  ls -la "${TNS_UNIX}/tnsnames.ora" "$SQLPLUS_EXE" 2>&1 || true
  if [[ -f "${TNS_UNIX}/tnsnames.ora" ]]; then
    grep -v -i -E 'password|passwd' "${TNS_UNIX}/tnsnames.ora" | head -n 40
  else
    echo "NO tnsnames.ora"
  fi
} >>"$sql_log" 2>&1

# SQL*Plus: TNS уже резолвится (ORA-01017 в pathes/2 = кавычки в argv Wine).
# CONNECT уходит в stdin sqlplus, пароль не в командной строке и не в логе.
if [[ -x "$WINE_BIN" && -f "$SQLPLUS_EXE" ]]; then
  echo "sqlplus: wine sqlplus.exe /nolog + CONNECT stdin (без пароля в argv)" >>"$sql_log"
  set +e
  (
    cd "${PREFIX}/drive_c/oracle/bin"
    {
      printf 'CONNECT %s/"%s"@%s\n' "$ORA_USER" "${ORA_PASS//\"/\"\"}" "$ORA_TNS"
      printf '%s\n' "@C:\\SAUMI\\logs\\_remarks1_diag.sql" "EXIT"
    } | env WINEPREFIX="$PREFIX" WINEARCH=win32 WINESERVER="$WINESERVER" \
      NLS_LANG="${NLS_LANG:-AMERICAN_AMERICA.CL8MSWIN1251}" \
      ORACLE_HOME="C:\\oracle" \
      TNS_ADMIN="C:\\oracle\\NETWORK\\ADMIN" \
      "$WINE_BIN" sqlplus.exe -S /nolog
  ) >>"$sql_log" 2>&1
  rc=$?
  set -e
  echo "wine_sqlplus_exit=$rc" >>"$sql_log"
  [[ "$rc" -eq 0 ]] && run_ok=1
elif command -v sqlplus >/dev/null 2>&1; then
  echo "sqlplus: host stdin CONNECT" >>"$sql_log"
  set +e
  {
    printf 'CONNECT %s/"%s"@%s\n' "$ORA_USER" "${ORA_PASS//\"/\"\"}" "$ORA_TNS"
    printf '%s\n' "@${SQL_SRC}" "EXIT"
  } | env TNS_ADMIN="${TNS_ADMIN:-$TNS_UNIX}" sqlplus -S /nolog >>"$sql_log" 2>&1
  rc=$?
  set -e
  echo "host_sqlplus_exit=$rc" >>"$sql_log"
  [[ "$rc" -eq 0 ]] && run_ok=1
else
  echo "FAIL: нет wine sqlplus.exe и нет sqlplus на хосте" >>"$sql_log"
fi

unset ORA_PASS CONN
rm -f "$SQL_WIN"

if grep -q 'ORA-01017\|ORA-12154\|ORA-12541\|ORA-12514\|ORA-01005' "$sql_log"; then
  echo "WARN: ошибка входа/TNS — смотри 03_oracle.txt (пароля в файле нет)"
fi

# cp1251 → utf-8 копия, если получится
if command -v iconv >/dev/null 2>&1; then
  iconv -f cp1251 -t utf-8 -c "$sql_log" >"$OUT/03_oracle.utf8.txt" 2>/dev/null || true
fi

{
  echo "=== log files in prefix ---"
  ls -la "${PREFIX}/logs" | tail -n 30
  echo "--- last saumi log names ---"
  ls -1t "${PREFIX}/logs"/saumi_*.log 2>/dev/null | head -n 5 || true
} >"$OUT/09_logs_index.txt" 2>&1

# последние wine-логи целиком не копируем (сотни КБ..МБ) — только хвост имён
if [[ -f "${PREFIX}/logs/saumi_latest.log" ]]; then
  tail -n 80 "${PREFIX}/logs/saumi_latest.log" >"$OUT/09_saumi_latest_tail.txt" 2>/dev/null || true
fi

echo "oracle_ok=$run_ok" >"$OUT/99_summary.txt"
echo "out=$OUT" >>"$OUT/99_summary.txt"

TAR="${PREFIX}/logs/remarks1_${stamp}.tar.gz"
tar -czf "$TAR" -C "${PREFIX}/logs" "remarks1_${stamp}"
echo
echo "OK: диагностика в $OUT"
echo "    архив: $TAR"
echo "Отдай архив (или папку remarks1_*). Пароль в файлах не должен быть."
echo "Sumatra/цвета пока НЕ ставил — это следующий патч после разбора лога."
[[ "$run_ok" -eq 1 ]] || echo "WARN: sqlplus не прошёл — клиентская часть всё равно в архиве."
