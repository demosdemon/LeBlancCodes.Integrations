using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LeBlancCodes.Common.Utilities;

namespace LeBlancCodes.Common.Extensions
{
    public interface IMetadata
    {
        IEnumerable<Attribute> Attributes { get; }

        IEnumerable<Attribute> ParentAttributes { get; }

        IEnumerable<Attribute> ChildAttributes { get; }

        IMetadata Parent { get; }

        IEnumerable<IMetadata> Children { get; }
    }

    public abstract class Metadata : IMetadata
    {
        protected internal static IEnumerable<IMetadata> Empty => Enumerable.Empty<IMetadata>();

        public abstract IEnumerable<Attribute> Attributes { get; }

        public virtual IEnumerable<Attribute> ParentAttributes => Parent.Attributes;

        public virtual IEnumerable<Attribute> ChildAttributes => Children.SelectMany(x => x.Attributes);

        public abstract IMetadata Parent { get; }

        public abstract IEnumerable<IMetadata> Children { get; }

        protected static IEnumerable<MethodMetadata> MethodMetas(params MethodInfo[] source) => source.NotNull().Select(MethodMetadata.Create);
    }

    public interface IDataMember
    {
        /// <summary>
        ///     THe type referenced by this member
        /// </summary>
        Type DataType { get; }

        /// <summary>
        ///     True if this member is not bound to an instance
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        ///     True if this member is normally hidden from view
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        ///     True if this member requires index parameters to access its value
        /// </summary>
        bool IsIndexed { get; }

        /// <summary>
        ///     The index parameters, if any
        /// </summary>
        IEnumerable<ParameterMetadata> IndexParameters { get; }

        /// <summary>
        ///     Get the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <returns>The value this member points to</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException"><see cref="IsIndexed" /> is true</exception>
        object GetValue(object instance);

        /// <summary>
        ///     Get the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <param name="indexParameter">the parameter to access the property</param>
        /// <returns>The value this member points to</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException">Member isn't indexed or takes more than one parameter.</exception>
        object GetValue(object instance, object indexParameter);

        /// <summary>
        ///     Get the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <param name="indexParameters">the parameters to access the property</param>
        /// <returns>The value this member points to</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException">Invalid number of index parameters</exception>
        object GetValue(object instance, object[] indexParameters);

        /// <summary>
        ///     Set the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <param name="value">the new member value</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException"><see cref="IsIndexed" /> is true</exception>
        /// <exception cref="InvalidCastException">Invalid type assignment</exception>
        void SetValue(object instance, object value);

        /// <summary>
        ///     Set the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <param name="value">the new member value</param>
        /// <param name="indexParameter">the parameter to access the property</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException">Member isn't indexed or takes more than one parameter.</exception>
        /// <exception cref="InvalidCastException">Invalid type assignment</exception>
        void SetValue(object instance, object value, object indexParameter);

        /// <summary>
        ///     Set the value represented by this member
        /// </summary>
        /// <param name="instance"><code>null</code> if <see cref="IsStatic" /> otherwise an instance of containing this member</param>
        /// <param name="value">the new member value</param>
        /// <param name="indexParameters">the parameters to access the property</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is null and <see cref="IsStatic" /> is false</exception>
        /// <exception cref="InvalidOperationException">Invalid number of index parameters</exception>
        /// <exception cref="InvalidCastException">Invalid type assignment</exception>
        void SetValue(object instance, object value, object[] indexParameters);
    }

    public sealed class PropertyMetadata : Metadata, IDataMember
    {
        private PropertyMetadata(PropertyInfo property) => Property = property;

        public override IEnumerable<Attribute> Attributes => Property.GetCachedAttributes();

        public override IMetadata Parent => TypeMetadata.Create(Property.DeclaringType);

        public override IEnumerable<IMetadata> Children => MethodMetas(GetMethod, SetMethod);

        internal PropertyInfo Property { get; }

        private MethodInfo GetMethod => Property.GetGetMethod(true);

        private MethodInfo SetMethod => Property.GetSetMethod(true);

        public object GetValue(object instance) => GetValue(instance, null);

        public object GetValue(object instance, object indexParameter) => GetValue(instance, new[] {indexParameter});

