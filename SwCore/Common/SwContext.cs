using System;
using System.Threading.Tasks;

namespace SwCore.Common
{
    internal class SwContext
    {
        private readonly SwContextMeta _meta;
        private readonly object[] _exportInstances;

        public SwContext(SwContextMeta meta)
        {
            _meta = meta;
            _exportInstances = new object[_meta.ExportCnt];
        }

        public async Task Run(params object[] initial_imports)
        {
            var output = new object[256];


            foreach (var idx in _meta.ExecuteList)
            {
                var entry = _meta.Entrypoints[idx];
                var instance = _meta.Instances[idx];
                var inputArgs = _meta.ImportIndex[idx];
                var input = instance == null ? initial_imports : new object[inputArgs.Length];
                for (int i = 0; i < inputArgs.Length; ++i)
                    input[i] = _exportInstances[inputArgs[i]];

                var outputCnt = entry.IsAsync ?
                    await entry.ExecuteAsync(instance, input, output) :
                    entry.Execute(instance, input, output);

                var outputArgs = _meta.ExportIndex[idx];
                if (outputCnt != outputArgs.Length)
                    throw new InvalidOperationException("Invalid result");

                for (int i = 0; i < outputCnt; ++i)
                    _exportInstances[outputArgs[i]] = output[i];
            }
        }
    }
}