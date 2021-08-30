using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Signatures;
using static ClassExplorer.FilterFrame<System.Reflection.MemberInfo>;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The Find-Member cmdlet searches the current AppDomain for matching members.
    /// </summary>
    [OutputType(
        typeof(PropertyInfo),
        typeof(MethodInfo),
        typeof(ConstructorInfo),
        typeof(EventInfo),
        typeof(FieldInfo),
        typeof(Type))]
    [Cmdlet(VerbsCommon.Find, "Member", DefaultParameterSetName = "ByFilter")]
    public class FindMemberCommand : FindReflectionObjectCommandBase<MemberInfo>
    {
        private readonly HashSet<Type> _processedTypes = new();

        private BindingFlags _flags;

        private MemberTypes? _memberTypes;

        /// <summary>
        /// Gets or sets the parameter type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public ScriptBlockStringOrType ParameterType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the return, property, or field type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public ScriptBlockStringOrType ReturnType { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to include special name members.
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeSpecialName { get; set; }

        [Parameter]
        public ScriptBlockStringOrType Decoration { get; set; } = null!;

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
                if (_memberTypes == null)
                {
                    return MemberTypes.All;
                }

                return Not.IsPresent
                    ? MemberTypes.All ^ _memberTypes.Value
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

        private protected override void OnNoInput()
        {
            foreach (Type type in new FindTypeCommand() { Force = Force }.Invoke<Type>())
            {
                type.FindMembers(
                    MemberType,
                    _flags,
                    static (m, fc) => AggregateFilter(m, fc),
                    this);
            }
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

            Type targetType = input.BaseObject.GetType();
            if (!_processedTypes.Add(targetType))
            {
                return;
            }

            ProcessSingleType(targetType);
        }

        /// <summary>
        /// Process input parameters and create the filter set.
        /// </summary>
        protected override void InitializeFilters()
        {
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap = InitializeResolutionMap();

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

            if (!Not.IsPresent)
            {
                ProcessParameter(static (m, fc) => SpecialNameFilter(m, fc), !IncludeSpecialName.IsPresent);
            }

            ProcessName(
                Name,
                static (m, fc) => WildcardNameFilter(m, fc),
                static (m, fc) => RegexNameFilter(m, fc));

            var parser = new SignatureParser(resolutionMap);

            if (ParameterType is not null)
            {
                Filters.Add(
                    CreateFrame(
                        static (m, fc) => Unsafe.As<IMemberSignature>(fc).IsMatch(m),
                        new ParameterTypeSignature(ParameterType.Resolve(parser))));
            }

            if (ReturnType is not null)
            {
                Filters.Add(
                    CreateFrame(
                        static (m, fc) => Unsafe.As<IMemberSignature>(fc).IsMatch(m),
                        new ReturnTypeSignature(ReturnType.Resolve(parser))));
            }

            if (Decoration is not null)
            {
                Filters.Add(
                    CreateFrame(
                        static (m, fc) => Unsafe.As<IMemberSignature>(fc).IsMatch(m),
                        new DecorationSignature(SignatureParser.ResolveAttributeTypeName(Decoration, resolutionMap))));
            }

            ProcessParameter(static (m, _) => m is MethodInfo method && method.IsAbstract, Abstract.IsPresent);
            ProcessParameter(static (m, _) => m is MethodInfo method && method.IsVirtual, Virtual.IsPresent);

            if (FilterScript != null)
            {
                IMemberSignature memberScriptSig = new FilterScriptSignature(FilterScriptPipe.Create(FilterScript));
                ReflectionFilter filter = new(
                    static (m, fc) => Unsafe.As<IMemberSignature>(fc).IsMatch(m));

                ProcessParameter(filter, memberScriptSig);
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

        private static bool SpecialNameFilter(MemberInfo m, object? _)
        {
            return m is not MethodInfo method || !method.IsSpecialName;
        }

        private void ProcessSingleType(Type input)
        {
            input.FindMembers(
                MemberType,
                _flags,
                static (m, fc) => AggregateFilter(m, fc),
                filterCriteria: this);
        }

        private void FindMemberProxy(MemberInfo member)
        {
            if (!MemberType.HasFlag(member.MemberType)
                || !MatchesBindingFlags(member))
            {
                return;
            }

            AggregateFilter(member, this);
        }

        private bool MatchesBindingFlags(MemberInfo member)
        {
            const BindingFlags staticInstance = BindingFlags.Static | BindingFlags.Instance;
            if ((_flags & staticInstance) is staticInstance)
            {
                return true;
            }

            if (IsStatic(member))
            {
                return (_flags & BindingFlags.Static) is not 0;
            }

            return (_flags & BindingFlags.Instance) is not 0;
        }

        private bool IsStatic(MemberInfo member)
        {
            if (member is MethodBase method)
            {
                return method.IsStatic;
            }

            if (member is PropertyInfo property)
            {
                return (property.GetGetMethod(nonPublic: true)
                    ?? property.GetSetMethod(nonPublic: true))?.IsStatic is true;
            }

            if (member is FieldInfo field)
            {
                return field.IsStatic;
            }

            if (member is EventInfo eventInfo)
            {
                return (eventInfo.GetAddMethod(nonPublic: true)
                    ?? eventInfo.GetRemoveMethod(nonPublic: true))?.IsStatic is true;
            }

            return true;
        }
    }
}
