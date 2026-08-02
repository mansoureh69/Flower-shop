using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Tests.TestDoubles;

internal sealed class StubCurrentUserService : ICurrentUserService
{
    public StubCurrentUserService(Guid? userId)
    {
        UserId = userId;
    }

    public Guid? UserId { get; }
    public string? UserName => UserId is null ? null : "customer@example.com";
}

internal abstract class RepositoryFake<T> : IRepository<T> where T : AggregateRoot
{
    public List<T> Added { get; } = [];
    public List<T> Updated { get; } = [];
    public List<T> Removed { get; } = [];

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<T?>(null);

    public virtual Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>([]);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Added.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity) => Updated.Add(entity);
    public void Remove(T entity) => Removed.Add(entity);
}

internal sealed class ProductRepositoryFake(params Product[] products)
    : RepositoryFake<Product>, IProductRepository
{
    private readonly IReadOnlyDictionary<Guid, Product> _products =
        products.ToDictionary(product => product.Id);

    public int GetByIdCalls { get; private set; }

    public override Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetByIdCalls++;
        return Task.FromResult(_products.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Product>>(
            _products.Values.Where(product => product.CategoryId == categoryId).ToList());

    public Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Product>>(
            _products.Values.Where(product => product.IsAvailable).ToList());
}

internal sealed class OrderRepositoryFake : RepositoryFake<Order>, IOrderRepository
{
    public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Order>>([]);
}

internal sealed class CartRepositoryFake(Cart? existingCart = null)
    : RepositoryFake<Cart>, ICartRepository
{
    public Guid? RequestedCustomerId { get; private set; }

    public Task<Cart?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        RequestedCustomerId = customerId;
        return Task.FromResult(
            existingCart?.CustomerId == customerId ? existingCart : null);
    }
}

internal sealed class UnitOfWorkFake : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await operation(cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }
}
