using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Common.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Queries.Runners;

namespace Wemogy.CQRS.Queries.Registries;

public class QueryRunnerRegistry : RegistryBase<Type, TypeMethodRegistryEntry>
{
    private readonly IServiceProvider _serviceProvider;

    public QueryRunnerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> ExecuteQueryRunnerAsync<TResult>(IQuery<TResult> query)
    {
        var queryRunnerEntry = GetQueryRunnerEntry(query);
        return ExecuteQueryRunnerAsync(queryRunnerEntry, query);
    }

    private TypeMethodRegistryEntry GetQueryRunnerEntry<TResult>(IQuery<TResult> query)
    {
        var queryType = query.GetType();
        var queryRunnerEntry = GetRegistryEntry(queryType);
        return queryRunnerEntry;
    }

    private Task<TResult> ExecuteQueryRunnerAsync<TResult>(
        TypeMethodRegistryEntry queryRunnerEntry,
        IQuery<TResult> query)
    {
        var queryRunner = _serviceProvider.GetRequiredService(queryRunnerEntry.Type);
        dynamic res = queryRunnerEntry.Method.Invoke(queryRunner, new object[] { query });
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type queryType)
    {
        queryType.InheritsOrImplements(typeof(IQuery<>), out var resultType);
        var queryRunnerType =
            typeof(QueryRunner<,>).MakeGenericType(queryType, resultType?.GenericTypeArguments[0]);
        var runAsyncMethod = queryRunnerType.GetMethods().First(x => x.Name == "QueryAsync");
        return new TypeMethodRegistryEntry(queryRunnerType, runAsyncMethod);
    }
}
