SasCoreService — payload и скрипты службы
==========================================

SaumiAppServer.zip     — исходный архив с Windows
payload/               — распакованное дерево (Core, Components, …)
MSVCR100_CLR0400.dll   — CRT CLR 4 (bake → Framework\v4.0.30319 и system32)
ndp_v4_full.reg        — ключи NDP\v4\Full (bake → system.reg)

Скрипты (в prefix/bin/ при bake):
  install_sascore.sh   — Settings.xml, ярлыки, старт
  start_sascore.sh     — запуск SasCoreWindowsService.exe
  stop_sascore.sh      — остановка
  probe_sascore.sh     — процесс и LISTEN

Поставка: install/installSasCore.sh + install/SasCoreWine.tar.gz
Сборка tar: files/scripts/bake_sascore_tar.sh
