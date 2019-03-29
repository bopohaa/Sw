using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SwCore.Common
{
    internal class InitialEntrypoint : IEntrypoint
    {
        public Type[] Export { get; }

        public Type[] Import => Array.Empty<Type>();

        public Type InstanceType => typeof(InitialEntrypoint);

        public SwComponentPriorityEnum Priority => (SwComponentPriorityEnum) (-1);

        public bool IsAsync => false;

        public InitialEntrypoint(Type[] exports)
        {
            Export = exports;
        }

        public int Execute(object instance, object[] args, object[] output)
        {
            if (args.Length != Export.Length)
                throw new InvalidOperationException("Invalid import");

            for (var i = 0; i < args.Length; ++i)
                output[i] = args[i];
            return args.Length;
        }


        public Task<int> ExecuteAsync(object instance, object[] args, object[] output)
        {
            throw new NotImplementedException();
        }
    }
}
