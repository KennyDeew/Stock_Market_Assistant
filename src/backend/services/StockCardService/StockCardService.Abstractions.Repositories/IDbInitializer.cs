namespace StockCardService.Abstractions.Repositories
{
    public interface IDbInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
