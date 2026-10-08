namespace CivilConstruction.Application.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
