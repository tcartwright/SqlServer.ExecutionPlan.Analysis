using System;
using System.Collections.Generic;
using System.Text;

namespace SqlServer.ExecutionPlan.Analysis.Clr
{
	internal static class Globals
	{
		public static readonly string GetIndexDetails;

		static Globals()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("SELECT si.Name, MAX(ddps.row_count) as [RowCount]");
			sb.AppendLine("FROM [{DB}].sys.objects so");
			sb.AppendLine("INNER JOIN [{DB}].sys.indexes si ");
			sb.AppendLine("    ON si.OBJECT_ID = so.OBJECT_ID");
			sb.AppendLine("INNER JOIN [{DB}].sys.dm_db_partition_stats AS ddps ");
			sb.AppendLine("    ON si.OBJECT_ID = ddps.OBJECT_ID  ");
			sb.AppendLine("    AND si.index_id = ddps.index_id");
			sb.AppendLine("WHERE si.index_id < 2  ");
			sb.AppendLine("    AND si.[name] in ({names})");
			sb.AppendLine("    AND so.is_ms_shipped = 0");
			sb.AppendLine("GROUP BY si.Name, ddps.row_count");
			sb.AppendLine("ORDER BY ddps.row_count DESC");

			GetIndexDetails = sb.ToString();
		}
	}
}
