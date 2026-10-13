# Главная панель и меню SAUMI — из `mmp32.exe`, не из догадок

Дата: 2026-09-14.  
Источник: `decompile/bin/mmp32.exe` (Delphi DFM `TSAUMIMAINPANEL` + SQL-строки `Panelmod.TSaumiMainPanel`), схема `db/saumi485_schema_20260904.sql`.

**Принцип:** пункты и SQL копируем из exe/схемы. Где Pascal-тело ещё не снято дизасмом — явно помечено «дыра», без выдуманных WHERE.

Это **не** классический `TMainMenu` File/Правка. Главный UI — `TSaumiMainPanel` (appbar): ряд кнопок + popup-меню, которые `PrepareMenu*` наполняют из БД.

---

## Где в exe собирается меню

Класс: `Panelmod.TSaumiMainPanel` (`panelmod.pas`). DFM-ресурс `TSAUMIMAINPANEL`.

| Метод exe | Что делает |
|-----------|------------|
| `PrepareMainPanel` | сборка панели после logon |
| `PrepareMenuReports` | popup «Отчеты» |
| `PrepareMenuPlugins` | popup «Дополнения» |
| `PrepareMenuCreateNewItem` | popup «Создать» |
| `PrepareMenuHistory` | popup «История» |
| `PrepareMenuOwnerAgent` | список базовых собственников |
| `ConfigureAutonomousReportFormsMenu` | подменю «Отчётные формы» кнопки SAUMI |
| `MenuCreateAddSubMenu` | вложенные пункты |
| `PluginMenuItemClick` / `RunDotNetPlugin` | запуск дополнения |
| `ReportClick` | открытие отчёта из дерева |
| `ContextReportFormClick` | печатная форма (контекст объекта, не главная панель) |
| `LoadTemplates` | шаблоны поиска |
| `TCheckRole` / `GetRoleValue` (`Ufmfuncs`) | проверка роли сессии |
| `AddMenuItem(...)` | SAUMI-Pascal может **добавлять** пункты в runtime |

`.NET` `InflexActionMenu` — **контекстное** меню ExplorerWindow, не эта панель. Не смешивать.

---

## 1. Статика (скомпилирована в DFM)

Форма: `Caption = 'SAUMI 4.6i'`.

### Кнопки панели (`Panel1`)

Порядок в DFM (слева направо в дереве детей `panUser`):

| Имя | Caption | OnClick | Visible в DFM | Popup / наполнение |
|-----|---------|---------|---------------|-------------------|
| `Label1` | `Имя пользователя ` | — | True | текущий Oracle user |
| `KPanelMvGrad2` | `Inflex` | `N11Click` | **False** | служебное |
| `ReportsBtn` | `Отчеты` | `ReportsBtnClick` | True | `ReportList` ← `PrepareMenuReports` |
| `ObligFindBtn` | `Обязательства` | `ObligFindBtnClick` | True | поиск обязательств (не дерево меню) |
| `FindBtn` | `Поиск` | `FindBtnClick` | True | `popFindTempls` ← `LoadTemplates` |
| `SAUMI_Button` | `SAUMI 4.7i` | `SAUMI_ButtonClick` | True | `BarPopupMenu1` + автономные формы |
| `PluginsBtn` | `Дополнения` | `PluginsBtnClick` | **False** | `PluginsMenu` ← `PrepareMenuPlugins`; кнопку показывают, если после фильтра есть пункты |
| `HistoryBtn` | `История` | `HistoryBtnClick` | True | `HistoryMenu` ← `PrepareMenuHistory` |
| `MapBtn` | `Карта` | `MapBtnClick` | **False** | `popMap` ← `sm.sd3_workspaces` |
| `CreateNewItemBtn` | `Создать` | `CreateNewItemBtnClick` | True | `CreateItemMenu` ← `PrepareMenuCreateNewItem` |

Пустые в DFM (наполняются кодом): `ReportList`, `PluginsMenu`, `CreateItemMenu`, `popMap`.

### `BarPopupMenu1` (кнопка SAUMI) — состав из DFM

