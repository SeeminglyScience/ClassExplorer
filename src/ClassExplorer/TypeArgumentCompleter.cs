using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
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
            return new FindTypeCommand() { Name = wordToComplete + "*" }.Invoke<Type>();
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
                return new CompletionResult(
                    type.FullName,
                    type.FullName,
                    CompletionResultType.ParameterValue,
                    type.FullName);
            }

            return new CompletionResult(
                ToStringCodeMethods.Type(PSObject.AsPSObject(type)),
                type.FullName,
                CompletionResultType.ParameterValue,
                type.FullName);
        }
    }
}
