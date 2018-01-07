using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
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
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the type name to match.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByName")]
        [Parameter(ParameterSetName = "ByFilter")]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(TypeNameArgumentCompleter))]
        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets the full type name to match.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the base type that a type must inherit to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [Alias("Base")]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type InheritsType { get; set; }

        /// <summary>
        /// Gets or sets the interface that a type must implement to match.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        [ArgumentCompleter(typeof(TypeArgumentCompleter))]
        public Type ImplementsInterface { get; set; }

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

        /// <summary>
        /// The EndProcessing method.
        /// </summary>
        protected override void EndProcessing()
        {
            if (ExpectingInput) return;

            Array.ForEach(
                AppDomain
                    .CurrentDomain
                    .GetAssemblies(),
                ProcessAssembly);
        }

        /// <summary>
        /// The ProcessSingleObject method.
        /// </summary>
        /// <param name="input">The input parameter from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            if (input.BaseObject is NamespaceInfo ns)
            {
                ProcessName(ns.FullName, WildcardNamespaceFilter, RegexNamespaceFilter);
                Array.ForEach(
                    ns.Assemblies.ToArray(),
                    ProcessAssembly);
                return;
            }

            if (input.BaseObject is Assembly assembly)
            {
                ProcessAssembly(assembly);
                return;
            }

            if (input.BaseObject is Type type)
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
            ProcessParameter(PublicFilter, !Force.IsPresent);
            ProcessParameter(AbstractFilter, Abstract.IsPresent);
            ProcessParameter(InterfaceFilter, Interface.IsPresent);
            ProcessParameter(ValueTypeFilter, ValueType.IsPresent);
            ProcessParameter<Type>(ParentFilter, InheritsType);
            ProcessParameter<Type>(ImplementsFilter, ImplementsInterface);
            ProcessNameParameters();

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
        /// A filter that matches only public types.
        /// </summary>
        /// <param name="m">The type to test for a match.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected override bool PublicFilter(Type m, object filterCriteria)
        {
            return m.IsPublic;
        }

        private static bool AbstractFilter(Type m, object filter)
        {
            return m.IsAbstract;
        }

        private static bool InterfaceFilter(Type m, object filter)
        {
            return m.IsInterface;
        }

        private static bool ValueTypeFilter(Type m, object filter)
        {
            return m.IsValueType;
        }

        private static bool WildcardNamespaceFilter(Type m, object filterCriteria)
        {
            WildcardPattern pattern = filterCriteria as WildcardPattern;

            return pattern == null ? false : pattern.IsMatch(m.Namespace);
        }

        private static bool RegexNamespaceFilter(Type m, object filterCriteria)
        {
            Regex pattern = filterCriteria as Regex;

            return !string.IsNullOrWhiteSpace(m.Namespace) &&
                pattern == null ? false : pattern.IsMatch(m.Namespace);
        }

        private static bool WildcardFullNameFilter(Type m, object filterCriteria)
        {
            WildcardPattern pattern = filterCriteria as WildcardPattern;

            return pattern == null ? false : pattern.IsMatch(m.ToString());
        }

        private static bool RegexFullNameFilter(Type m, object filterCriteria)
        {
            Regex pattern = filterCriteria as Regex;

            return pattern == null ? false : pattern.IsMatch(m.ToString());
        }

        private static bool ParentFilter(Type m, object filterCriteria)
        {
            var parent = filterCriteria as Type;

            return parent == null ? false : m.IsSubclassOf(parent);
        }

        private static bool ImplementsFilter(Type m, object filterCriteria)
        {
            var implementedInterface = filterCriteria as Type;

            return m.GetInterfaces().Contains(implementedInterface);
        }

        private void ProcessNameParameters()
        {
            ProcessName(Name, WildcardNameFilter, RegexNameFilter);
            ProcessName(Namespace, WildcardNamespaceFilter, RegexNamespaceFilter);
            ProcessName(FullName, WildcardFullNameFilter, RegexFullNameFilter);
        }

        private void ProcessAssembly(Assembly assembly)
        {
            WriteObject(
                assembly
                    .GetModules()
                    .SelectMany(
                        m =>
                        {
                            try
                            {
                                return m.FindTypes(AggregateFilter, null);
                            }
                            catch (ReflectionTypeLoadException)
                            {
                                // TODO: add a debug or verbose message here.
                                return Enumerable.Empty<Type>();
                            }
                        }),
                enumerateCollection: true);
        }
    }
}
