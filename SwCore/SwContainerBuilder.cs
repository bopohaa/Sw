using SwCore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SwCore
{
    public abstract class SwContainerBuilderBase
    {
        private List<Assembly> _assemblies;
        private IResolver _resolver;

        public SwContainerBuilderBase()
        {
            _assemblies = new List<Assembly>();
        }

        public SwContainerBuilderBase AddAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);

            return this;
        }

        public SwContainerBuilderBase RegisterExternalIoc(IResolver resolver)
        {
            _resolver = resolver;

            return this;
        }

        internal SwContextBuilder Build(Type[] initial_imports)
        {
            var contextBuilder = new SwContextBuilder();

            var ft = typeof(SwComponentAttribute);
            var components = _assemblies.SelectMany(a => a.GetTypes()
                    .Where(t => !t.IsInterface && !t.IsAbstract && t.CustomAttributes.Any(att => att.AttributeType == ft)));

            foreach (var component in components)
            {
                var infoAtt = component.GetCustomAttributes<SwComponentAttribute>().First();
                var methodRun = component.GetMethod("Run");
                contextBuilder.AddEntrypointMethod(methodRun, infoAtt.Priority);
            }
            contextBuilder
                .RegisterInitialImports(initial_imports)
                .CreateInstances(_resolver);

            return contextBuilder;
        }
    }

    public class SwContainerBuilder : SwContainerBuilderBase
    {
        public SwContainer Build()
        {
            var contextBuilder = Build(Array.Empty<Type>());
            return new SwContainer(() => contextBuilder.Build());
        }
    }

    public class SwContainerBuilder<T> : SwContainerBuilderBase
    {
        private static Type[] _valueTupleTypes = new[] { typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>), typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>) };

        public SwContainer<T> Build()
        {
            var import = typeof(T);
            var isMultipleRet = import.IsGenericType ? _valueTupleTypes.Contains(import.GetGenericTypeDefinition()) : false;
            var tupleFields = isMultipleRet ? import.GetFields() : null;

            var contextBuilder = Build(tupleFields?.Select(f => f.FieldType).ToArray() ?? new[] { import });
            return new SwContainer<T>(() => contextBuilder.Build(), tupleFields);
        }
    }
}
