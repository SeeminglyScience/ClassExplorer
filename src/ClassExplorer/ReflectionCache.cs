using System;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer;

internal static class ReflectionCache
{
    private static readonly Type? ExecutionContextType;

    private static readonly MethodInfo? GetExecutionContextFromTLSMethod;

    private static readonly Func<string[]>? GetUsingNamespacesFunc;

    static ReflectionCache()
    {
        ExecutionContextType = typeof(PSObject).Assembly.GetType("System.Management.Automation.ExecutionContext");
        if (ExecutionContextType is null) return;

        GetExecutionContextFromTLSMethod = typeof(PSObject).Assembly.GetType("System.Management.Automation.Runspaces.LocalPipeline")
            ?.GetMethod("GetExecutionContextFromTLS", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (GetExecutionContextFromTLSMethod is null) return;

        GetUsingNamespacesFunc = CreateGetUsingNamespaces();
    }

    public static string[] GetUsingNamespacesFromTLS()
    {
        return GetUsingNamespacesFunc?.Invoke() ?? ["System"];
    }

    private static Func<string[]>? CreateGetUsingNamespaces()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        if (GetExecutionContextFromTLSMethod is null) return null;

        MethodInfo? getEngineSessionState = ExecutionContextType
            ?.GetProperty("EngineSessionState", flags)
            ?.GetGetMethod(nonPublic: true);
        if (getEngineSessionState is null) return null;

        MethodInfo? currentScope = getEngineSessionState?.ReturnType
            ?.GetProperty("CurrentScope", flags)
            ?.GetGetMethod(nonPublic: true);
        if (currentScope is null) return null;

        MethodInfo? typeRes = currentScope?.ReturnType
            ?.GetProperty("TypeResolutionState", flags)
            ?.GetGetMethod(nonPublic: true);
        if (typeRes is null) return null;

        FieldInfo? namespaces = typeRes.GetReturnType()?.GetField("namespaces", flags);
        if (namespaces is null) return null;
        if (namespaces.FieldType != typeof(string[])) return null;

        return Expression.Lambda<Func<string[]>>(
            Expression.Field(
                Expression.Call(
                    Expression.Call(
                        Expression.Call(
                            Expression.Call(GetExecutionContextFromTLSMethod),
                            getEngineSessionState!),
                        currentScope!),
                    typeRes),
                namespaces),
            "GetUsingNamespacesDynamically",
            [])
            .Compile();
    }
}
