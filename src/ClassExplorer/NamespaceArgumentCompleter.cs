using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

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
            List<CompletionResult> results = new();
            Search.Types(
                new TypeSearchOptions() { Namespace = (wordToComplete ?? string.Empty) + "*" },
                new ListBuilder(results))
                .SearchAll();

            return results;
        }

        private readonly struct ListBuilder : IEnumerationCallback<Type>
        {
            private readonly HashSet<string> _alreadyProcessed;

            private readonly List<CompletionResult> _results;

            public ListBuilder(List<CompletionResult> results)
            {
                _alreadyProcessed = new(StringComparer.OrdinalIgnoreCase);
                _results = results;
            }

            public void Invoke(Type value)
            {
                string? ns = value.Namespace;
                if (Poly.IsStringNullOrEmpty(ns))
                {
                    return;
                }

                if (!_alreadyProcessed.Add(ns))
                {
                    return;
                }

                _results.Add(
                    new CompletionResult(ns, ns, CompletionResultType.ParameterValue, ns));
            }
        }
    }
}
