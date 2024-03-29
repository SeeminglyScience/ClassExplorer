using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Signatures;

namespace ClassExplorer;

internal sealed class MemberSearch<TCallback> : ReflectionSearch<MemberInfo, TCallback, MemberSearchOptions>
    where TCallback : struct, IEnumerationCallback<MemberInfo>
{
    private readonly HashSet<Type> _processedTypes = new();

    private BindingFlags _flags;

    private bool WasMemberTypeSpecified;

    public MemberSearch(MemberSearchOptions options, TCallback callback)
        : base(options, callback)
    {
    }

    public override void SearchAll()
    {
        Search.Types(
            new TypeSearchOptions() { Force = _options.Force },
            new EnumerateMembersFromTypes(this))
            .SearchAll();
    }

    private readonly struct EnumerateMembersFromTypes : IEnumerationCallback<Type>
    {
        private readonly MemberSearch<TCallback> _parent;

        public EnumerateMembersFromTypes(MemberSearch<TCallback> parent)
        {
            _parent = parent;
        }

        public void Invoke(Type value)
        {
            _parent.SearchSingleType(value);
        }
    }

    public override void SearchSingleObject(PSObject pso)
    {
        if (pso.BaseObject is Type type && (_options.RecurseNestedType || !type.IsNested))
        {
            SearchSingleType(type);
            return;
        }

        if (pso.BaseObject is MemberInfo member)
        {
            if (WasAccessSpecified)
            {
                if (member.DoesMatchView(_options.AccessView))
                {
                    if (_options.Not)
                    {
                        return;
                    }
                }
                else if (!_options.Not)
                {
                    return;
                }
            }

            if (_options.Not)
            {
                bool isStatic = IsStatic(member);
                if (_options.Static && isStatic)
                {
                    return;
                }

                if (_options.Instance && !isStatic)
                {
                    return;
                }

                if (WasMemberTypeSpecified && (_options.MemberType & member.MemberType) is not 0)
                {
                    return;
                }
            }
            else
            {
                if ((_options.MemberType & member.MemberType) is 0 || !MatchesBindingFlags(member))
                {
                    return;
                }
            }

            AggregateFilter(member, this, isPipeFilter: true);
            return;
        }

        Type targetType = pso.BaseObject.GetType();
        if (!_processedTypes.Add(targetType))
        {
            return;
        }

        SearchSingleType(targetType);
    }

    private bool MatchesBindingFlags(MemberInfo member)
    {
        const BindingFlags staticInstance = BindingFlags.Static | BindingFlags.Instance;
        if ((_flags & staticInstance) is staticInstance)
        {
            return true;
        }

        if (IsStatic(member))
        {
            return (_flags & BindingFlags.Static) is not 0;
        }

        return (_flags & BindingFlags.Instance) is not 0;
    }

    private static bool IsStatic(MemberInfo member)
    {
        return member switch
        {
            MethodBase method => method.IsStatic,
            PropertyInfo property => property.GetFirstMethod()?.IsStatic is true,
            EventInfo eventInfo => eventInfo.GetFirstMethod()?.IsStatic is true,
            FieldInfo field => field.IsStatic,
            _ => true,
        };
    }

    private void SearchSingleType(Type type)
    {
        if (_options.Extension && !_options.Not && !type.IsDefined(typeof(ExtensionAttribute)))
        {
            return;
        }

        type.FindMembers(
            _options.MemberType,
            _flags,
            static (m, fc) => AggregateFilter(m, fc),
            filterCriteria: this);
    }

    protected override void InitializeFastFilters(List<Filter<MemberInfo>> filters, SignatureParser parser)
    {
        _flags = BindingFlags.Instance | BindingFlags.Static;
        if ((_options.AccessView & AccessView.Public) is not 0)
        {
            _flags |= BindingFlags.Public;
        }

        if (_options.AccessView is not AccessView.External)
        {
            _flags |= BindingFlags.NonPublic;
            if (_options.AccessView is not AccessView.This)
            {
                filters.AddFilter(
                    new StrongBox<AccessView>(_options.AccessView),
                    static (m, view) => m.DoesMatchView(view.Value),
                    FilterOptions.ExcludePipeFilter);
            }
        }

        if (_options.Static && !_options.Instance)
        {
            _flags &= ~BindingFlags.Instance;
        }

        if (_options.Instance && !_options.Static)
        {
            _flags &= ~BindingFlags.Static;
        }

        WasMemberTypeSpecified = true;
        if (_options.MemberType is 0)
        {
            WasMemberTypeSpecified = false;
            _options.MemberType = MemberTypes.All;
        }
        else if (_options.Not)
        {
            _options.MemberType = MemberTypes.All & ~_options.MemberType;
        }

        if (!_options.IncludeSpecialName)
        {
            filters.AddFilter(
                static (member, _) => member is not MethodInfo method || !method.IsSpecialName,
                FilterOptions.DoNotInverseNot | FilterOptions.ExcludePipeFilter);
        }

        if (_options.Abstract && _options.Virtual)
        {
            filters.AddFilter(static (member, _) => member.IsVirtualOrAbstract());
        }
        else if (_options.Abstract)
        {
            filters.AddFilter(static (member, _) => member.IsAbstract());
        }
        else if (_options.Virtual)
        {
            filters.AddFilter(static (member, _) => member.IsVirtual());
        }

        if (_options.Declared)
        {
            filters.AddFilter(static (member, _) => member.DeclaringType == member.ReflectedType);
        }

        if (_options.Extension)
        {
            filters.AddFilter(static (member, _) => member.IsDefined(typeof(ExtensionAttribute)));
        }

        if (!_options.IncludeObject)
        {
            filters.AddFilter(
                static (member, _) =>
                {
                    // Show members by default if the caller is querying the
                    // class directly.
                    Type? reflectedType = member.ReflectedType;
                    if (reflectedType == typeof(object)
                        || reflectedType == typeof(ValueType)
                        || reflectedType == typeof(Enum))
                    {
                        return true;
                    }

                    // Otherwise check the declaring type. This tells us if the
                    // method is overridden. Also check for ValueType and Enum
                    // as we typically don't want to see their implementations
                    // either.
                    Type? declaringType = member.DeclaringType;
                    return declaringType != typeof(object)
                        && declaringType != typeof(ValueType)
                        && declaringType != typeof(Enum);
                },
                FilterOptions.DoNotInverseNot | FilterOptions.ExcludePipeFilter);
        }
    }

    protected override void InitializeOtherFilters(List<Filter<MemberInfo>> filters, SignatureParser parser)
    {
        if (_options.ParameterCount is { Length: > 0 })
        {
            filters.AddFilter(
                new ParameterCountSignature(_options.ParameterCount),
                static (member, signature) => signature.IsMatch(member));
        }

        if (_options.GenericParameterCount is { Length: > 0 })
        {
            filters.AddFilter(
                new GenericParameterCountSignature(_options.GenericParameterCount),
                static (member, signature) => signature.IsMatch(member));
        }

        if (_options.GenericParameter is not null)
        {
            filters.AddFilter(
                new GenericParameterTypeSignature(_options.GenericParameter.Resolve(parser)),
                static (member, signature) => signature.IsMatch(member));
        }

        if (_options.ParameterType is not null)
        {
            filters.AddFilter(
                new ParameterTypeSignature(_options.ParameterType.Resolve(parser)),
                static (member, signature) => signature.IsMatch(member));
        }

        if (_options.ReturnType is not null)
        {
            filters.AddFilter(
                new ReturnTypeSignature(_options.ReturnType.Resolve(parser)),
                static (member, signature) => signature.IsMatch(member));
        }

        if (_options.Decoration is not null)
        {
            filters.AddFilter(
                new DecorationSignature(
                    SignatureParser.ResolveAttributeTypeName(
                        _options.Decoration,
                        _options.ResolutionMap)),
                static (member, signature) => signature.IsMatch(member));
        }
    }
}
