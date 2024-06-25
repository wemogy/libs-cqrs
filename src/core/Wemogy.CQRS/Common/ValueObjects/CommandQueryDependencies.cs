using System;
using System.Collections.Generic;

namespace Wemogy.CQRS.Common.ValueObjects;

public class CommandQueryDependencies : Dictionary<Type, Type>
{
    public CommandQueryDependencies(Dictionary<Type, Type> entries)
        : base(entries)
    {
    }
}