| Имя | Caption | OnClick | Visible |
|-----|---------|---------|---------|
| `MI_Welcome` | `&Добро пожаловать в SAUMI` | `MI_WelcomeClick` | True |
| `MI_ReportFormsRoot` | `Отчётные формы` | — | True | дети ← `ConfigureAutonomousReportFormsMenu` |
| `N3` | `-` | | |
| `N4` | `Мои действия за день` | `N4Click` | True |
| `N2` | `Документы на исполнение` | `N2Click` | True |
| `MI_CompareAns` | `Сравнительный анализ` | `MI_CompareAnsClick` | **False** |
| `N13` | `-` | | |
| `MI_ExportPayments` | `Выгрузка начислений и платежей` | `MI_ExportPaymentsClick` | True |
| `N5` | `-` | | |
| `N14` | `Настройки..` | подменю | True |
| └ `MI_SaveCurrentOwnerID` | `Запомнить текущего базового собственника` | `MI_SaveCurrentOwnerIDClick` | |
| └ `N16` | `-` | | |
| └ `MI_ShowCase` | `Использовать буфер` | `MI_ShowCaseClick` | |
| └ `MI_SaveCase` | `Сохранять содержимое буфера` | `MI_SaveCaseClick` | |
| └ `MI_FreeSAUMIpanel` | `Плавающая панель SAUMI` | `MI_FreeSAUMIpanelClick` | |
| └ `N15` | `-` | | |
| └ `MI_ChangePassword` | `Сменить пароль..` | `MI_ChangePasswordClick` | |
| `N7` | `-` | | |
| `MI_DEBUGMODE` | `Режим отладки` | `MI_DEBUGMODEClick` | True |
| `MI_SPDEBUG` | `Пошаговое выполнение SAUMI-Pascal` | `MI_SPDEBUGClick` | **False** |
| `N8` | `-` | | |
| `N9` | `О программе..` | `N9Click` | True |
| `N10` | `Выход` | `N10Click` | True |

### `popFindTempls` (статика + шаблоны из БД)

| Имя | Caption | Tag |
|-----|---------|-----|
| `MI_FullFind` | `Полный поиск` | −1 |
| `N6`, `N12`, `N11` | `-` | | между ними вставляются шаблоны |
| `MI_NewFindTemplate` | `Новый шаблон поиска` | |
| `MI_DeleteFindTemplate` | `Удалить шаблон поиска` | |

`HistoryMenu` в DFM содержит заглушку `N1` = `&Пункты меню`; живой список — `PrepareMenuHistory`.

---

## 2. Динамика: SQL из exe (копировать как есть)

Схема в запросах — `sm.` (в строках exe часто `.plugins` / `.fm_tables`: Delphi склеивает schema).

### 2.1 Роли сессии (фильтр прав)

```sql
select ROLE from session_roles
```

`.NET` то же: `SessionRolesAccessor` / `select role from <T>` по `sys.session_roles`, `AppManager.LoadEnabledRoles`.

**Дыра:** тело `TCheckRole`/`GetRoleValue` не дизассемблировано. Везде, где exe делает проверку роли на клиенте, веб пока использует членство в этом списке (тот же источник, что `USER_REPORTFORMS` и `REPORTSTREE`). Краёв вроде «пустая роль = всегда да» для `PLUGINS.EXECUTE_ROLE` нет: колонка `NOT NULL`, default `'SM_ALLUSERS'`.

Права на таблицу (рядом с «Создать», тот же шаблон что в `login_flow.md`):

```sql
begin
  :I:='N'; :U:='N'; :S:='N'; :D:='N';
  for xx in (
    select distinct(a.privilege) as aaa
      from all_tab_privs a, session_roles b
     where a.table_name='…'   -- подстановка runtime
       -- grantor/grantee тоже подставляются в runtime; полный текст после :D в строке обрезан
  ) loop
    -- выставляет :I/:U/:S/:D = Y
  end loop;
end;
```

### 2.2 «Дополнения» — `PrepareMenuPlugins`

Таблицы: `SM.PLUGIN_GROUPS`, `SM.PLUGINS`.  
Комментарии схемы: группы и пункты меню **«Дополнения»**; `EXECUTE_ROLE` — роль, без которой пункт не запускается.

Список пунктов (**роли в SQL не фильтруются** — только колонка `execute_role`, фильтр на клиенте):

```sql
select
  nvl(pl.ord,1000) PL_ORD,
  pl.id PL_ID,
  pl.menu_caption,
  pl.file_name,
  pl.file_extention,
  pl.execute_role,
  nvl(pl.is_dot_net,'N') IS_DOT_NET,
  nvl(plg.ord,1000) PLG_ORD,
  plg.name GROUP_NAME,
  '1' as ORD
from sm.plugins pl, sm.plugin_groups plg
where
  pl.plugin_groups_id is not null and
  pl.plugin_groups_id=plg.id
union
select
  nvl(pl.ord,1000) PL_ORD,
  pl.id PL_ID,
  pl.menu_caption,
  pl.file_name,
  pl.file_extention,
  pl.execute_role,
  nvl(pl.is_dot_net,'N') IS_DOT_NET,
  1000 as PLG_ORD,
  null as GROUP_NAME,
  '2' as ORD
from sm.plugins pl
where
  pl.plugin_groups_id is null
order by ORD,PLG_ORD,GROUP_NAME,PL_ORD desc, menu_caption desc
```

