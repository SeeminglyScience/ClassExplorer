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
    public sealed class NamespaceArgumentCompleter : IArgumentCompleter
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
            wordToComplete = CompletionHelper.GetWordToComplete(
                wordToComplete,
                out char prefix,
                out char suffix);

            Search.Types(
                new TypeSearchOptions() { Namespace = string.Concat(wordToComplete, "*") },
                new CreateCompletionResult(prefix, suffix, out List<CompletionResult> results))
                .SearchAll();

            return results;
        }

        private readonly struct CreateCompletionResult : IEnumerationCallback<Type>
        {
            private readonly char _prefix;

            private readonly char _suffix;

            private readonly HashSet<string> _alreadyProcessed;

            private readonly List<CompletionResult> _results;

            public CreateCompletionResult(char prefix, char suffix, out List<CompletionResult> results)
            {
                _prefix = prefix;
                _suffix = suffix;
                _alreadyProcessed = new(StringComparer.OrdinalIgnoreCase);
                _results = results = new();
            }

            public void Invoke(Type value)
            {
                string? @namespace = value.Namespace;
                if (Poly.IsStringNullOrEmpty(@namespace))
                {
                    return;
                }

                if (!_alreadyProcessed.Add(@namespace))
                {
                    return;
                }

                string completionValue = CompletionHelper.FinishCompletionValue(
                    @namespace,
                    (_prefix, _suffix));

                _results.Add(
                    new CompletionResult(
                        completionValue,
                        @namespace,
                        CompletionResultType.ParameterValue,
                        @namespace));
            }

            public void Invoke(Type value, object? source) => Invoke(value);
        }
    }
}