        public object GetValue(object instance, object[] indexParameters) => MethodMetas(SetMethod).Single().Invoke(instance, indexParameters);

        public void SetValue(object instance, object value) => SetValue(instance, value, null);

        public void SetValue(object instance, object value, object indexParameter) => SetValue(instance, value, new[] {indexParameter});

        public void SetValue(object instance, object value, object[] indexParameters) =>
            MethodMetas(SetMethod).Single().Invoke(instance, indexParameters.EmptyIfNull().Append(value).ToArray());

        public Type DataType => Property.DeclaringType;

        public bool IsStatic => GetMethod?.IsStatic ?? SetMethod?.IsStatic ?? false;

        public bool IsPublic => (GetMethod?.IsPublic ?? false) || (SetMethod?.IsPublic ?? false);

        public bool IsIndexed => IndexParameters.Any();

        public IEnumerable<ParameterMetadata> IndexParameters => (
                GetMethod?.GetParameters().AsEnumerable() ??
                SetMethod?.GetParameters().SkipLast(1)
            )
            .EmptyIfNull()
            .Select(ParameterMetadata.Create);

        internal static PropertyMetadata Create(PropertyInfo property) => new PropertyMetadata(property);
    }

    public sealed class FieldMetadata : Metadata, IDataMember
    {
        private FieldMetadata(FieldInfo field) => Field = field;

        public override IEnumerable<IMetadata> Children => Empty;

        public override IEnumerable<Attribute> Attributes => Field.GetCachedAttributes();

        public override IMetadata Parent => TypeMetadata.Create(Field.DeclaringType);

        internal FieldInfo Field { get; }

        public object GetValue(object instance)
        {
            if (!IsStatic)
                Ensure.NotNull(instance, nameof(instance));

            return Field.GetValue(instance);
        }

        public object GetValue(object instance, object indexParameter) => GetValue(instance);

        public object GetValue(object instance, object[] indexParameters) => GetValue(instance);

        public void SetValue(object instance, object value)
        {
            if (!IsStatic)
                Ensure.NotNull(instance, nameof(instance));

            if (!DataType.IsNullable())
                Ensure.NotNull(value, nameof(value));

            Field.SetValue(instance, value);
        }

        public void SetValue(object instance, object value, object indexParameter) => SetValue(instance, value);

        public void SetValue(object instance, object value, object[] indexParameters) => SetValue(instance, value);

        public Type DataType => Field.FieldType;

        public bool IsStatic => Field.IsStatic;

        public bool IsPublic => Field.IsPublic;

        public bool IsIndexed => false;

        public IEnumerable<ParameterMetadata> IndexParameters => Empty.Cast<ParameterMetadata>();

        internal static FieldMetadata Create(FieldInfo field) => new FieldMetadata(field);
    }

    public sealed class ConstructorMetadata : MethodMetadata
    {
        private ConstructorMetadata(MethodBase ctor) : base(ctor)
        {
        }

        public bool IsDefault => !Children.Any();

        internal static ConstructorMetadata Create(ConstructorInfo ctor) => new ConstructorMetadata(ctor);
    }

    public sealed class EventMetadata : Metadata
    {
        private EventMetadata(EventInfo @event) => Event = @event;

        public override IMetadata Parent => TypeMetadata.Create(Event.DeclaringType);

        public override IEnumerable<Attribute> Attributes => Event.GetCachedAttributes();

        public override IEnumerable<IMetadata> Children => MethodMetas(AddMethod, RemoveMethod, RaiseMethod).Concat(MethodMetas(OtherMethods));

        public Type DelegateType => Event.EventHandlerType;

        internal EventInfo Event { get; }

        internal MethodInfo AddMethod => Event.GetAddMethod(true);

        internal MethodInfo RemoveMethod => Event.GetRemoveMethod(true);

        internal MethodInfo RaiseMethod => Event.GetRaiseMethod(true);

        internal MethodInfo[] OtherMethods => Event.GetOtherMethods(true) ?? Array.Empty<MethodInfo>();

        public void AddHandler(object instance, Delegate handler)
        {
            var method = MethodMetadata.Create(Ensure.NotNull(AddMethod, nameof(AddMethod)));
            handler = (Delegate) Ensure.IsInstanceOfType(handler, DelegateType, nameof(handler));
            method.Invoke(instance, handler);
        }

