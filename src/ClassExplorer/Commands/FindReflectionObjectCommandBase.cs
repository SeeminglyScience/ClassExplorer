using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using ClassExplorer.Signatures;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// Provides common parameters and setup for the Find- cmdlets.
    /// </summary>
    /// <typeparam name="TMemberType">The member type that the cmdlet can match.</typeparam>
    public abstract class FindReflectionObjectCommandBase<TMemberType> : PSCmdlet
        where TMemberType : MemberInfo
    {
        /// <summary>
        /// Gets or sets a ScriptBlock to invoke as a predicate filter.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName="ByFilter")]
        [Parameter(ParameterSetName="ByName")]
        [ValidateNotNull]
        public virtual ScriptBlock FilterScript { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name to match.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName="ByName")]
        [Parameter(ParameterSetName="ByFilter")]
        [SupportsWildcards]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to include nonpublic members.
        /// </summary>
        [Parameter]
        [Alias("IncludeNonPublic", "F")]
        public virtual SwitchParameter Force { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use regular expressions to match parameters
        /// that support wildcards.
        /// </summary>
        [Parameter]
        [Alias("Regex", "re")]
        public virtual SwitchParameter RegularExpression { get; set; }

        /// <summary>
        /// Gets or sets the object passed from the pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public virtual PSObject InputObject { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the match should be negated.
        /// </summary>
        [Parameter]
        public virtual SwitchParameter Not { get; set; }

        [Parameter]
        [Alias("map")]
        public virtual Hashtable ResolutionMap { get; set; } = null!;

        [Parameter]
        [Alias("as")]
        public virtual AccessView AccessView { get; set; }

        private bool _hadError;

        /// <summary>
        /// The BeginProcessing method.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Not sure why invocation info isn't public, but it's impossible to know pipeline
            // position without it.  The long term fix is to move all of the logic in these cmdlets
            // to public APIs and inherit PSCmdlet instead of Cmdlet.  I should have done that in the
            // first place, but that's hindsight for ya.
            try
            {
                InitializeFilters();
            }
            catch (SignatureParseException spe)
            {
                ParseException parseException = new(
                    new[]
                    {
                        new ParseError(spe.ErrorPosition, "SignatureParseError", spe.Message)
                    });

                WriteError(new ErrorRecord(parseException.ErrorRecord, parseException));
                _hadError = true;
                return;
            }
            catch (PSInvalidCastException ice)
            {
                WriteError(new ErrorRecord(ice.ErrorRecord, ice));
                _hadError = true;
                return;
            }

            if (MyInvocation.ExpectingInput || InputObject is not null)
            {
                return;
            }

            OnNoInput();
        }

        /// <summary>
        /// The ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (InputObject == null || _hadError) return;

            // Support `Find-X -InputObject $memberList` syntax. This will be less performant but
            // you probably aren't passing the entire AppDomain like this.
            if (InputObject.BaseObject is IList list)
            {
                foreach (var item in list)
                {
                    ProcessSingleObject(PSObject.AsPSObject(item));
                }

                return;
            }

            ProcessSingleObject(InputObject);
        }

        /// <summary>
        /// Process a single non-null object from the pipeline.
        /// </summary>
        /// <param name="input">The object from the pipeline.</param>
        protected abstract void ProcessSingleObject(PSObject input);

        /// <summary>
        /// Process parameters and create the list of filters.
        /// </summary>
        protected abstract void InitializeFilters();

        private protected abstract void OnNoInput();

        private protected Dictionary<string, ScriptBlockStringOrType>? InitializeResolutionMap()
        {
            if (ResolutionMap is null)
            {
                return null;
            }

            Dictionary<string, ScriptBlockStringOrType> resolutionMap = new(
                ResolutionMap.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (object? item in ResolutionMap)
            {
                DictionaryEntry entry = (DictionaryEntry)item!;
                string key = LanguagePrimitives.ConvertTo<string>(entry.Key);
                if (entry.Value is null)
                {
                    WriteError(
                        new ErrorRecord(
                            new PSInvalidCastException(SR.ResolveMapNullValue),
                            nameof(SR.ResolveMapNullValue),
                            ErrorCategory.InvalidArgument,
                            entry));
                    continue;
                }

                // if (!LanguagePrimitives.TryConvertTo(entry.Value, out Type value))
                // {
                //     WriteError(
                //         new ErrorRecord(
                //             new PSInvalidCastException(
                //                 SR.Format(
                //                     "Cannot convert the \"{0}\" value of type \"{1}\" to type \"{1}\".",
                //                     entry.Value,
                //                     entry.Value.GetType().FullName)),
                //             "InvalidTypeResolutionMap",
                //             ErrorCategory.InvalidArgument,
                //             entry));
                // }

                resolutionMap[key] = LanguagePrimitives.ConvertTo<ScriptBlockStringOrType>(entry.Value);
            }

            return resolutionMap;
        }
    }
}
