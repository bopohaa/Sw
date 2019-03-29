using System;
using System.Collections.Generic;
using System.Text;

namespace SwCore.Common
{
    internal struct SwContextMeta
    {
        public readonly IEntrypoint[] Entrypoints;
        public readonly object[] Instances;
        public readonly int[][] ExportIndex;
        public readonly int[][] ImportIndex;
        public readonly int[] ExecuteList;
        public readonly int ExportCnt;

        public SwContextMeta(IEntrypoint[] entrypoints, object[] instances, int[][] exportIndex, int[][] importIndex, int[] executeList, int export_cnt)
        {
            Entrypoints = entrypoints;
            Instances = instances;
            ExportIndex = exportIndex;
            ImportIndex = importIndex;
            ExecuteList = executeList;
            ExportCnt = export_cnt;
        }

        public bool IsEmpty => Entrypoints == null;
    }
}
