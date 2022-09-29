using System.Collections.Generic;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommandDependencyResolver
{
    List<ScheduledCommandDependency> ResolveDependencies();
}
