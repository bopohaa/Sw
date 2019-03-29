using System;
using System.Collections.Generic;
using System.Text;

namespace SwCore
{
    public interface IResolver
    {
        bool CanResolve(Type type);
        object Resolve(Type type);
    }
}