Во второй ветке в дампе строк `where` отрезан от `from`; без `where` запрос невалиден — склеиваем как в первой ветке.

Запуск по `PL_ID`:

```sql
select
  nvl(pl.need_login,'N') NEEDLOGIN,
  pl.file_name,
  pl.file_extention,
  pl.execute_params,
  nvl(pl.is_dot_net,'N') IS_DOT_NET,
  pl.menu_caption
from sm.plugins pl
where pl.id = :id
```

Если `NEEDLOGIN=Y`, exe дописывает `/USER=` `/BASE=` `/PASS=` `/KUMI_ID=`.  
`IS_DOT_NET=Y` → `RunDotNetPlugin`, иначе native/`TPluginManager`.

### 2.3 «Отчеты» — `PrepareMenuReports`

Таблица `SM.REPORTSTREE` («Дерево отчетов»). Колонки exe: `ID, NAME, GROUPMARK, BIGREPORT, SEQN, ROLEGRANTED, ROOT`.

Корень:

```sql
select t.ID, t.NAME, t.GROUPMARK, t.BIGREPORT
  from SM.REPORTSTREE t, session_roles sr
 where t.ROOT=0
   and (
        (t.rolegranted=sr.role and (not t.rolegranted is null))
     or (t.rolegranted is null and sr.role='CONNECT')
       )
 order by SEQN
```

Дети (тот же предикат ролей, `ROOT=` подставляется):

```sql
select t.ID, t.NAME, t.GROUPMARK, t.BIGREPORT
  from SM.REPORTSTREE t, session_roles sr
 where t.ROOT=:parent
   and (
        (t.rolegranted=sr.role and (not t.rolegranted is null))
     or (t.rolegranted is null and sr.role='CONNECT')
       )
 order by SEQN
```

(`or` во фрагменте детей в дампе разрезан; берём предикат из полного корневого запроса.)

`GROUPMARK` — узел-группа vs лист. Лист → `ReportClick`.

Пользовательское дерево (не SM-схема отчётов, а таблица пользователя; exe умеет `create table`, если нет):

```sql
select TABLE_NAME from user_tables where TABLE_NAME='SM_USER_REPORTSTREE'
-- create table SM_USER_REPORTSTREE (...)  -- текст в exe
-- create sequence seqSM_USER_REPORTSTREE
```

Корень пользовательских (аналитика vs нет — union по `qparams`):

```sql
select t.ID, t.NAME, t.GROUPMARK, 'Y' ANALITICS
  from SM_USER_REPORTSTREE t
 where t.ROOT is NULL and t.qparams like '%[Analitic]%'
union
select t.ID, t.NAME, t.GROUPMARK, 'N' ANALITICS
  from SM_USER_REPORTSTREE t
 where (t.qparams not like '%[Analitic]%' or t.qparams is null)
   and t.ROOT is NULL
 order by GROUPMARK DESC, Name
```

Дети: `t.ROOT=:id`, `ORDER BY Name`. Методы `GetPrivateTree` / `LoadPrivateTree` / `UserReports1Click` / `OrganizeReportsMenu1Click`.

**Дыра:** точный порядок склейки системного дерева, пользовательского и пунктов «загрузить из файла» / «организовать» внутри `PrepareMenuReports` без дизасма не восстанавливается. SQL узлов — выше, без выдуманных подписей.

### 2.4 Автономные «Отчётные формы» (кнопка SAUMI)

Вьюха уже фильтрует роли (это **серверная** логика, не догадка):

```sql
-- SM.USER_REPORTFORMS
select rf."ID", rf."NAME", rf."DESCRIPTION", rf."REPORT_ID", rf."SEQN",
       rf."ITEM_HANDLING", rf."IS_DEFAULT", rf."SHORTCUT_CODE",
       rf."FORM_BODY", rf."ROLE_GRANTED", gr."NAME" as "MENU_GROUP"
  from ReportForms rf, reportformgroups gr
 where (gr.id(+)=rf.repformgroup_id)
   and (rf.ROLE_GRANTED IS NULL
        or exists (select ROLE from SESSION_ROLES where ROLE=ROLE_GRANTED))
```

Наполнение `MI_ReportFormsRoot`:

```sql
select ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP
  from sm.User_reportforms
 where NAME IS NULL and Report_ID IS NULL
 order by MENU_GROUP DESC, "DESCRIPTION" DESC
```

Тело формы: `select DESCRIPTION, FORM_BODY from sm.USER_REPORTFORMS where ID=:id`.

