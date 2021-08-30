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
        [ArgumentCompleter(typeof(AssemblyNameArgumentCompleter))]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The BeginProcessing method.
        /// </summary>
        protected override void BeginProcessing()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(assemblies, enumerateCollection: true);
                return;
            }

            Func<string, string, WildcardPattern, bool> matcher = Name switch
            {
                string name when WildcardPattern.ContainsWildcardCharacters(name)
                    => (assemblyName, name, pattern) => pattern.IsMatch(assemblyName),

                string name when !name.Any(c => char.IsUpper(c))
                    => (assemblyName, name, pattern) => name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase),

                string name
                    => (assemblyName, name, pattern) => name.Equals(assemblyName, StringComparison.Ordinal),

                _ => Unreachable.Code<Func<string, string, WildcardPattern, bool>>(),
            };

            WildcardPattern pattern = new(Name, WildcardOptions.IgnoreCase);
            foreach (Assembly assembly in assemblies)
            {
                string assemblyName = assembly.GetName()?.Name ?? string.Empty;
                if (matcher(assemblyName, Name, pattern))
                {
                    WriteObject(assembly);
                }
            }
        }
    }
}
