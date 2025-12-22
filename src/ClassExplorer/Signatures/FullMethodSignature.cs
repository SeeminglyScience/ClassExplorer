using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures;

internal sealed class FullMethodSignature(ITypeSignature returnType, ImmutableArray<ITypeSignature> parameters) : MemberSignature
{
    internal ITypeSignature ReturnType { get; } = returnType;

    internal ImmutableArray<ITypeSignature> Parameters { get; } = parameters;

    public override bool IsMatch(MemberInfo subject)
    {
        if (subject is not MethodBase methodBase)
        {
            return false;
        }

        if (subject is ConstructorInfo ctor)
        {
            if (ctor.ReflectedType is null || !ReturnType.IsMatch(ctor.ReflectedType))
            {
                return false;
            }
        }
        else if (subject is MethodInfo method)
        {
            if (!ReturnType.IsMatch(method.ReturnParameter))
            {
                return false;
            }
        }

        ParameterInfo[] parameters = methodBase.GetParameters();
        if (parameters.Length != Parameters.Length)
        {
            return false;
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (!Parameters[i].IsMatch(parameters[i]))
            {
                return false;
            }
        }

        return true;
    }
}