        public void RemoveHandler(object instance, Delegate handler)
        {
            var method = MethodMetadata.Create(Ensure.NotNull(RemoveMethod, nameof(RemoveMethod)));
            handler = (Delegate) Ensure.IsInstanceOfType(handler, DelegateType, nameof(handler));
            method.Invoke(instance, handler);
        }

        // `RaiseMethod` always returns null for some reason, so no point in implementing this if it doesn't work.
        // Need to figure out another way of raising the event handlers using reflection
        public void InvokeHandler(object instance, params object[] args) => throw new NotImplementedException();

        internal static EventMetadata Create(EventInfo @event) => new EventMetadata(@event);
    }

    public sealed class TypeMetadata : Metadata
    {
        private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private TypeMetadata(Type type) => Type = type;

        public bool IsGenericDefinition => Type.IsGenericTypeDefinition;

        public bool IsConstructedGeneric => Type.IsConstructedGenericType;

        public IEnumerable<TypeMetadata> GenericTypeArguments => Type.GetGenericArguments().Select(Create);

        public TypeMetadata GenericTypeDefinition => IsGenericDefinition ? this : (IsConstructedGeneric ? Create(Type.GetGenericTypeDefinition()) : null);

        public override IEnumerable<Attribute> Attributes => Type.GetCachedAttributes();

        public IEnumerable<ConstructorMetadata> Constructors => Type.GetConstructors(Flags).Select(ConstructorMetadata.Create);

        public IEnumerable<EventMetadata> Events => Type.GetEvents(Flags).Select(EventMetadata.Create);

        public IEnumerable<PropertyMetadata> Properties => Type.GetProperties(Flags).Select(PropertyMetadata.Create);

        public IEnumerable<FieldMetadata> Fields => Type.GetFields(Flags).Select(FieldMetadata.Create);

        public IEnumerable<MethodMetadata> Methods => Type.GetMethods(Flags).Select(MethodMetadata.Create);

        public override IMetadata Parent => new AssemblyMetadata(Type.Assembly);

        public override IEnumerable<IMetadata> Children => Empty.Chain(Constructors, Events, Properties, Fields, Methods);

        internal Type Type { get; }

        internal static TypeMetadata Create(Type type) => new TypeMetadata(type);
    }

    public class MethodMetadata : Metadata
    {
        protected MethodMetadata(MethodBase method) => Method = method;

        public override IEnumerable<Attribute> Attributes => Method.GetCachedAttributes();

        public override IMetadata Parent => TypeMetadata.Create(Method.DeclaringType);

        public override IEnumerable<IMetadata> Children => Method.GetParameters().Select(ParameterMetadata.Create);

        internal MethodBase Method { get; }

        public object Invoke(object instance, params object[] arguments)
        {
            if (!Method.IsStatic)
                Ensure.NotNull(instance, nameof(instance));

            arguments = Ensure.ArgumentsBind(Method.GetParameters(), arguments).ToArray();

            var returnValue = Method.Invoke(instance, arguments);

            return returnValue;
        }

        internal static MethodMetadata Create(MethodBase method) => new MethodMetadata(method);
    }

    public class ParameterMetadata : Metadata
    {
        private ParameterMetadata(ParameterInfo parameter) => Parameter = parameter;

        public override IMetadata Parent => MethodMetadata.Create(Ensure.NotNull(Parameter.Member as MethodBase, nameof(Parent)));

        public override IEnumerable<IMetadata> Children => Empty;

        public override IEnumerable<Attribute> Attributes => Parameter.GetCachedAttributes();

        internal ParameterInfo Parameter { get; }

        public static ParameterMetadata Create(ParameterInfo parameter) => new ParameterMetadata(parameter);
    }

    public class AssemblyMetadata : Metadata
    {
        private readonly Lazy<IReadOnlyCollection<IMetadata>> _children;

        internal AssemblyMetadata(Assembly assembly)
        {
            Assembly = assembly;
            _children = new Lazy<IReadOnlyCollection<IMetadata>>(() => assembly.GetTypes().Select(TypeMetadata.Create).ToImmutableList());
        }

        public override IMetadata Parent => null;

        public override IEnumerable<IMetadata> Children => _children.Value;

        public override IEnumerable<Attribute> Attributes => Assembly.GetCachedAttributes();

