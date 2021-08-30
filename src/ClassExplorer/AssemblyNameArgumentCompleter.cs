using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ClassExplorer
{
    /// <summary>
    /// Provides argument completion for assembly name input parameters.
    /// </summary>
    public class AssemblyNameArgumentCompleter : IArgumentCompleter
    {
        private static readonly Lazy<HashSet<string>> s_cachedAssemblyNames = new(static () => LoadAssemblyNames());

        /// <summary>
        /// Called by the PowerShell engine to complete a parameter.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="wordToComplete">The current parameter value.</param>
        /// <param name="commandAst">The AST of the command.</param>
        /// <param name="fakeBoundParameters">A dictionary of currently bound parameters.</param>
        /// <returns>The result of completion.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(
            string commandName,
            string parameterName,
            string wordToComplete,
            CommandAst commandAst,
            IDictionary fakeBoundParameters)
        {
            return s_cachedAssemblyNames.Value
                .Where(name => name.StartsWith(wordToComplete, StringComparison.CurrentCultureIgnoreCase))
                .Select(
                    static name => new CompletionResult(
                        name,
                        name,
                        CompletionResultType.ParameterValue,
                        name));
        }

        private static HashSet<string> LoadAssemblyNames()
        {
            AppDomain.CurrentDomain.AssemblyLoad += static (s, e) => AppDomain_OnAssemblyLoad(s, e);
            try
            {
                return
                    new HashSet<string>(
                        AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .Select(static assembly => assembly.GetName()?.Name ?? string.Empty));
            }
            catch
            {
                // TODO: Better handling
                AppDomain.CurrentDomain.AssemblyLoad -= static (s, e) => AppDomain_OnAssemblyLoad(s, e);
                throw;
            }
        }

        private static void AppDomain_OnAssemblyLoad(object? source, AssemblyLoadEventArgs e)
        {
            string? assemblyName = e.LoadedAssembly.GetName()?.Name ?? null;
            if (string.IsNullOrEmpty(assemblyName))
            {
                return;
            }

            s_cachedAssemblyNames.Value.Add(assemblyName);
        }
    }
}
