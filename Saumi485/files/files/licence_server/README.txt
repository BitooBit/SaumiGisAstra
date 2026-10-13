Guardant Net — служба лицензий SAUMI
====================================

Папка для сборки (bake_licence_into_tar.sh) и установки (--licence-server).

Обязательные файлы:
  NNKSRV32.EXE, NNKSRV32.INI, NOVEX32.DLL, GNCLIENT.INI
  install_licence_server.sh, start_licence_server.sh
  stop_licence_server.sh, probe_licence_server.sh

Дополнительно (драйверы / установщик Guardant на Windows, для справки):
  LicenseInst.exe, GrdDriversRU.msi, GrdDriversRU_x64.msi
  setup_drv.exe, setup_drv.ini, drivers-x64.zip, GSREMOTE.EXE

После bake файлы попадают в tar:
  drive_c/SAUMI/NNKSRV32.*
  drive_c/SAUMI/gnclient.ini.licence_local
  bin/install_licence_server.sh и др.

Установка на сервере — через instalSaumi.sh --licence-server
(см. install/ИНСТРУКЦИЯ.docx).
