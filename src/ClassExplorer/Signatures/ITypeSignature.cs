using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal interface ITypeSignature
    {
        bool IsMatch(ParameterInfo parameter);

        bool IsMatch(Type subject);
    }
}
