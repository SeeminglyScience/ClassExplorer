using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

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
    [Alias("fime")]
    [Cmdlet(VerbsCommon.Find, "Member", DefaultParameterSetName = "ByFilter")]
    public class FindMemberCommand : FindReflectionObjectCommandBase<MemberInfo>
    {
        private readonly MemberSearchOptions _options = new();

        /// <summary>
        /// Gets or sets the parameter type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        [Alias("pt")]
        public ScriptBlockStringOrType? ParameterType
        {
            get => _options.ParameterType;
            set => _options.ParameterType = value;
        }

        /// <summary>
        /// Gets or sets the generic parameter to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        [Alias("gp")]
        public ScriptBlockStringOrType? GenericParameter
        {
            get => _options.GenericParameter;
            set => _options.GenericParameter = value;
        }

        /// <summary>
        /// Gets or sets the parameter count to match.
        /// </summary>
        [Parameter]
        [Alias("pc")]
        public RangeExpression[]? ParameterCount
        {
            get => _options.ParameterCount;
            set => _options.ParameterCount = value;
        }

        /// <summary>
        /// Gets or sets the generic parameter count to match.
        /// </summary>
        [Parameter]
        [Alias("gpc")]
        public RangeExpression[]? GenericParameterCount
        {
            get => _options.GenericParameterCount;
            set => _options.GenericParameterCount = value;
        }

        /// <summary>
        /// Gets or sets the return, property, or field type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        [Alias("ret", "rt")]
        public ScriptBlockStringOrType? ReturnType
        {
            get => _options.ReturnType;
            set => _options.ReturnType = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include special name members.
        /// </summary>
        [Parameter]
        [Alias("isn")]
        public SwitchParameter IncludeSpecialName
        {
            get => _options.IncludeSpecialName;
            set => _options.IncludeSpecialName = value;
        }

        [Parameter]
        [Alias("HasAttr", "attr")]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        public ScriptBlockStringOrType? Decoration
        {
            get => _options.Decoration;
            set => _options.Decoration = value;
        }

        /// <summary>
        /// Gets or sets the member type to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [Alias("mt")]
        public MemberTypes MemberType
        {
            get => _options.MemberType;
            set => _options.MemberType = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match static members.
        /// </summary>
        [Parameter]
        [Alias("s")]
        public SwitchParameter Static
        {
            get => _options.Static;
            set => _options.Static = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match instance members.
        /// </summary>
        [Parameter]
        [Alias("i")]
        public SwitchParameter Instance
        {
            get => _options.Instance;
            set => _options.Instance = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match abstract members.
        /// </summary>
        [Parameter]
        [Alias("a")]
        public SwitchParameter Abstract
        {
            get => _options.Abstract;
            set => _options.Abstract = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match virtual members.
        /// </summary>
        [Parameter]
        [Alias("v")]
        public SwitchParameter Virtual
        {
            get => _options.Virtual;
            set => _options.Virtual = value;
        }

        [Parameter]
        [Alias("d")]
        public SwitchParameter Declared
        {
            get => _options.Declared;
            set => _options.Declared = value;
        }

        [Parameter]
        [Alias("io")]
        public SwitchParameter IncludeObject
        {
            get => _options.IncludeObject;
            set => _options.IncludeObject = value;
        }

        [Parameter]
        [Alias("r")]
        public SwitchParameter RecurseNestedType
        {
            get => _options.RecurseNestedType;
            set => _options.RecurseNestedType = value;
        }

        [Parameter]
        [Alias("ext")]
        public SwitchParameter Extension
        {
            get => _options.Extension;
            set => _options.Extension = value;
        }

        private MemberSearch<PipelineEmitter<MemberInfo>> _search = null!;

        private protected override void OnNoInput()
        {
            _search.SearchAll();
        }

        /// <summary>
        /// Process a object from the pipeline. Filter passed members and get members for passed types
        /// and the types of any other type of object passed.
        /// </summary>
        /// <param name="input">The input from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            _search.SearchSingleObject(input);
        }

        /// <summary>
        /// Process input parameters and create the filter set.
        /// </summary>
        protected override void InitializeFilters()
        {
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap = InitializeResolutionMap();
            _options.FilterScript = FilterScript;
            _options.Force = Force;
            _options.Name = Name;
            _options.Not = Not;
            _options.RegularExpression = RegularExpression;
            _options.ResolutionMap = resolutionMap;
            _options.AccessView = AccessView;
            _search = Search.Members(_options, new PipelineEmitter<MemberInfo>(this));
        }
    }
}
