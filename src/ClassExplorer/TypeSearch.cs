using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Signatures;

namespace ClassExplorer;

internal sealed class TypeSearch<TCallback> : ReflectionSearch<Type, TCallback, TypeSearchOptions>
    where TCallback : struct, IEnumerationCallback<Type>
{
    public TypeSearch(TypeSearchOptions options, TCallback callback)
        : base(options, callback)
    {
    }

    public override void SearchAll()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            ProcessAssembly(assembly);
        }
    }

    private void ProcessAssembly(Assembly assembly)
    {
        Module[] modules;
        try
        {
            modules = assembly.GetModules();
        }
        catch (ReflectionTypeLoadException)
        {
            return;
        }

        foreach (Module module in modules)
        {
            try
            {
                module.FindTypes(static (m, fc) => AggregateFilter(m, fc), this);
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }
    }

    public override void SearchSingleObject(PSObject pso)
    {
        if (pso.BaseObject is Assembly assembly)
        {
            ProcessAssembly(assembly);
            return;
        }

        if (pso.BaseObject is Type type)
        {
            AggregateFilter(type, this);
            return;
        }

        AggregateFilter(pso.BaseObject.GetType(), this);
    }

    protected override void InitializeFastFilters(List<Filter<Type>> filters, SignatureParser parser)
    {
        if (_options.Static || (_options.Abstract && _options.Sealed))
        {
            filters.AddFilter(static (type, _) => type.IsAbstract && type.IsSealed);
        }
        else if (_options.Abstract)
        {
            filters.AddFilter(static (type, _) => type.IsAbstract && !type.IsSealed && !type.IsInterface);
        }
        else if (_options.Sealed)
        {
            filters.AddFilter(static (type, _) => !type.IsAbstract && type.IsSealed);
        }
        else if (_options.Interface)
        {
            filters.AddFilter(static (type, _) => type.IsInterface);
        }
        else if (_options.ValueType)
        {
            filters.AddFilter(static (type, _) => type.IsValueType);
        }

        if (_options.AccessView is not AccessView.This)
        {
            filters.AddFilter(
                new StrongBox<AccessView>(_options.AccessView),
                static (type, view) => type.DoesMatchView(view.Value),
                WasAccessSpecified ? default : FilterOptions.ExcludeNot | FilterOptions.ExcludePipeFilter);
        }
    }

    protected override void InitializeOtherFilters(List<Filter<Type>> filters, SignatureParser parser)
    {
        if (_options.Namespace is not null)
        {
            if (_options.RegularExpression)
            {
                filters.AddFilter(
                    StringMatcher.CreateRegex(_options.Namespace),
                    static (type, matcher) => matcher.IsMatch(type.Namespace ?? ""));
            }
            else
            {
                filters.AddFilter(
                    StringMatcher.Create(_options.Namespace),
                    static (type, matcher) => matcher.IsMatch(type.Namespace ?? ""));
            }
        }

        if (_options.FullName is not null)
        {
            if (_options.RegularExpression)
            {
                filters.AddFilter(
                    StringMatcher.CreateRegex(_options.FullName),
                    static (type, matcher) => matcher.IsMatch(type.FullName));
            }
            else
            {
                filters.AddFilter(
                    StringMatcher.Create(_options.FullName),
                    static (type, matcher) => matcher.IsMatch(type.FullName));
            }
        }

        if (_options.Signature is not null)
        {
            filters.AddFilter(
                _options.Signature,
                static (type, signature) => signature.IsMatch(type));
        }
    }
}