        internal Assembly Assembly { get; }
    }

    public static class AttributeProvider
    {
        private static readonly ThreadSafeStore<ICustomAttributeProvider, Attribute[]> Cache =
            new ThreadSafeStore<ICustomAttributeProvider, Attribute[]>(GetAttributesImpl);

        public static Attribute[] GetCachedAttributes(this ICustomAttributeProvider type) => Cache.Get(Ensure.NotNull(type, nameof(type)));

        public static IEnumerable<Attribute> GetAttributes<T>(Expression<Func<T>> attributeProviderExpression)
        {
            Ensure.NotNull(attributeProviderExpression, nameof(attributeProviderExpression));

            return GetAttributes(attributeProviderExpression.Body);
        }

        private static Attribute[] GetAttributesImpl(ICustomAttributeProvider type) =>
            Ensure.NotNull(type, nameof(type)).GetCustomAttributes(true).Cast<Attribute>().ToArray();

        private static IEnumerable<Attribute> GetAttributes(Expression expression)
        {
            switch (Unwrap(expression))
            {
                case null:
                    return Enumerable.Empty<Attribute>();
                case MemberExpression memberExpression:
                    return GetAttributes(memberExpression);
                case NewExpression newExpression:
                    return GetAttributes(newExpression);
                case ConstantExpression constantExpression:
                    return GetAttributes(constantExpression);
                case MethodCallExpression methodCallExpression:
                    return GetAttributes(methodCallExpression);
            }

            throw new ArgumentOutOfRangeException(nameof(expression), expression.NodeType, "Unexpected expression");
        }

        private static Expression Unwrap(Expression expression)
        {
            var current = expression;
            while (current.NodeType == ExpressionType.Convert)
                current = ((UnaryExpression) current).Operand;
            return current;
        }

        private static IEnumerable<Attribute> GetAttributes(MemberExpression expression) =>
            GetCachedAttributes(expression.Member).Concat(GetAttributes(expression.Expression));

        private static IEnumerable<Attribute> GetAttributes(NewExpression expression) =>
            GetCachedAttributes(expression.Constructor).Concat(GetCachedAttributes(expression.Type));

        private static IEnumerable<Attribute> GetAttributes(ConstantExpression expression) =>
            expression.Value is ICustomAttributeProvider provider ? GetCachedAttributes(provider) : GetCachedAttributes(expression.Type);

        private static IEnumerable<Attribute> GetAttributes(MethodCallExpression expression) =>
            GetCachedAttributes(expression.Method).Concat(GetAttributes(expression.Object));
    }

    public static class ReflectionExtensions
    {
        public static bool IsNullable(this Type type)
        {
            Ensure.NotNull(type, nameof(type));

            return !type.IsValueType || IsNullableType(type);
        }

        public static bool IsNullableType(this Type type)
        {
            Ensure.NotNull(type, nameof(type));
            return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetCollectionItemType(this Type type)
        {
            Ensure.NotNull(type, nameof(type));

            if (type.IsArray)
                return type.GetElementType();

            var openEnumerable = typeof(IEnumerable<>);
            if (type.ImplementsGenericDefinition(openEnumerable, out var implementingType))
            {
                if (!implementingType.IsGenericTypeDefinition)
                    return implementingType.GetGenericArguments().Single();
            }

            if (type != openEnumerable && typeof(IEnumerable).IsAssignableFrom(type))
                return null;

            throw new ArgumentException($"{type.Name} is not a collection type.");
        }

        public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            Ensure.NotNull(type, nameof(type));
            Ensure.NotNull(genericInterfaceDefinition, nameof(genericInterfaceDefinition));

            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("Not a generic interface definition", nameof(genericInterfaceDefinition));

            if (type.IsInterface && type.IsGenericType)
            {
                var def = type.GetGenericTypeDefinition();
                if (def == genericInterfaceDefinition)
                {
                    implementingType = type;
                    return true;
                }
            }

            var i = type.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceDefinition);
            if (i != null)
            {
                implementingType = i;
                return true;
            }

            implementingType = null;
            return false;
        }

        public static string FriendlyName(this Type type)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var reference = new CodeTypeReference(type);

            return provider.GetTypeOutput(reference);
        }
    }
}
