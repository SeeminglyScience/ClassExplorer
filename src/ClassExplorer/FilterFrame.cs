using System.Reflection;

namespace ClassExplorer
{
    /// <summary>
    /// Represents a single filter invocation including criteria.
    /// </summary>
    /// <typeparam name="TMemberType">The member type this frame can match.</typeparam>
    public class FilterFrame<TMemberType>
        where TMemberType : MemberInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterFrame{TMemberType}"/> class.
        /// </summary>
        /// <param name="filter">The filter to invoke.</param>
        /// <param name="criteria">The criteria to pass to the filter.</param>
        public FilterFrame(ReflectionFilter filter, object? criteria)
        {
            Filter = filter;
            Criteria = criteria;
        }

        /// <summary>
        /// A filter like TypeFilter and MemberFilter that can be adjusted to different types.
        /// </summary>
        /// <param name="m">The object to test for a match.</param>
        /// <param name="filterCriteria">The criteria to pass to the filter.</param>
        /// <returns>A value indicating whether the object matches.</returns>
        public delegate bool ReflectionFilter(TMemberType m, object? filterCriteria);

        /// <summary>
        /// Gets the filter to invoke.
        /// </summary>
        public ReflectionFilter Filter { get; }

        /// <summary>
        /// Gets the criteria to pass to the filter.
        /// </summary>
        public object? Criteria { get; }
    }
}
