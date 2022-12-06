using System;
namespace Bakhoo;

public interface IBakhooJobHandler {}

public interface IBakhooJobHandler<TJobType> : IBakhooJobHandler
{
    Task Handle(TJobType job, CancellationToken ct);
}
