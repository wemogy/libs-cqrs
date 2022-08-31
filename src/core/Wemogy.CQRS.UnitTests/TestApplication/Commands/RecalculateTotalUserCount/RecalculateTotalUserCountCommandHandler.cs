using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.RecalculateTotalUserCount;

public class RecalculateTotalUserCountCommandHandler : ICommandHandler<RecalculateTotalUserCountCommand, int>
{
    public Task<int> HandleAsync(RecalculateTotalUserCountCommand command)
    {
        throw new NotImplementedException();
    }
}
