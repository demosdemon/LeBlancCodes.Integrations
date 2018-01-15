using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using LeBlancCodes.Common.Extensions;
using LeBlancCodes.Common.Utilities.Constraints;

namespace LeBlancCodes.Common.Utilities
{
    public class ConstraintBuilder : IResolveConstraint
    {
        public IConstraint Resolve() => throw new NotImplementedException();
    }

    public static class Ensure
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>(T value, [InvokerParameterName] string name) where T : class
        {
            if (value == null) throw new ArgumentNullException(NotNull(name, nameof(name)));
            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T NotNullOrWhitespace<T>(T value, [InvokerParameterName] string name) where T : class
        {
            if (!(value is string))
                return NotNull(value, name);

            if (string.IsNullOrWhiteSpace(value as string)) throw new ArgumentNullException(NotNull(name, nameof(name)));
            return value;
        }

        [ContractAnnotation("source:null => notnull")]
        public static T[] Contains<T>(T[] source, int elements)
        {
            if (source == null)
                source = Array.Empty<T>();

            if (source.Length == elements)
                return source;

            throw new ArgumentOutOfRangeException(nameof(source), source.Length, "Incorrent number of elements.");
        }

        public static IEnumerable<object> ArgumentsBind(IEnumerable<ParameterInfo> parameters, IEnumerable<object> arguments)
        {
            var ll = new LinkedList<object>();
            using (var param = parameters.GetEnumerator())
            using (var arg = arguments.GetEnumerator())
            {
                while (param.MoveNext())
                {
                    object currentParam;

                    if (IsParams(param.Current))
                        currentParam = DrainParams(param.Current, arg);
                    else if (arg.MoveNext())
                        currentParam = arg.Current;
                    else if (param.Current.HasDefaultValue)
                        currentParam = param.Current.DefaultValue;
                    else if (param.Current.IsOptional)
                        currentParam = null;
                    else
                        throw new ParameterBindingException(param.Current, "undefined");

                    if (currentParam == null && param.Current.ParameterType.IsValueType)
                        throw new ParameterBindingException(param.Current);

                    if (currentParam != null && param.Current.ParameterType.IsInstanceOfType(currentParam))
                        ll.AddLast(currentParam);
                    else
                        throw new ParameterBindingException(param.Current, currentParam);
                }
            }

            return ll.ToImmutableArray();
        }

        public static Type NonNullableType(this Type type)
        {
            NotNull(type, nameof(type));
            return type.IsNullableType() ? type.GetGenericArguments().Single() : type;
        }

        public static object IsInstanceOfType(object instance, Type type, [InvokerParameterName] string name)
        {
            if (NotNull(type, nameof(type)).IsInstanceOfType(NotNull(instance, name)))
                return instance;

            throw new ArgumentException($"Argument is not a {type.FriendlyName()}.", name);
        }

        public static T IsInstanceOfType<T>(object instance, [InvokerParameterName] string name) => (T) IsInstanceOfType(instance, typeof(T), name);

        // ReSharper disable once SuggestBaseTypeForParameter
        private static bool IsParams(ParameterInfo parameter) =>
            NotNull(parameter, nameof(parameter)).GetCachedAttributes().OfType<ParamArrayAttribute>().Any();

        private static object[] DrainParams(ParameterInfo parameter, IEnumerator<object> enumerator)
        {
            if (!IsParams(parameter))
                throw new ArgumentException("Invalid parameter", nameof(parameter));

            var ll = new LinkedList<object>();
            while (enumerator.MoveNext())
                ll.AddLast(enumerator.Current);

            if (ll.Count == 1 && parameter.ParameterType.IsInstanceOfType(ll.First.Value))
                return (object[]) ll.First.Value;

            return ll.ToArray();
        }
    }

    [Serializable]
    public class ParameterBindingException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ParameterBindingException(ParameterInfo parameter, object actualValue = null, Exception innerException = null)
            : base("The argument does not bind to the parameter.", innerException)
        {
            Parameter = parameter;
            ActualValue = actualValue;
        }

        public ParameterBindingException(string message) : base(message)
        {
        }

        public ParameterBindingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ParameterBindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public ParameterInfo Parameter
        {
            get => (ParameterInfo) Data[nameof(Parameter)];
            private set => Data[nameof(Parameter)] = value;
        }

        public object ActualValue
        {
            get => Data[nameof(ActualValue)];
            set => Data[nameof(ActualValue)] = value;
        }

        public override string Message =>
            Parameter == null ? base.Message : $"The argument does not bind to the parameter {Describe(Parameter)}, actual value: {ActualValue}";

        public static string Describe(ParameterInfo parameter)
        {
            var sb = new StringBuilder();
            sb.Append(parameter.ParameterType.FriendlyName());
            sb.Append(" ");
            sb.Append(parameter.Name);
            if (parameter.HasDefaultValue)
            {
                sb.Append(" = ");
                sb.Append(parameter.DefaultValue);
            }

            var result = sb.ToString();
            if (parameter.IsOptional)
                result = $"[{result}]";

            return result;
        }
    }
}
