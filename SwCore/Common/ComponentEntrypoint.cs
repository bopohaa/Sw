using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SwCore.Common
{
    internal class ComponentEntrypoint : IEntrypoint
    {
        private MethodInfo _method;
        private PropertyInfo _taskResultPoperty;
        private FieldInfo[] _tupleFields;
        public Type[] Export { get; }

        public Type[] Import { get; }

        public Type InstanceType { get; }

        public SwComponentPriorityEnum Priority { get; }

        public bool IsAsync { get; }

        private ComponentEntrypoint(MethodInfo method, Type[] imports, Type[] exports, PropertyInfo task_result_prop, FieldInfo[] tuple_fields, SwComponentPriorityEnum priority, bool is_async)
        {
            _method = method;
            _taskResultPoperty = task_result_prop;
            _tupleFields = tuple_fields;
            InstanceType = method.DeclaringType;

            Import = imports;
            Export = exports;
            Priority = priority;
            IsAsync = is_async;
        }

        public int Execute(object instance, object[] args, object[] output)
        {
            if (args.Length != Import.Length)
                throw new InvalidOperationException("Invalid args count");

            var result = _method.Invoke(instance, args);

            return GetResult(result, output);
        }

        public async Task<int> ExecuteAsync(object instance, object[] args, object[] output)
        {
            if (args.Length != Import.Length)
                throw new InvalidOperationException("Invalid args count");

            var task = (Task)_method.Invoke(instance, args);
            await task;

            return GetResult(_taskResultPoperty?.GetValue(task), output);
        }

        private static Type[] _valueTupleTypes = new[] { typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>), typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>) };
        public static ComponentEntrypoint Create(MethodInfo method, SwComponentPriorityEnum priority)
        {
            var param = method.GetParameters();
            var export = method.ReturnType == typeof(void) ? null : method.ReturnType;
            var isAsync = export != null && (export == typeof(Task) || export.IsSubclassOf(typeof(Task)));
            export = isAsync ? export?.GenericTypeArguments?.FirstOrDefault() : export;

            var imports = param.Select(p => p.ParameterType).ToArray();
            var isMultipleRet = export?.IsGenericType ?? false ? _valueTupleTypes.Contains(export.GetGenericTypeDefinition()) : false;
            var exports = isMultipleRet ? export.GenericTypeArguments : export != null ? new[] { export } : Array.Empty<Type>();

            var resultProperty = isAsync && export != null ? typeof(Task<>).MakeGenericType(method.ReturnType.GenericTypeArguments.Single()).GetProperty("Result") : null;
            var tupleFields = isMultipleRet ? export.GetFields() : null;

            return new ComponentEntrypoint(method, imports, exports, resultProperty, tupleFields, priority, isAsync);
        }

        private int GetResult(object result, object[] output)
        {
            if (Export.Length == 0)
                return 0;
            if (Export.Length == 1 && _tupleFields == null)
            {
                output[0] = result;
                return 1;
            }

            for (int i = 0; i < _tupleFields.Length; ++i)
                output[i] = _tupleFields[i].GetValue(result);

            return _tupleFields.Length;
        }
    }
}
