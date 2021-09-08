using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Text.RegularExpressions;
using ClassExplorer.Commands;
using Microsoft.PowerShell;

namespace ClassExplorer
{
    /// <summary>
    /// Provides argument completion for Type input parameters.
    /// </summary>
    public class TypeArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Gets types that match the current text.
        /// </summary>
        /// <param name="wordToComplete">The current parameter value.</param>
        /// <returns>Type objects that match the current text.</returns>
        public static IEnumerable<Type> GetTypesForCompletion(string wordToComplete)
        {
            List<Type> types = new();
            Search.Types(
                new TypeSearchOptions() { Name = wordToComplete + "*" },
                new ListBuilder(types))
                .SearchAll();

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
            return GetTypesForCompletion(wordToComplete).Select(static (type) => NewResult(type));
        }

        private static CompletionResult NewResult(Type type)
        {
            if (type.IsGenericType)
            {
                StringBuilder name = new(Regex.Replace(type.FullName, @"`\d+$", string.Empty));
                name.Append("[any");
                int arity = type.GetGenericArguments().Length;
                for (int i = 1; i < arity; i++)
                {
                    name.Append(", any");
                }

                name.Append(']');
                string fullName = name.ToString();
                return new CompletionResult(
                    fullName,
                    fullName,
                    CompletionResultType.ParameterValue,
                    fullName);
            }

            return new CompletionResult(
                ToStringCodeMethods.Type(PSObject.AsPSObject(type)),
                type.FullName,
                CompletionResultType.ParameterValue,
                type.FullName);
        }
    }
}
