using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// Provides common parameters and setup for the Find- cmdlets.
    /// </summary>
    /// <typeparam name="TMemberType">The member type that the cmdlet can match.</typeparam>
    public abstract class FindReflectionObjectCommandBase<TMemberType> : Cmdlet
        where TMemberType : MemberInfo
    {
        private readonly List<FilterFrame<TMemberType>> _filters = new List<FilterFrame<TMemberType>>();

        /// <summary>
        /// Gets or sets a ScriptBlock to invoke as a predicate filter.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        public virtual ScriptBlock FilterScript { get; set; }

        /// <summary>
        /// Gets or sets the name to match.
        /// </summary>
        [Parameter(Position = 0)]
        [SupportsWildcards]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

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
        [Alias("Regex")]
        public virtual SwitchParameter RegularExpression { get; set; }

        /// <summary>
        /// Gets or sets the object passed from the pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public virtual PSObject InputObject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid input has been processed from the pipeline.
        /// </summary>
        protected bool HasHadInput { get; set; }

        /// <summary>
        /// Gets a list of filters to use for matching.
        /// </summary>
        protected List<FilterFrame<TMemberType>> Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// A filter that matches the Name property of the object passed using PowerShell wildcard
        /// matching.
        /// </summary>
        /// <param name="m">The object to test for a match.</param>
        /// <param name="filterCriteria">The wildcard pattern to test.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected static bool WildcardNameFilter(TMemberType m, object filterCriteria)
        {
            WildcardPattern pattern = filterCriteria as WildcardPattern;

            return pattern == null ? false : pattern.IsMatch(m.Name);
        }

        /// <summary>
        /// A filter that matches the Name property of the object passed using regular expressions.
        /// </summary>
        /// <param name="m">The object to test for a match.</param>
        /// <param name="filterCriteria">The regex pattern to test.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected static bool RegexNameFilter(TMemberType m, object filterCriteria)
        {
            Regex pattern = filterCriteria as Regex;

            return pattern == null ? false : pattern.IsMatch(m.Name);
        }

        /// <summary>
        /// The BeginProcessing method.
        /// </summary>
        protected override void BeginProcessing()
        {
            InitializeFilters();
        }

        /// <summary>
        /// The ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (InputObject == null) return;

            HasHadInput = true;
            ProcessSingleObject(InputObject);
        }

        /// <summary>
        /// A filter that invokes all filters set during initialization. This is the main filter
        /// to be based to the Module.FindTypes or Type.FindMembers methods
        /// </summary>
        /// <param name="m">The object to test for a match.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected bool AggregateFilter(TMemberType m, object filterCriteria)
        {
            foreach (var frame in Filters)
            {
                bool filterResult = frame.Filter.Invoke(m, frame.Criteria);
                if (!filterResult) return false;
            }

            return true;
        }

        /// <summary>
        /// A filter that matches only public objects.
        /// </summary>
        /// <param name="m">The object to test for a match.</param>
        /// <param name="filterCriteria">The parameter is not used.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        protected abstract bool PublicFilter(TMemberType m, object filterCriteria);

        /// <summary>
        /// Process a single non-null object from the pipeline.
        /// </summary>
        /// <param name="input">The object from the pipeline.</param>
        protected virtual void ProcessSingleObject(PSObject input)
        {
            TMemberType member = input.BaseObject as TMemberType;
            if (member == null || !AggregateFilter(member, null)) return;
            WriteObject(member);
        }

        /// <summary>
        /// Process parameters and create the list of filters.
        /// </summary>
        protected abstract void InitializeFilters();

        /// <summary>
        /// Process a string parameter that supports wildcards. If the RegularExpressions input
        /// parameter is specified, the regexMethod will be used. Otherwise the wildcard method.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="wildcardMethod">The filter to use for wildcard matching.</param>
        /// <param name="regexMethod">The filter to use for regex matching.</param>
        protected void ProcessName(
            string pattern,
            FilterFrame<TMemberType>.ReflectionFilter wildcardMethod,
            FilterFrame<TMemberType>.ReflectionFilter regexMethod)
        {
            if (pattern == null) return;

            if (RegularExpression.IsPresent)
            {
                var regexPattern = new Regex(pattern, RegexOptions.IgnoreCase);
                Filters.Add(CreateFrame(regexMethod, regexPattern));
                return;
            }

            var wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            Filters.Add(CreateFrame(wildcardMethod, wildcardPattern));
        }

        /// <summary>
        /// Creates a filter frame to add to the filter list.
        /// </summary>
        /// <param name="filter">The filter to invoke.</param>
        /// <param name="filterCriteria">The criteria to pass to the filter.</param>
        /// <returns>The created filter frame.</returns>
        protected FilterFrame<TMemberType> CreateFrame(
            FilterFrame<TMemberType>.ReflectionFilter filter,
            object filterCriteria)
        {
            return new FilterFrame<TMemberType>(filter, filterCriteria);
        }

        /// <summary>
        /// Process an input parameter for a filter with no criteria.
        /// </summary>
        /// <param name="filter">The filter to invoke.</param>
        /// <param name="shouldAdd">A value indicating if the filter should be added.</param>
        protected void ProcessParameter(
            FilterFrame<TMemberType>.ReflectionFilter filter,
            bool shouldAdd)
        {
            if (!shouldAdd) return;

            Filters.Add(CreateFrame(filter, null));
        }

        /// <summary>
        /// Process a parameter that passes criteria to the filter.
        /// </summary>
        /// <param name="filter">The filter to invoke.</param>
        /// <param name="value">The criteria to pass to the filter.</param>
        /// <typeparam name="TCriteria">The type of the criteria.</typeparam>
        protected void ProcessParameter<TCriteria>(
            FilterFrame<TMemberType>.ReflectionFilter filter,
            TCriteria value)
        {
            if (value == null) return;

            Filters.Add(CreateFrame(filter, value));
        }
    }
}
