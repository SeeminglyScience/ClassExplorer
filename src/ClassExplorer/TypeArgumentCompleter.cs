using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ClassExplorer
{
    /// <summary>
    /// Provides argument completion for Type input parameters.
    /// </summary>
    public sealed class TypeArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Gets types that match the current text.
        /// </summary>
        /// <param name="wordToComplete">The current parameter value.</param>
        /// <returns>Type objects that match the current text.</returns>
        [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<Type> GetTypesForCompletion(string wordToComplete)
        {
            TypeSearchOptions options = wordToComplete?.IndexOf('.') is not null or -1
                ? new() { FullName = wordToComplete + "*" }
                : new() { Name = wordToComplete + "*" };

            List<Type> types = new();
            Search.Types(options, new ListBuilder(types)).SearchAll();
            return types;
        }

        private readonly struct ListBuilder : IEnumerationCallback<Type>
        {
            private readonly List<Type> _types;

            public ListBuilder(List<Type> types) => _types = types;

            public void Invoke(Type value) => _types.Add(value);
        }

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
                new() { Name = string.Concat(wordToComplete, "*") },
                new CreateCompletionResult(prefix, suffix, out List<CompletionResult> results))
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
                var (completionValue, list, tip) = CompletionHelper.GetCompletionValue(value);
                completionValue = CompletionHelper.FinishCompletionValue(
                    completionValue,
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
