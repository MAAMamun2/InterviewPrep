using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Discounts;
using InterviewPrep.Infrastructure.Auth;
using InterviewPrep.Infrastructure.BackgroundJobs;
using InterviewPrep.Infrastructure.Caching;
using InterviewPrep.Infrastructure.Persistence;
using InterviewPrep.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPrep.Infrastructure;

/// <summary>
/// Composition root for this layer. Everything the Application layer declared as an interface
/// gets its concrete implementation wired up here, in ONE place -- so Program.cs just calls
/// `builder.Services.AddInfrastructure(builder.Configuration)` and stays tiny. Also where the
/// Strategy pattern's "many implementations of one interface" and the Decorator pattern's
/// "wrap an existing registration" both get wired up, each a one-liner thanks to Scrutor.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InterviewPrepDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=interviewprep.db"));

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        // STRATEGY PATTERN registration: scan the Domain assembly for every IDiscountRule
        // implementation and register all of them. Add a new rule class -> it's picked up
        // automatically, no code here changes. This is the Open/Closed principle living in DI config.
        services.Scan(scan => scan.FromAssemblyOf<IDiscountRule>()
            .AddClasses(classes => classes.AssignableTo<IDiscountRule>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
        services.AddSingleton<DiscountCalculator>();

        // DECORATOR PATTERN registration: MemberRepository is registered first (the "real" one),
        // then .Decorate<>() wraps every existing IMemberRepository registration in CachedMemberRepository.
        // Anything that injects IMemberRepository transparently gets the cached, decorated version.
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.Decorate<IMemberRepository, CachedMemberRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHostedService<TierAuditJob>();

        return services;
    }
}
