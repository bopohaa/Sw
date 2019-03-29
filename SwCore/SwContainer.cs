using SwCore.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SwCore
{
    public class SwContainer
    {
        private readonly Func<SwContext> _factory;

        internal SwContainer(Func<SwContext> factory)
        {
            _factory = factory;
        }

        public Task Run()
        {
            var context = _factory();
            return context.Run();
        }
    }

    public class SwContainer<T>
    {
        private readonly Func<SwContext> _factory;
        private readonly object[] _initialImports;
        private readonly FieldInfo[] _tupleFields;

        internal SwContainer(Func<SwContext> factory, FieldInfo[] tuple_fields)
        {
            _factory = factory;
            _tupleFields = tuple_fields;

            _initialImports = new object[_tupleFields?.Length ?? 1];
        }

        public Task Run(T initial_imports)
        {
            var context = _factory();
            if (_tupleFields != null)
                for (int i = 0; i < _tupleFields.Length; ++i)
                    _initialImports[i] = _tupleFields[i].GetValue(initial_imports);
            else
                _initialImports[0] = initial_imports;
            return context.Run(_initialImports);
        }
    }
}