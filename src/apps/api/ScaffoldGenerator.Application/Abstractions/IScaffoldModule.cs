using ScaffoldGenerator.Contracts.Requests;

namespace ScaffoldGenerator.Application.Abstractions;

public interface IScaffoldModule
{
    string Name { get; }

    int Order { get; }

    bool IsEnabled(GenerateScaffoldRequest request);

    Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct = default);
}
