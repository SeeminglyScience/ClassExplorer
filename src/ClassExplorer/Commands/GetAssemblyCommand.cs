using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The Get-Assembly cmdlet gets the assemblies currently loaded in the AppDomain.
    /// </summary>
    [OutputType(typeof(Assembly))]
    [Cmdlet(VerbsCommon.Get, "Assembly")]
    public class GetAssemblyCommand : Cmdlet
    {
        /// <summary>
        /// Gets or sets the name to match.
        /// </summary>
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        public string Name { get; set; }

        /// <summary>
        /// The EndProcessing method.
        /// </summary>
        protected override void EndProcessing()
        {
            Assembly[] assemblies =
                AppDomain
                    .CurrentDomain
                    .GetAssemblies();

            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(assemblies, enumerateCollection: true);
                return;
            }

            var ignoreCase = StringComparison.CurrentCultureIgnoreCase;
            if (!WildcardPattern.ContainsWildcardCharacters(Name))
            {
                WriteObject(
                    assemblies.Where(assembly => assembly.GetName().Name.Equals(Name, ignoreCase)),
                    enumerateCollection: true);
                return;
            }

            var pattern = new WildcardPattern(Name, WildcardOptions.IgnoreCase);

            WriteObject(
                assemblies.Where(assembly => pattern.IsMatch(assembly.GetName().Name)),
                enumerateCollection: true);
        }
    }
}
