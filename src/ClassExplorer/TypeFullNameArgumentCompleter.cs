using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ClassExplorer
{
    /// <summary>
    /// Provides argument completion for Type input parameters.
    /// </summary>
    public sealed class TypeFullNameArgumentCompleter : IArgumentCompleter
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

            List<CompletionResult> results;
            if (wordToComplete.AsSpan().IndexOf('.') is not -1)
            {
                Search.Types(
                    new() { FullName = string.Concat(wordToComplete, "*") },
                    new CreateCompletionResult(prefix, suffix, out results))
                    .SearchAll();

                return results;
            }

            Search.Types(
                new() { Name = string.Concat(wordToComplete, "*") },
                new CreateCompletionResult(prefix, suffix, out results))
                .SearchAll();

            return results;
        }

        private readonly struct CreateCompletionResult : IEnumerationCallback<Type>
        {
            private readonly List<CompletionResult> _results;

            private readonly char _prefix;

            private readonly char _suffix;

            public CreateCompletionResult(char prefix, char suffix, out List<CompletionResult> results)
            {
                _results = results = new();
                _prefix = prefix;
                _suffix = suffix;
            }

            public void Invoke(Type value)
            {
                var (_, list, tip) = CompletionHelper.GetCompletionValue(value);
                string completionValue = CompletionHelper.FinishCompletionValue(
                    value.FullName ?? value.Name,
                    (_prefix, _suffix));

                _results.Add(
                    new CompletionResult(
                        completionValue,
                        list,
                        CompletionResultType.ParameterValue,
                        tip));
            }
        }
    }
}
