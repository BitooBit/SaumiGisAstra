# Логика входа в SAUMI (из декомпиляции exe) — черновик

Дата: 2026-09-10. Источники: `decompile/bin/mmp32.exe` (Delphi/DOA), `decompile/src/InflexENV` (.NET).

**Принцип:** восстанавливаем **порядок действий** клиента, не только имена символов.

---

## Цепочка (высокоуровнево)

```
1. UI логина (Delphi)
   LoginMod.TLoginPrompt  и/или  Oraclelogon.TLogOnForm (DOA)
   поля: Username / Password / Database (TNS alias)
   история: HKCU\Software\Allround Automations\DOA\LogonHistory

2. Подключение Oracle (Delphi DOA)
   TOracleSession.SetLogonUsername / SetLogonPassword / SetLogonDatabase
   TOracleSession.SetConnected → DoBeforeLogon → connect → DoAfterLogon

3. Хуки сессии SAUMI (Delphi)
   Datamod.TGData.MMP_SessionBeforeLogOn
   Datamod.TGData.MMP_SessionAfterLogOn
   Inflex.TAppManager.AfterLogonInit

4. Мост в .NET (после успешного connect)
   Saumi4.UserName / Saumi4.Database
   OracleProvider.StartConnection(user, database, password)  // ODP.NET / System.Data.OracleClient
   Inflex_GW.Inflex_Init("SAUMI30", user, "", database)      // native inflexe_gw
   опционально InflexUser.OnStart / UpdateService
```

Пилот веб-клиента на шаге 1–2 + минимальный post-logon probe (шаг 3 без UI).

---

## Поля формы логина

| Поле | Где в exe | Смысл |
|------|----------|--------|
| Username | `SetLogonUsername` / `Saumi4.UserName` | схема Oracle (часто `SM`) |
| Password | `SetLogonPassword` / `OracleProvider` | пароль схемы |
| Database | `SetLogonDatabase` / `Saumi4.Database` | TNS alias из `tnsnames.ora` |

Дополнительно в строках DOA: `ldConnectAs`, `ldPasswordOnly`, «Запросить пароль при подключении», `HistoryWithPassword`.

Кастомный диалог: `Loginmod@TLoginPrompt` (`Edit1`/`Edit2`, `Button1`/`Button3`, `DetailsBtnClick`) — поверх/рядом со стандартным DOA `TLogOnForm`.

---

## Post-logon (что клиент делает после connect)

Из строк `mmp32` (не полный дизасм, но рабочие фрагменты SQL):

1. **Схема сессии**  
   `SELECT sys_context('userenv', 'CURRENT_SCHEMA') cur_user FROM dual`

2. **Права на таблицу (шаблон)** — заполняет `:I/:U/:S/:D` = Y/N по `all_tab_privs` + `session_roles` (INSERT/UPDATE/SELECT/…). Имена таблицы/grantor/grantee подставляются в runtime.

3. **Параметры сессии** (частично видны):  
   `alter session set isolation_level = …`  
   `alter session set optimizer_…` / `optimizer_mode = …`  
   `alter session set sql_trace = …`

4. **.NET** хранит то же user/db/password и открывает **второе** соединение ODP для Pascal/плагинов (`SaumiPascalSQL`), либо шарит handle через `SetConnection(password, handle)`.

Точный полный список вызовов `MMP_SessionAfterLogOn` — дальше: трассировка SQL на стенде + IDR/Ghidra по `MMP_SessionAfterLogOn`.

---

## Соответствие веб-пилоту

| Exe | Web |
|-----|-----|
| Username + Password + Database | `POST /auth/login` JSON |
| TNS alias | `connect_string` / DSN из env или body |
| DOA session | `oracledb` connection в FastAPI |
| AfterLogon probe | `SELECT USER, sys_context('userenv','CURRENT_SCHEMA') FROM dual` |
| Главное окно | позже: shell + меню |

Локальная БД для пробы: см. `db/README.md` (`SM`/`smpwd`/`FREEPDB1`).

---

## Открытые вопросы

- [ ] Полный код `MMP_SessionAfterLogOn` / `AfterLogonInit` (порядок SQL)
- [ ] Чем `TLoginPrompt` отличается от штатного DOA Logon (автологин, `/updated`)
- [ ] Когда вызывается `Inflex_Init` относительно первой формы главного окна
- [ ] NLS_LANG / клиентская кодировка (в Wine: `AMERICAN_AMERICA.CL8MSWIN1251`)
