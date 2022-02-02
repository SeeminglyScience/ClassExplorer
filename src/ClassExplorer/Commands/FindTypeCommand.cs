using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Management.Automation;
using ClassExplorer.Signatures;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The FindType cmdlet searches the AppDomain for matching types.
    /// </summary>
    [OutputType(typeof(Type))]
    [Cmdlet(VerbsCommon.Find, "Type", DefaultParameterSetName = "ByFilter")]
    public class FindTypeCommand : FindReflectionObjectCommandBase<Type>
    {
        private readonly TypeSearchOptions _options = new();

        /// <summary>
        /// Gets or sets the namespace to match.
        /// </summary>
        [Parameter(Position = 1)]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(NamespaceArgumentCompleter))]
        [Alias("ns")]
        public string? Namespace
        {
            get => _options.Namespace;
            set => _options.Namespace = value;
        }

        /// <summary>
        /// Gets or sets the type name to match.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByName")]
        [Parameter(ParameterSetName = "ByFilter")]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(TypeNameArgumentCompleter))]
        public override string Name
        {
            get => _options.Name!;
            set => _options.Name = value;
        }

        /// <summary>
        /// Gets or sets the full type name to match.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [Alias("fn")]
        public string? FullName
        {
            get => _options.FullName;
            set => _options.FullName = value;
        }

        /// <summary>
        /// Gets or sets the base type that a type must inherit to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [Alias("Base")]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        public ScriptBlockStringOrType InheritsType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the interface that a type must implement to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeFullNameArgumentCompleter))]
        [Alias("int")]
        public ScriptBlockStringOrType ImplementsInterface { get; set; } = null!;

        [Parameter]
        [ValidateNotNull]
        [Alias("sig")]
        public ScriptBlockStringOrType Signature { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to only match abstract classes.
        /// </summary>
        [Parameter]
        [Alias("a")]
        public SwitchParameter Abstract
        {
            get => _options.Abstract;
            set => _options.Abstract = value;
        }

        [Parameter]
        [Alias("s")]
        public SwitchParameter Static
        {
            get => _options.Static;
            set => _options.Static = value;
        }

        [Parameter]
        [Alias("se")]
        public SwitchParameter Sealed
        {
            get => _options.Sealed;
            set => _options.Sealed = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match interfaces.
        /// </summary>
        [Parameter]
        [Alias("i")]
        public SwitchParameter Interface
        {
            get => _options.Interface;
            set => _options.Interface = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only match ValueTypes.
        /// </summary>
        [Parameter]
        [Alias("vt")]
        public SwitchParameter ValueType
        {
            get => _options.ValueType;
            set => _options.ValueType = value;
        }

        private TypeSearch<PipelineEmitter<Type>> _search = null!;

        private protected override void OnNoInput()
        {
            _search.SearchAll();
        }

        /// <summary>
        /// The ProcessSingleObject method.
        /// </summary>
        /// <param name="input">The input parameter from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            _search.SearchSingleObject(input);
        }

        /// <summary>
        /// Process parameters and create the filter list.
        /// </summary>
        protected override void InitializeFilters()
        {
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap = InitializeResolutionMap();

            var parser = new SignatureParser(resolutionMap);
            var signatures = ImmutableArray.CreateBuilder<ITypeSignature>();
            ITypeSignature? signature = null;
            if (Signature is not null)
            {
                signature = Signature.Resolve(parser);
                signatures.Add(signature);
            }

            if (InheritsType is not null)
            {
                ITypeSignature inheritsTypeSignature = InheritsType.Resolve(parser, excludeSelf: true);
                signatures.Add(inheritsTypeSignature);
                signature ??= inheritsTypeSignature;
            }

            if (ImplementsInterface is not null)
            {
                ITypeSignature implementsSignature = ImplementsInterface.Resolve(parser, excludeSelf: true);
                signatures.Add(implementsSignature);
                signature ??= implementsSignature;
            }

            if (signatures is { Count: > 1 })
            {
                signature = new AllOfTypeSignature(signatures.ToImmutable());
            }

            _options.FilterScript = FilterScript;
            _options.Force = Force;
            _options.Not = Not;
            _options.RegularExpression = RegularExpression;
            _options.ResolutionMap = resolutionMap;
            _options.Signature = signature;
            _options.AccessView = AccessView;
            _search = Search.Types(_options, new PipelineEmitter<Type>(this));
        }
    }
}
