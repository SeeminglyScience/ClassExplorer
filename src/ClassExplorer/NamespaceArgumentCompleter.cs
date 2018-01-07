using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using ClassExplorer.Commands;

namespace ClassExplorer
{
    /// <summary>
    /// Provides argument completion for namespace input parameters.
    /// </summary>
    public class NamespaceArgumentCompleter : IArgumentCompleter
    {
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
            return new FindNamespaceCommand() { Name = wordToComplete + "*" }
                .Invoke<NamespaceInfo>()
                .Select(
                    ns => new CompletionResult(
                        ns.FullName,
                        ns.FullName,
                        CompletionResultType.ParameterValue,
                        ns.FullName));
        }
    }
}