Контекстные печатные формы документа (не главная панель; `name='DOCFORM'`):

```sql
select ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, MENU_GROUP, '2' as ORD
  from sm.User_reportforms
 where Report_ID=:id and UPPER(name)='DOCFORM' and MENU_GROUP is not null
union
select ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, MENU_GROUP, '1' as ORD
  from sm.User_reportforms
 where Report_ID=:id and UPPER(name)='DOCFORM' and MENU_GROUP is null
 order by ORD, MENU_GROUP DESC, "DESCRIPTION" DESC
```

Группы: `SM.REPORTFORMGROUPS` (комментарий: «Наименования групп меню для печатных форм», `DATATYPE_ID`).

### 2.5 «Создать» — `PrepareMenuCreateNewItem`

`SM.FM_TABLES.CREATION_NAME` = «Название пункта в меню "Создать" главной панели SAUMI».  
`CREATION_ROLE` = «Роль, позволяющая создать экземпляр данной сущности».

```sql
select type_id, creation_name, table_name
  from sm.fm_tables
 where subtype=0
 order by use_in_search
```

Рядом в том же месте exe: идентификаторы привилегий `INSERT`/`UPDATE`/`DELETE`/`VIEW` и строки `CREATION_ROLE` / `OnNewItem`.

**Дыра:** `CREATION_ROLE` **нет в SELECT**; как именно `TCheckRole` + `all_tab_privs` отсекают строки — только дизасм `PrepareMenuCreateNewItem`. Пока не фильтруем «наугад» сверх «есть `creation_name`».

### 2.6 «Поиск» — шаблоны

```sql
select st.id as templ_id, st.name as templ_name, st.USER_NAME as USER_NAME,
       1 as ORD, REPORT_ID
  from sm.SEARCHTEMPLATES st
 where st.USER_NAME= :current_user
union
select st.id as templ_id, st.name as templ_name, st.USER_NAME as USER_NAME,
       2 as ORD, REPORT_ID
  from sm.SEARCHTEMPLATES st
 where st.USER_NAME='SM'
 order by ORD, REPORT_ID, templ_name
```

Первый `USER_NAME=` в дампе обрезан — это текущий пользователь (склейка строки).

### 2.7 «История»

Таблица пользователя `SM_USER_HISTORY` (exe создаёт, если нет):

```sql
select * from SM_USER_HISTORY order by seq DESC
```

Колонки: `SEQ, ACCESS_DATE, HANDLER_ID, DATA_ID`. Подписи пунктов — резолв `HANDLER_ID` в коде, не отдельный SQL.

**Дыра:** как `HANDLER_ID` превращается в caption — дизасм `PrepareMenuHistory`.

### 2.8 «Карта»

```sql
select id, name, show_role from sm.sd3_workspaces order by name
```

Видимость кнопки `MapBtn` в DFM False; показ завязан на workspace/`show_role` (точная роль вроде `SM_GIS_MANAGER` рядом в строках — **не доказано** как условие кнопки).

### 2.9 Собственники / документы (не дерево меню, но дергается с панели)

```sql
select c.id, nvl(c.ownership_name, c.name) as ownership_name
  from sm.user_owner_agents a, sm.clients c
 where a.client_id=c.id
```

```sql
select count(*) from sm.documents where EXECUTED='N' and MOVED_USER=USER
```

(`N2Click` — документы на исполнение.)

```sql
select SM.GetUserName, USER from DUAL
```

---

## 3. Что не является меню главной панели

| Объект | Зачем |
|--------|--------|
| `SM.DOCROLES` | роли документа в пакете, не пользователя |
| `InflexActionMenu` | контекст Explorer |
| `MUNREPORT*` | муниципальная отчётность, другие формы |
| `USER_REPORTFORMS` с `Report_ID=` | печать из карточки объекта |

---

## 4. SAUMI-Pascal

Строка exe: `procedure AddMenuItem(Menu:TMenuHandle; MenuItem,Caption:string; Seq:integer; EventProc:TMenuProcEvent; ShortCut:word; Flag:integer)`.  
`TSaumiPascalExtMenuItem`, хук `SP_OnPrepareMenus`. Пункты из скриптов БД **добавляются поверх** DFM/SQL.

**Дыра:** каталог скриптов, которые зовут `AddMenuItem` при старте панели, не снят. Веб это не эмулирует, пока не будет списка вызовов.

---

## 5. Веб-контракт

`GET /api/v1/menu` (JWT): статическое дерево DFM 1:1 + динамические коллекции тем же SQL.  
Пока нет пула Oracle-сессии после login — динамика пустая (мок), статика всё равно с exe. Не подставлять выдуманные captions отчётов/плагинов.
