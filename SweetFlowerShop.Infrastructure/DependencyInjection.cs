using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Infrastructure.Authentication;
using SweetFlowerShop.Infrastructure.Identity;
using SweetFlowerShop.Infrastructure.Persistence;
using SweetFlowerShop.Infrastructure.Persistence.Interceptors;
using SweetFlowerShop.Infrastructure.Persistence.Repositories;
using SweetFlowerShop.Infrastructure.Services;

namespace SweetFlowerShop.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure services. This is the ONLY method Presentation calls.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddIdentityServices()
            .AddJwtAuthentication(configuration)
            .AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Configure it in appsettings.Local.json, .NET User Secrets, " +
                "or the ConnectionStrings__DefaultConnection environment variable.");
        }

        // Register interceptors
        // ORDER MATTERS: SoftDelete → Audit → SlowQuery
        // SoftDelete first: converts Delete→Modified so Audit sees Modified state
        // Audit second: sets timestamps on the now-Modified entity
        // SlowQuery: Singleton (stateless, thread-safe logger)
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddSingleton<SlowQueryInterceptor>();

        services.AddDbContext<FlowerShopDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString)
                .AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<SlowQueryInterceptor>()));

        // Repositories — Scoped because they depend on Scoped DbContext
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Password policy
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User policy
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<FlowerShopDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and validate JwtSettings at startup (fail fast)
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Resolve validated settings from DI
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.FromSeconds(30) // Reduce default 5min skew
            };
        });

        // JwtTokenService — Singleton-safe (stateless, reads from IOptions)
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // HttpContextAccessor — Singleton (thread-safe, uses AsyncLocal internally)
        services.AddHttpContextAccessor();

        // CurrentUserService — Scoped (resolves user per request from HttpContext)
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

