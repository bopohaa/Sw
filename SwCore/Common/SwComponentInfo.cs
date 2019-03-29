using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwCore.Common
{
    internal class SwComponentInfo
    {
        public IEntrypoint Entrypoint { get; private set; }

        public Type Export => Entrypoint.InstanceType;
        public Type[] Import { get; private set; }
        public SwComponentPriorityEnum Priority { get; set; }


        public SwComponentInfo(SwComponentPriorityEnum priority, IEntrypoint entrypoint)
        {
            Priority = priority;
            Init(entrypoint);
        }

        private void Init(IEntrypoint entrypoint)
        {
            Entrypoint = entrypoint;
            var constructor = Export.GetConstructors().Single();
            Import = constructor.GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType).ToArray();
        }
    }
}
