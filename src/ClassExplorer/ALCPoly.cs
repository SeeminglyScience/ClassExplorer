using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

#if NETFRAMEWORK
namespace System.Runtime.Loader
{
    internal sealed class AssemblyLoadContext
    {
        public static AssemblyLoadContext Default { get; } = new();
        public bool IsCollectible => false;

        public IEnumerable<Assembly> Assemblies => AppDomain.CurrentDomain.GetAssemblies();

        public string Name => "Default";
    }
}
#endif

namespace ClassExplorer
{
    internal static class ALC
    {
        public static IEnumerable<Assembly> SafeGetAssemblies(AssemblyLoadContext alc)
        {
            IEnumerator<Assembly>? enumerator = null;
            try
            {
                try
                {
                    enumerator = alc.Assemblies?.GetEnumerator();
                }
                catch
                {
                }

                if (enumerator is null)
                {
                    yield break;
                }

                while (true)
                {
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }
                    }
                    catch
                    {
                        yield break;
                    }

                    Assembly? assembly = null;
                    try { assembly = enumerator.Current; } catch { }
                    if (assembly is not null)
                    {
                        yield return assembly;
                    }
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        public static string SafeGetName(AssemblyLoadContext alc)
        {
            string? name = null;
            try
            {
                name = alc.Name;
            }
            catch
            {
            }

            return name ?? alc.GetType().FullName;
        }

        public static void AssertSupported(PSCmdlet cmdlet)
        {
#if NETFRAMEWORK
            cmdlet.ThrowTerminatingError(
                new ErrorRecord(
                    new NotSupportedException("AssemblyLoadContext does not exist in .NET Framework."),
                    "ALCNotSupported",
                    ErrorCategory.NotEnabled,
                    null));
#endif
        }

        public static IEnumerable<AssemblyLoadContext> GetAll()
        {
#if NETFRAMEWORK
            return [AssemblyLoadContext.Default];
#else
            return AssemblyLoadContext.All;
#endif
        }

        public static AssemblyLoadContext GetLoadContext(Assembly assembly)
        {
#if NETFRAMEWORK
            return AssemblyLoadContext.Default;
#else
            return AssemblyLoadContext.GetLoadContext(assembly);
#endif
        }

        public static AssemblyLoadContext GetDefault()
        {
#if NETFRAMEWORK
            return AssemblyLoadContext.Default;
#else
            return AssemblyLoadContext.Default;
#endif
        }

        public static bool IsSupported
        {
            get
            {
#if NETFRAMEWORK
                return false;
#else
                return true;
#endif
            }
        }
    }
}
