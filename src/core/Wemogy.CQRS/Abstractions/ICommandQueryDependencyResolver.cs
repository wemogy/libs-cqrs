using System.Collections.Generic;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Abstractions;

public interface ICommandQueryDependencyResolver
{
    List<CommandQueryDependency> ResolveDependencies();
}
