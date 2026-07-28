using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Identity;

namespace SweetFlowerShop.Infrastructure.Persistence;

/// <summary>
/// The application's primary DbContext. Responsibilities:
/// 1. Unit of Work — tracks changes, commits in a single transaction
/// 2. Entity mapping — via Fluent API configurations in Configurations/
/// 3. Identity store — extends IdentityDbContext for ASP.NET Core Identity
/// 4. Global Query Filters — automatic soft-delete filtering
///
/// Design decisions:
/// - Sealed: prevents inheritance (EF Core proxies not used), small perf gain
/// - Only Aggregate Root DbSets exposed: enforces DDD aggregate boundaries
/// - Child entities (OrderItem, CartItem, etc.) mapped via configurations but not directly queryable
/// </summary>
public sealed class FlowerShopDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // ──────────────────────────────────────────────────────────────
    // Aggregate Roots ONLY — these are the entry points for queries
    // ──────────────────────────────────────────────────────────────
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Payment> Payments => Set<Payment>();

    public FlowerShopDbContext(DbContextOptions<FlowerShopDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply Identity schema mappings first
        base.OnModelCreating(modelBuilder);

        // Auto-discover all IEntityTypeConfiguration<T> in this assembly
        // This includes configurations for child entities (OrderItem, CartItem, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlowerShopDbContext).Assembly);

        // ──────────────────────────────────────────────────────────────
        // GLOBAL QUERY FILTERS — Soft Delete
        // Applied automatically to every query on ISoftDeletable entities.
        // To bypass: context.Products.IgnoreQueryFilters().Where(...)
        // ──────────────────────────────────────────────────────────────
        ApplySoftDeleteFilters(modelBuilder);

        // ──────────────────────────────────────────────────────────────
        // SHADOW PROPERTIES — Audit Trail
        // Added to every entity implementing IAuditable.
        // Values are set automatically by AuditableEntityInterceptor.
        // ──────────────────────────────────────────────────────────────
        ApplyAuditShadowProperties(modelBuilder);

        // Move Identity tables to a separate schema to avoid namespace collision
        modelBuilder.Entity<ApplicationUser>().ToTable("Users", "identity");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles", "identity");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "identity");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "identity");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "identity");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "identity");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "identity");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Global convention: all string properties default to 256 max length
        // Individual configurations can override this
        configurationBuilder.Properties<string>().HaveMaxLength(256);

        // Global convention: all decimals use consistent precision
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    /// <summary>
    /// Scans all entity types in the model and applies a soft-delete filter
    /// to those implementing ISoftDeletable.
    ///
    /// HOW IT WORKS INTERNALLY:
    /// EF Core appends this filter as a WHERE clause to every SQL query.
    /// The filter is applied at the IQueryable level — even .Include() respects it.
    ///
    /// PERFORMANCE: The filter translates to a simple WHERE "IsDeleted" = false.
    /// With an index on IsDeleted (or a filtered index), this has near-zero overhead.
    ///
    /// BYPASS: Use .IgnoreQueryFilters() for admin views, undelete operations, or auditing.
    /// </summary>
    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            // Build the filter expression: entity => !entity.IsDeleted
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var condition = System.Linq.Expressions.Expression.Equal(
                property,
                System.Linq.Expressions.Expression.Constant(false));
            var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Scans all entity types implementing IAuditable and adds shadow properties
    /// for audit tracking. These properties have NO corresponding C# class member —
    /// they exist only in EF Core's metadata and the database schema.
    ///
    /// PROPERTIES ADDED:
    /// - "CreatedAtUtc" (DateTime) — when the record was created
    /// - "ModifiedAtUtc" (DateTime?) — when last modified (null if never modified)
    /// - "CreatedBy" (string?) — who created it
    /// - "ModifiedBy" (string?) — who last modified it
    ///
    /// VALUES ARE SET BY: AuditableEntityInterceptor (SaveChanges pipeline)
    /// </summary>
    private static void ApplyAuditShadowProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property<DateTime>(AuditShadowProperties.CreatedAtUtc)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                builder.Property<DateTime?>(AuditShadowProperties.ModifiedAtUtc);

                builder.Property<string?>(AuditShadowProperties.CreatedBy)
                    .HasMaxLength(256);

                builder.Property<string?>(AuditShadowProperties.ModifiedBy)
                    .HasMaxLength(256);
            });
        }
    }
}

