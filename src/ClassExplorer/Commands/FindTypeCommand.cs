using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ClassExplorer.Signatures;
using static ClassExplorer.FilterFrame<System.Type>;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The FindType cmdlet searches the AppDomain for matching types.
    /// </summary>
    [OutputType(typeof(Type))]
    [Cmdlet(VerbsCommon.Find, "Type", DefaultParameterSetName = "ByFilter")]
    public class FindTypeCommand : FindReflectionObjectCommandBase<Type>
    {
        /// <summary>
        /// Gets or sets the namespace to match.
        /// </summary>
        [Parameter(Position = 1)]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(NamespaceArgumentCompleter))]
        [Alias("ns")]
        public string Namespace { get; set; } = null!;

        /// <summary>
        /// Gets or sets the type name to match.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByName")]
        [Parameter(ParameterSetName = "ByFilter")]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(TypeNameArgumentCompleter))]
        public override string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the full type name to match.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the base type that a type must inherit to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [Alias("Base")]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type InheritsType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the interface that a type must implement to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type ImplementsInterface { get; set; } = null!;

        [Parameter]
        [ValidateNotNull]
        public ScriptBlock Signature { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to only match abstract classes.
        /// </summary>
        [Parameter]
        public SwitchParameter Abstract { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only match interfaces.
        /// </summary>
        [Parameter]
        public SwitchParameter Interface { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only match ValueTypes.
        /// </summary>
        [Parameter]
        public SwitchParameter ValueType { get; set; }

        private protected override void OnNoInput()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                ProcessAssembly(assembly);
            }
        }

        /// <summary>
        /// The ProcessSingleObject method.
        /// </summary>
        /// <param name="input">The input parameter from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            if (input.BaseObject is Assembly assembly)
            {
                ProcessAssembly(assembly);
                return;
            }

            if (input.BaseObject is Type)
            {
                base.ProcessSingleObject(input);
                return;
            }

            base.ProcessSingleObject(
                new PSObject(
                    input.BaseObject.GetType()));
        }

        /// <summary>
        /// Process parameters and create the filter list.
        /// </summary>
        protected override void InitializeFilters()
        {
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap = InitializeResolutionMap();
            ProcessParameter(static (m, _) => m.IsPublic, !Force.IsPresent);
            ProcessParameter(static (m, _) => m.IsAbstract, Abstract.IsPresent);
            ProcessParameter(static (m, _) => m.IsInterface, Interface.IsPresent);
            ProcessParameter(static (m, _) => m.IsValueType, ValueType.IsPresent);
            if (InheritsType is not null)
            {
                ProcessParameter(
                    static (m, fc) => Unsafe.As<ITypeSignature>(fc).IsMatch(m),
                    new AssignableTypeSignature(InheritsType));
            }

            ProcessParameter(static (m, fc) => ImplementsFilter(m, fc), ImplementsInterface);
            ProcessNameParameters();

            if (Signature is not null)
            {
                ProcessParameter(
                    static (m, fc) => Unsafe.As<ITypeSignature>(fc).IsMatch(m),
                    new ScriptBlockStringOrType(Signature).Resolve(new SignatureParser(resolutionMap)));
            }

            if (FilterScript is not null)
            {
                ITypeSignature filterScriptSig = new FilterScriptSignature(FilterScriptPipe.Create(FilterScript));
                ReflectionFilter filter = new(
                    static (type, criteria) => Unsafe.As<ITypeSignature>(criteria).IsMatch(type));

                ProcessParameter(filter, filterScriptSig);
            }
        }

        /// <summary>
        /// A filter that matches only public types.
        /// </summary>
        /// <param name="m">The type to test for a match.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected override bool PublicFilter(Type m, object filterCriteria)
        {
            return m.IsPublic;
        }

        private static bool WildcardNamespaceFilter(Type m, object? filterCriteria)
        {
            return Unsafe.As<WildcardPattern>(filterCriteria).IsMatch(m.Namespace);
        }

        private static bool RegexNamespaceFilter(Type m, object? filterCriteria)
        {
            return m.Namespace is { Length: > 0 }
                && Unsafe.As<Regex>(filterCriteria).IsMatch(m.Namespace);
        }

        private static bool WildcardFullNameFilter(Type m, object? filterCriteria)
        {
            return Unsafe.As<WildcardPattern>(filterCriteria).IsMatch(m.FullName);
        }

        private static bool RegexFullNameFilter(Type m, object? filterCriteria)
        {
            return Unsafe.As<Regex>(filterCriteria).IsMatch(m.FullName);
        }

        private static bool ImplementsFilter(Type m, object? filterCriteria)
        {
            Type implementedInterface = Unsafe.As<Type>(filterCriteria);
            foreach (Type @interface in m.GetInterfaces())
            {
                if (@interface.IsGenericType)
                {
                    if (implementedInterface == @interface.GetGenericTypeDefinition())
                    {
                        return true;
                    }

                    continue;
                }

                if (implementedInterface.IsGenericType)
                {
                    continue;
                }

                if (@interface == implementedInterface)
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessNameParameters()
        {
            ProcessName(
                Name,
                static (m, fc) => WildcardNameFilter(m, fc),
                static (m, fc) => RegexNameFilter(m, fc));

            ProcessName(
                Namespace,
                static (m, fc) => WildcardNamespaceFilter(m, fc),
                static (m, fc) => RegexNamespaceFilter(m, fc));
            ProcessName(
                FullName,
                static (m, fc) => WildcardFullNameFilter(m, fc),
                static (m, fc) => RegexFullNameFilter(m, fc));
        }

        private void ProcessAssembly(Assembly assembly)
        {
            Module[] modules;
            try
            {
                modules = assembly.GetModules();
            }
            catch (ReflectionTypeLoadException)
            {
                return;
            }

            foreach (Module module in modules)
            {
                try
                {
                    module.FindTypes(static (m, fc) => AggregateFilter(m, fc), this);
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
            }
        }
    }
}
