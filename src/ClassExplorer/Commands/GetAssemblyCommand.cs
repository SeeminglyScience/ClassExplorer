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
    public sealed class GetAssemblyCommand : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the name to match.
        /// </summary>
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        [ArgumentCompleter(typeof(AssemblyNameArgumentCompleter))]
        public string Name { get; set; } = null!;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; } = null!;

        /// <summary>
        /// The BeginProcessing method.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (MyInvocation.ExpectingInput)
            {
                return;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(assemblies, enumerateCollection: true);
                return;
            }

            Array.Sort(
                assemblies,
                static (x, y) =>
                {
                    string xLocation = x.IsDynamic ? string.Empty : x.Location;
                    string yLocation = y.IsDynamic ? string.Empty : y.Location;
                    return xLocation.CompareTo(yLocation);
                });

            StringMatcher matcher = StringMatcher.Create(Name);
            foreach (Assembly assembly in assemblies)
            {
                string assemblyName = assembly.GetName()?.Name ?? string.Empty;
                if (matcher.IsMatch(assemblyName))
                {
                    WriteObject(assembly);
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (InputObject is not { BaseObject: not null })
            {
                return;
            }

            if (InputObject.BaseObject is Type type)
            {
                WriteObject(type.Assembly, enumerateCollection: false);
                return;
            }

            if (InputObject.BaseObject is MemberInfo member)
            {
                WriteObject(member.Module.Assembly, enumerateCollection: false);
                return;
            }

            if (InputObject.BaseObject is ParameterInfo parameter)
            {
                WriteObject(parameter.Member.Module.Assembly, enumerateCollection: false);
                return;
            }

            if (InputObject.BaseObject is Assembly assembly)
            {
                WriteObject(assembly, enumerateCollection: false);
                return;
            }

            WriteObject(InputObject.BaseObject.GetType().Assembly, enumerateCollection: false);
        }
    }
}
