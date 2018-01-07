using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ClassExplorer.Commands;

namespace ClassExplorer
{
    /// <summary>
    /// Provides a representation of a namespace.
    /// </summary>
    public class NamespaceInfo : MemberInfo
    {
        private static readonly object[] s_emptyArray = new object[0] { };

        private static readonly Lazy<NamespaceCache> s_namespaceCache =
            new Lazy<NamespaceCache>(() => new NamespaceCache());

        private readonly List<Assembly> _assemblies = new List<Assembly>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceInfo" /> class.
        /// </summary>
        /// <param name="fullName">The full name of the namespace.</param>
        /// <param name="assemblies">The assemblies that declare types within this namespace.</param>
        internal NamespaceInfo(string fullName, IEnumerable<Assembly> assemblies)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentNullException(nameof(fullName));
            if (assemblies == null || !assemblies.Any()) throw new ArgumentNullException(nameof(assemblies));
            Name = fullName.Split('.').Last();
            FullName = fullName;
            _assemblies.AddRange(assemblies);
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the last section of the namespace. For example, the name of the namespace
        /// System.Management.Automation would be Automation.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        /// Gets the assemblies that declare classes within this namespace.
        /// </summary>
        public ReadOnlyCollection<Assembly> Assemblies => _assemblies.AsReadOnly();

        /// <summary>
        /// Gets the type of member. This is always "Custom".
        /// </summary>
        public override MemberTypes MemberType { get; } = MemberTypes.Custom;

        /// <summary>
        /// Gets the declaring type. This is always <see cref="NamespaceInfo" />.
        /// </summary>
        public override Type DeclaringType => typeof(NamespaceInfo);

        /// <summary>
        /// Gets the reflected type. This is always <see cref="NamespaceInfo" />.
        /// </summary>
        public override Type ReflectedType => typeof(NamespaceInfo);

        /// <summary>
        /// Gets a dictionary of all cached namespaces.
        /// </summary>
        internal static Dictionary<string, NamespaceInfo> Namespaces => s_namespaceCache.Value._namespaces;

        /// <summary>
        /// Gets all cached namespace names.
        /// </summary>
        internal static IEnumerable<string> CachedNames => s_namespaceCache.Value._names;

        /// <summary>
        /// Gets custom attributes for the member. This currently always returns an empty array.
        /// </summary>
        /// <param name="inherit">The parameter is not used.</param>
        /// <returns>An empty array.</returns>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return s_emptyArray;
        }

        /// <summary>
        /// Gets custom attributes for the member. This currently always returns an empty array.
        /// </summary>
        /// <param name="attributeType">The parameter is not used.</param>
        /// <param name="inherit">The parameter is not used.</param>
        /// <returns>An empty array.</returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return s_emptyArray;
        }

        /// <summary>
        /// Tests if an attribute is defined for this member. This currently always returns false.
        /// </summary>
        /// <param name="attributeType">The parameter is not used.</param>
        /// <param name="inherit">The parameter is not used.</param>
        /// <returns>A value indicating whether the attribute is defined.</returns>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        /// <summary>
        /// Returns the full name of the namespace.
        /// </summary>
        /// <returns>A string containing the full name of the namespace.</returns>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Gets all namespaces in the current AppDomain.
        /// </summary>
        /// <returns>A collection of namespaces.</returns>
        internal static ReadOnlyCollection<NamespaceInfo> GetNamespaces()
        {
            return new ReadOnlyCollection<NamespaceInfo>(
                new List<NamespaceInfo>(Namespaces.Values));
        }

        /// <summary>
        /// Adds an assembly to the list of assemblies that declare types within this namespace.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
        internal void AddAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            _assemblies.Add(assembly);
        }

        private class NamespaceCache
        {
            internal readonly HashSet<string> _names = new HashSet<string>();

            internal readonly Dictionary<string, NamespaceInfo> _namespaces;

            internal NamespaceCache()
            {
                _names = new HashSet<string>();
                _namespaces = LoadNamespaces();
                foreach (var name in _namespaces.Values)
                {
                    _names.Add(name.Name);
                }
            }

            private static IEnumerable<NamespaceInfo> GetNamespaces(IEnumerable<Type> types)
            {
                return types.GroupBy(type => type.Namespace)
                    .Where(group => !string.IsNullOrWhiteSpace(group.Key))
                    .Select(
                        group => new NamespaceInfo(
                            group.Key,
                            group.Select(type => type.Assembly).Distinct()));
            }

            private Dictionary<string, NamespaceInfo> LoadNamespaces()
            {
                AppDomain.CurrentDomain.AssemblyLoad += AppDomain_OnAssemblyLoad;
                try
                {
                    return GetNamespaces(new FindTypeCommand().Invoke<Type>())
                        .ToDictionary(ns => ns.FullName);
                }
                catch (Exception)
                {
                    // TODO: Better handling
                    AppDomain.CurrentDomain.AssemblyLoad -= AppDomain_OnAssemblyLoad;
                    throw;
                }
            }

            private void AppDomain_OnAssemblyLoad(object source, AssemblyLoadEventArgs e)
            {
                foreach (var ns in GetNamespaces(e.LoadedAssembly.GetTypes()))
                {
                    if (_namespaces.TryGetValue(ns.FullName, out NamespaceInfo existing))
                    {
                        existing.AddAssembly(e.LoadedAssembly);
                        continue;
                    }

                    _namespaces.Add(ns.FullName, ns);
                    _names.Add(ns.Name);
                }
            }
        }
    }
}
