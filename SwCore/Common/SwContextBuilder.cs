using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SwCore.Common
{
    internal class SwContextBuilder
    {
        private readonly List<IEntrypoint> _entrypoints;
        private readonly List<object> _instances;
        private readonly List<int[]> _exportIndex;
        private readonly List<int[]> _importIndex;
        private readonly List<int> _executeList;
        private int _exportCnt;
        private SwContextMeta _contextmeta;


        public SwContextBuilder()
        {
            _entrypoints = new List<IEntrypoint>();
            _instances = new List<object>();
            _exportIndex = new List<int[]>();
            _importIndex = new List<int[]>();
            _executeList = new List<int>();
        }

        public SwContextBuilder AddEntrypointMethod(MethodInfo method, SwComponentPriorityEnum priority)
        {
            if (_instances.Count > 0)
                throw new InvalidOperationException("Already created");

            var entrypoint = ComponentEntrypoint.Create(method, priority);
            _entrypoints.Add(entrypoint);
            return this;
        }

        public SwContextBuilder RegisterInitialImports(Type[] intial_imports)
        {
            if (_entrypoints.Count>0 && _entrypoints[0] is InitialEntrypoint)
                throw new InvalidOperationException("Initial imports already defined");

            if (intial_imports?.Length > 0)
                _entrypoints.Insert(0, new InitialEntrypoint(intial_imports));

            return this;
        }

        public SwContextBuilder CreateInstances(IResolver resolver)
        {
            if (_instances.Count > 0)
                throw new InvalidOperationException("Already created");

            var exportTypes = new List<Type>();

            foreach (var entryPoint in _entrypoints)
            {
                object instance;
                if (entryPoint is InitialEntrypoint)
                    instance = null;
                else
                {
                    var constructor = entryPoint.InstanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
                    var parameters = constructor.GetParameters();
                    var constructArgs = parameters.Select(p => resolver.Resolve(p.ParameterType)).ToArray();
                    instance = constructor.Invoke(constructArgs);
                }
                _instances.Add(instance);

                var exportIndex = GetExportIndex(exportTypes, entryPoint.Export, entryPoint.InstanceType);
                _exportIndex.Add(exportIndex);
            }

            _exportCnt = exportTypes.Count;

            foreach (var entryPoint in _entrypoints)
            {
                var importIndex = GetImportIndex(exportTypes, entryPoint.Import, entryPoint.InstanceType);
                _importIndex.Add(importIndex);
            }

            CheckUseExports(exportTypes, _importIndex, _exportIndex, _entrypoints);

            foreach (var group in _entrypoints.GroupBy(e => e.Priority).OrderBy(g => g.Key))
                foreach (var entry in group)
                {
                    var idx = _entrypoints.FindIndex(e => e == entry);
                    if (_executeList.Contains(idx))
                        continue;

                    ResolveDependencyRecurse(idx, new Stack<int>(), exportTypes);
                }

            return this;
        }

        public SwContext Build()
        {
            if (_instances.Count == 0)
                throw new InvalidOperationException("Is not a created");

            if (_contextmeta.IsEmpty)
                _contextmeta = new SwContextMeta(_entrypoints.ToArray(), _instances.ToArray(), _exportIndex.ToArray(), _importIndex.ToArray(), _executeList.ToArray(), _exportCnt);

            return new SwContext(_contextmeta);
        }

        private void ResolveDependencyRecurse(int idx, Stack<int> parent, List<Type> types)
        {
            parent.Push(idx);

            foreach (var dep in _importIndex[idx])
            {
                var depIdx = _exportIndex.FindIndex(e => e.Contains(dep));
                if (depIdx < 0)
                    throw new InvalidOperationException($"This type '{ types[dep] }' is not exported in any component");

                if (_executeList.Contains(depIdx))
                    continue;

                if (parent.Contains(depIdx))
                    throw new InvalidOperationException($"Circular dependency detected, for {string.Join("->", parent.Select(i => _entrypoints[i].ToString()))}");

                ResolveDependencyRecurse(depIdx, parent, types);
            }

            _executeList.Add(idx);

            parent.Pop();
        }

        private static void CheckUseExports(List<Type> exportTypes, List<int[]> import_index, List<int[]> export_index, List<IEntrypoint> entrypoints)
        {
            var imports = import_index.SelectMany(e => e).Distinct().ToArray();
            var notusedexports = export_index.SelectMany(e => e).Except(imports).ToArray();
            if (notusedexports.Length > 0)
            {
                var badComponents = new StringBuilder("Find unused export(s) value in type(s):");
                badComponents.AppendLine();
                foreach (var export in notusedexports)
                {
                    var idx = export_index.FindIndex(e => e.Contains(export));
                    if (idx < 0) continue;
                    badComponents.Append('(');
                    badComponents.Append(string.Join(", ", export_index[idx].Intersect(notusedexports).Select(e => exportTypes[e].ToString())));
                    badComponents.Append(')');
                    badComponents.AppendLine(entrypoints[idx].InstanceType.ToString());
                }
                throw new InvalidOperationException(badComponents.ToString());
            }
        }

        private static int[] GetExportIndex(List<Type> exportTypes, Type[] export, Type component_type)
        {
            if (export == null)
                return Array.Empty<int>();

            var index = new int[export.Length];
            for (var i = 0; i < index.Length; i++)
            {
                if (exportTypes.Contains(export[i]))
                    throw new InvalidOperationException($"This type '({ export[i] }){component_type}' already exported in other component");

                var idx = exportTypes.Count;
                exportTypes.Add(export[i]);

                index[i] = idx;
            }

            return index;
        }

        private static int[] GetImportIndex(List<Type> exportTypes, Type[] import, Type component_type)
        {
            var index = new int[import.Length];
            for (var i = 0; i < index.Length; i++)
            {
                var idx = exportTypes.FindIndex(t => t == import[i]);
                if (idx < 0)
                    throw new IndexOutOfRangeException($"Import type '{component_type}({import[i]})' is not defined as exported");

                index[i] = idx;
            }

            return index;
        }
    }
}
