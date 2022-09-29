using System;
using System.Collections.Generic;

namespace Wemogy.CQRS.Common.ValueObjects;

public class ScheduledCommandDependencies : Dictionary<Type, Type>
{
    public ScheduledCommandDependencies(Dictionary<Type, Type> entries)
        : base(entries)
    {
    }
}
