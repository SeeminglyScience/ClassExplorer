using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using static ClassExplorer.FilterFrame<System.Reflection.MemberInfo>;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The Find-Member cmdlet searches the current AppDomain for matching members.
    /// </summary>
    [OutputType(typeof(MemberInfo))]
    [Cmdlet(VerbsCommon.Find, "Member")]
    public class FindMemberCommand : FindReflectionObjectCommandBase<MemberInfo>
    {
        private BindingFlags _flags;

        private MemberTypes? _memberTypes;

        private List<Type> _processedTypes = new List<Type>();

        /// <summary>
        /// Gets or sets the parameter type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the return, property, or field type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type ReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include special name members.
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeSpecialName { get; set; }

        /// <summary>
        /// Gets or sets the member type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [Alias("MT")]
        public MemberTypes MemberType
        {
            get
            {
                return _memberTypes == null
                    ? MemberTypes.All
                    : _memberTypes.Value;
            }

            set
            {
                _memberTypes = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match static members.
        /// </summary>
        [Parameter]
        public SwitchParameter Static { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only match instance members.
        /// </summary>
        [Parameter]
        public SwitchParameter Instance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only match abstract members.
        /// </summary>
        [Parameter]
        public SwitchParameter Abstract { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only match virtual members.
        /// </summary>
        [Parameter]
        public SwitchParameter Virtual { get; set; }

        /// <summary>
        /// The EndProcessing method.
        /// </summary>
        protected override void EndProcessing()
        {
            if (HasHadInput) return;

            WriteObject(
                new FindTypeCommand().Invoke<Type>()
                    .Where(type => Force.IsPresent || type.IsPublic)
                    .SelectMany(
                        type =>
                        {
                            return type.FindMembers(
                                MemberType,
                                _flags,
                                AggregateFilter,
                                null);
                        }),
                enumerateCollection: true);
        }

        /// <summary>
        /// Process a object from the pipeline. Filter passed members and get members for passed types
        /// and the types of any other type of object passed.
        /// </summary>
        /// <param name="input">The input from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            if (input.BaseObject is Type type)
            {
                ProcessSingleType(type);
                return;
            }

            if (input.BaseObject is MemberInfo member)
            {
                // Filtering by binding flags and member type is done with arguments to Type.FindMembers
                // instead of the AggregateFilter so for individual members we need a proxy method.
                FindMemberProxy(member);
                return;
            }

            var targetType = input.BaseObject.GetType();
            if (_processedTypes.Contains(targetType)) return;

            _processedTypes.Add(targetType);
            ProcessSingleType(targetType);
        }

        /// <summary>
        /// Process input parameters and create the filter set.
        /// </summary>
        protected override void InitializeFilters()
        {
            _flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            if (Force.IsPresent)
            {
                _flags |= BindingFlags.NonPublic;
            }

            if (Static.IsPresent && !Instance.IsPresent)
            {
                _flags &= ~BindingFlags.Instance;
            }

            if (Instance.IsPresent && !Static.IsPresent)
            {
                _flags &= ~BindingFlags.Static;
            }

            ProcessParameter(SpecialNameFilter, !IncludeSpecialName.IsPresent);
            ProcessName(Name, WildcardNameFilter, RegexNameFilter);
            ProcessParameter<Type>(ParameterTypeFilter, ParameterType);
            ProcessParameter<Type>(ReturnTypeFilter, ReturnType);
            ProcessParameter((m, fc) => m is MethodInfo method ? method.IsAbstract : false, Abstract.IsPresent);
            ProcessParameter((m, fc) => m is MethodInfo method ? method.IsVirtual : false, Virtual.IsPresent);

            if (FilterScript != null)
            {
                var filter = new ReflectionFilter(
                    (type, criteria) =>
                    {
                        return LanguagePrimitives.IsTrue(
                            FilterScript.InvokeWithContext(
                                null,
                                new List<PSVariable>() { new PSVariable("_", type) },
                                type));
                    });
                ProcessParameter(filter, true);
            }
        }

        /// <summary>
        /// Not all member types have a property for this, and it's controlled by BindingFlags on
        /// Type.FindMembers anyway.
        /// </summary>
        /// <param name="m">The parameter is not used.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>N/A</returns>
        protected override bool PublicFilter(MemberInfo m, object filterCriteria)
        {
            throw new NotSupportedException();
        }

        private static bool ParameterTypeFilter(MemberInfo m, object filterCriteria)
        {
            if (m is MethodBase method && filterCriteria is Type parameterType)
            {
                return method
                    .GetParameters()
                    .Any(p => IsOfTypeOrElementType(p.ParameterType, parameterType));
            }

            return false;
        }

        private static bool ReturnTypeFilter(MemberInfo m, object filterCriteria)
        {
            var targetType = filterCriteria as Type;
            if (targetType == null) return false;

            if (m is PropertyInfo property)
            {
                return IsOfTypeOrElementType(property.PropertyType, targetType);
            }

            if (m is MethodInfo method)
            {
                return IsOfTypeOrElementType(method.ReturnType, targetType);
            }

            if (m is ConstructorInfo constructor)
            {
                return IsOfTypeOrElementType(constructor.ReflectedType, targetType);
            }

            if (m is FieldInfo field)
            {
                return IsOfTypeOrElementType(field.FieldType, targetType);
            }

            return false;
        }

        private static bool IsOfTypeOrElementType(Type sourceType, Type targetType)
        {
            if (IsTypeCompatible(sourceType, targetType)) return true;

            if (HasElement(sourceType))
            {
                Type newSource = sourceType;
                while (HasElement(newSource) && ShouldUnwrap(newSource, targetType))
                {
                    newSource = newSource.GetElementType();
                }

                return IsTypeCompatible(newSource, targetType);
            }

            if (sourceType.IsGenericType)
            {
                return sourceType
                    .GetGenericArguments()
                    .Any(type => IsTypeCompatible(sourceType, targetType));
            }

            return false;
        }

        private static bool IsTypeCompatible(Type sourceType, Type targetType)
        {
            // While technically true Object is compatible it's not super helpful.
            if (sourceType == typeof(object))
            {
                return false;
            }

            if (targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (targetType.IsSubclassOf(sourceType))
            {
                return true;
            }

            return false;
        }

        private static bool ShouldUnwrap(Type sourceType, Type targetType)
        {
            return (sourceType.IsArray && !targetType.IsArray) ||
                    sourceType.IsPointer ||
                    sourceType.IsByRef;
        }

        private static bool HasElement(Type type)
        {
            return type.IsArray || type.IsPointer || type.IsByRef;
        }

        private static bool SpecialNameFilter(MemberInfo m, object filterCriteria)
        {
            return m is MethodInfo method
                ? !method.IsSpecialName
                : true;
        }

        private void ProcessSingleType(Type input)
        {
            WriteObject(
                input.FindMembers(
                    MemberType,
                    _flags,
                    AggregateFilter,
                    null),
                enumerateCollection: true);
        }

        private void FindMemberProxy(MemberInfo member)
        {
            if (!MemberType.HasFlag(member.MemberType) ||
                !MatchesBindingFlags(member) ||
                !AggregateFilter(member, null))
            {
                return;
            }

            WriteObject(member);
        }

        private bool MatchesBindingFlags(MemberInfo member)
        {
            if (_flags.HasFlag(BindingFlags.Static) && _flags.HasFlag(BindingFlags.Instance))
            {
                return true;
            }

            return
                !((IsStatic(member) && !_flags.HasFlag(BindingFlags.Static)) ||
                (!IsStatic(member) && !_flags.HasFlag(BindingFlags.Instance)));
        }

        private bool IsStatic(MemberInfo member)
        {
            if (member is MethodBase method)
            {
                return method.IsStatic;
            }

            if (member is PropertyInfo property)
            {
                return property.GetMethod.IsStatic;
            }

            if (member is FieldInfo field)
            {
                return field.IsStatic;
            }

            if (member is EventInfo eventInfo)
            {
                return eventInfo.AddMethod.IsStatic;
            }

            return true;
        }
    }
}
