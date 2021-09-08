using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ClassExplorer.Signatures;

namespace ClassExplorer;

internal abstract class ReflectionSearch<TMemberType, TCallback, TOptions>
    where TMemberType : MemberInfo
    where TCallback : struct, IEnumerationCallback<TMemberType>
    where TOptions : ReflectionSearchOptions
{
    private readonly TCallback _callback;

    private readonly Filter<TMemberType>[] _filters;

    protected readonly TOptions _options;

    protected bool WasAccessSpecified { get; }

    protected ReflectionSearch(TOptions options, TCallback callback)
    {
        _options = options;
        _callback = callback;

        List<Filter<TMemberType>> filters = new();
        if (_options.AccessView is 0)
        {
            _options.AccessView = AccessView.External;
        }
        else
        {
            WasAccessSpecified = true;
        }

        if (_options.Force)
        {
            _options.AccessView = AccessView.This;
        }

        InitCommonFilters(filters);
        InitializeFilters(filters);

        if (_options.FilterScript is not null)
        {
            filters.AddFilter(
                new FilterScriptSignature(FilterScriptPipe.Create(_options.FilterScript)),
                static (member, state) => state.IsMatch(member));
        }

        _filters = filters.ToArray();
    }

    public abstract void SearchAll();

    public virtual void SearchSingleObject(PSObject pso)
    {
        if (pso.BaseObject is not TMemberType member)
        {
            return;
        }

        AggregateFilter(member, this);
    }

    private void InitCommonFilters(List<Filter<TMemberType>> filters)
    {
        if (_options.Name is { Length: > 0 })
        {
            if (_options.RegularExpression)
            {
                filters.AddFilter(
                    StringMatcher.CreateRegex(_options.Name),
                    static (m, s) => Unsafe.As<Regex>(s).IsMatch(m.Name));
            }
            else
            {
                filters.AddFilter(
                    StringMatcher.Create(_options.Name),
                    static (m, s) => Unsafe.As<StringMatcher>(s).IsMatch(m.Name));
            }
        }
    }

    protected abstract void InitializeFilters(List<Filter<TMemberType>> filters);

    protected static bool AggregateFilter(TMemberType member, object? state, bool isPipeFilter = false)
    {
        var context = Unsafe.As<ReflectionSearch<TMemberType, TCallback, TOptions>>(state);
        if (context._filters.Length is 0)
        {
            context._callback.Invoke(member);
            return false;
        }

        bool not = context._options.Not;
        foreach (Filter<TMemberType> filter in context._filters)
        {
            if (not && (filter.Options & FilterOptions.ExcludeNot) is not 0)
            {
                continue;
            }

            if (isPipeFilter && (filter.Options & FilterOptions.ExcludePipeFilter) is not 0)
            {
                continue;
            }

            if (filter.Func(member, filter.State))
            {
                if (not)
                {
                    return false;
                }

                continue;
            }

            if (not)
            {
                continue;
            }

            return false;
        }

        context._callback.Invoke(member);
        return false;
    }
}
