using System.Collections.Generic;

namespace InflexCore.DataImplementation;

public delegate void BeforeCombineSqlHandler(ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy);
