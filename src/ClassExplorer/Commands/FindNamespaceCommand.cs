using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using static ClassExplorer.FilterFrame<ClassExplorer.NamespaceInfo>;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The Find-Namespace cmdlet searches the AppDomain for matching namespaces.
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "Namespace", DefaultParameterSetName = "ByFilter")]
    [OutputType(typeof(NamespaceInfo))]
    public class FindNamespaceCommand : FindReflectionObjectCommandBase<NamespaceInfo>
    {
        private static HashSet<string> _processedNamespaces = new HashSet<string>();

        /// <summary>
        /// Gets or sets the name of the namespace to match.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByName")]
        [Parameter(ParameterSetName = "ByFilter")]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(NamespaceNameArgumentCompleter))]
        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name of the namespace to match.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(NamespaceArgumentCompleter))]
        public string FullName { get; set; }

        /// <summary>
        /// Process parameters and create the filter list.
        /// </summary>
        protected override void InitializeFilters()
        {
            ProcessName(Name, WildcardNameFilter, RegexNameFilter);
            ProcessName(FullName, WildcardNamespaceFilter, RegexNamespaceFilter);
            if (FilterScript != null)
            {
                var filter = new ReflectionFilter(
                    (ns, criteria) =>
                    {
                        return LanguagePrimitives.IsTrue(
                            FilterScript.InvokeWithContext(
                                null,
                                new List<PSVariable>() { new PSVariable("_", ns) },
                                ns));
                    });
                ProcessParameter(filter, true);
            }
        }

        /// <summary>
        /// The EndProcessing method.
        /// </summary>
        protected override void EndProcessing()
        {
            if (ExpectingInput)
            {
                return;
            }

            if (!(RegularExpression.IsPresent ||
                string.IsNullOrWhiteSpace(FullName) ||
                WildcardPattern.ContainsWildcardCharacters(FullName)))
            {
                if (NamespaceInfo.Namespaces.TryGetValue(FullName, out NamespaceInfo ns))
                {
                    WriteObject(ns);
                }

                return;
            }

            WriteObject(
                NamespaceInfo
                    .GetNamespaces()
                    .Where(ns => AggregateFilter(ns, null)),
                enumerateCollection: true);
        }

        /// <summary>
        /// A filter that matches only public namespaces. This currently always returns true.
        /// </summary>
        /// <param name="m">The namepsace to test for a match.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected override bool PublicFilter(NamespaceInfo m, object filterCriteria)
        {
            return true;
        }

        /// <summary>
        /// The ProcessSingleObject method.
        /// </summary>
        /// <param name="input">The input parameter from the pipeline.</param>
        protected override void ProcessSingleObject(PSObject input)
        {
            if (input.BaseObject == null)
            {
                return;
            }

            if (input.BaseObject is NamespaceInfo)
            {
                base.ProcessSingleObject(input);
                return;
            }

            if (input.BaseObject is Assembly assembly)
            {
                WriteObject(
                    NamespaceInfo
                        .GetNamespaces()
                        .Where(ns => ns.Assemblies.Contains(assembly) && AggregateFilter(ns, null)),
                    enumerateCollection: true);
                return;
            }

            if (input.BaseObject is Type type)
            {
                ProcessSingleNamespace(type.Namespace);
                return;
            }

            if (input.BaseObject is MemberInfo member)
            {
                ProcessSingleNamespace(member.ReflectedType.Namespace);
                return;
            }

            ProcessSingleNamespace(input.BaseObject.GetType().Namespace);
        }

        private static bool WildcardNamespaceFilter(NamespaceInfo m, object filterCriteria)
        {
            WildcardPattern pattern = filterCriteria as WildcardPattern;

            return pattern == null ? false : pattern.IsMatch(m.FullName);
        }

        private static bool RegexNamespaceFilter(NamespaceInfo m, object filterCriteria)
        {
            Regex pattern = filterCriteria as Regex;

            return !string.IsNullOrWhiteSpace(m.FullName) &&
                pattern == null ? false : pattern.IsMatch(m.FullName);
        }

        private bool ProcessSingleNamespace(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                return false;
            }

            if (_processedNamespaces.Contains(ns))
            {
                return false;
            }

            NamespaceInfo existing;
            if (!NamespaceInfo.Namespaces.TryGetValue(ns, out existing))
            {
                return false;
            }

            if (!AggregateFilter(existing, null))
            {
                return false;
            }

            _processedNamespaces.Add(ns);
            WriteObject(existing);
            return true;
        }
    }
}
