"""SQL главной панели — строки из mmp32.exe (docs/menu_flow.md). Не переписывать «на понятнее»."""

SESSION_ROLES = "select ROLE from session_roles"

PLUGINS_LIST = """
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
"""

PLUGIN_BY_ID = """
select
  nvl(pl.need_login,'N') NEEDLOGIN,
  pl.file_name,
  pl.file_extention,
  pl.execute_params,
  nvl(pl.is_dot_net,'N') IS_DOT_NET,
  pl.menu_caption
from sm.plugins pl
where pl.id = :id
"""

REPORTSTREE_ROOT = """
select t.ID, t.NAME, t.GROUPMARK, t.BIGREPORT
  from SM.REPORTSTREE t, session_roles sr
 where t.ROOT=0
   and (
        (t.rolegranted=sr.role and (not t.rolegranted is null))
     or (t.rolegranted is null and sr.role='CONNECT')
       )
 order by SEQN
"""

REPORTSTREE_CHILDREN = """
select t.ID, t.NAME, t.GROUPMARK, t.BIGREPORT
  from SM.REPORTSTREE t, session_roles sr
 where t.ROOT=:parent
   and (
        (t.rolegranted=sr.role and (not t.rolegranted is null))
     or (t.rolegranted is null and sr.role='CONNECT')
       )
 order by SEQN
"""

USER_REPORTSTREE_ROOT = """
select t.ID, t.NAME, t.GROUPMARK, 'Y' ANALITICS
  from SM_USER_REPORTSTREE t
 where t.ROOT is NULL and t.qparams like '%[Analitic]%'
union
select t.ID, t.NAME, t.GROUPMARK, 'N' ANALITICS
  from SM_USER_REPORTSTREE t
 where t.ROOT is NULL
   and (t.qparams not like '%[Analitic]%' or t.qparams is null)
 order by GROUPMARK DESC, Name
"""

USER_REPORTSTREE_CHILDREN = """
select t.ID, t.NAME, t.GROUPMARK, 'Y' ANALITICS
  from SM_USER_REPORTSTREE t
 where t.ROOT=:parent and t.qparams like '%[Analitic]%'
union
select t.ID, t.NAME, t.GROUPMARK, 'N' ANALITICS
  from SM_USER_REPORTSTREE t
 where t.ROOT=:parent
   and (t.qparams not like '%[Analitic]%' or t.qparams is null)
 order by Name
"""

AUTONOMOUS_REPORTFORMS = """
select ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP
  from sm.User_reportforms
 where NAME IS NULL and Report_ID IS NULL
 order by MENU_GROUP DESC, "DESCRIPTION" DESC
"""

CREATE_ITEMS = """
select type_id, creation_name, table_name
  from sm.fm_tables
 where subtype=0
 order by use_in_search
"""

SEARCH_TEMPLATES = """
select st.id as templ_id, st.name as templ_name, st.USER_NAME as USER_NAME,
       1 as ORD, REPORT_ID
  from sm.SEARCHTEMPLATES st
 where st.USER_NAME=:uname
union
select st.id as templ_id, st.name as templ_name, st.USER_NAME as USER_NAME,
       2 as ORD, REPORT_ID
  from sm.SEARCHTEMPLATES st
 where st.USER_NAME='SM'
 order by ORD, REPORT_ID, templ_name
"""

HISTORY = "select * from SM_USER_HISTORY order by seq DESC"

MAP_WORKSPACES = "select id, name, show_role from sm.sd3_workspaces order by name"

OWNER_AGENTS = """
select c.id, nvl(c.ownership_name, c.name) as ownership_name
  from sm.user_owner_agents a, sm.clients c
 where a.client_id=c.id
"""

PENDING_DOCS_COUNT = (
    "select count(*) from sm.documents where EXECUTED='N' and MOVED_USER=USER"
)

GET_USER_NAME = "select SM.GetUserName, USER from DUAL"
