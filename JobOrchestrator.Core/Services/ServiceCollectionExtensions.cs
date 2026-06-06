using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using JobOrchestrator.Core.Services;
using JobOrchestrator.Core.Data;
using JobOrchestrator.Core.Repositories;
using JobOrchestrator.Core.Mapping;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Extension methods for configuring job scheduler services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core scheduler services and returns an <see cref="ISchedulerBuilder"/> for fluent configuration.
    /// You must call <see cref="UseInMemoryStorage"/> or <see cref="UseSqlServer"/> to configure storage.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddScheduler()
    ///     .UseInMemoryStorage()
    ///     .AddJobHandler&lt;MyHandler&gt;();
    /// </code>
    /// </example>
    public static ISchedulerBuilder AddScheduler(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // Core services
        services.AddSingleton<ICronValidationService, CronValidationService>();
        services.AddSingleton<IJobHandlerRegistry, JobHandlerRegistry>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IMappingService, MappingService>();
        services.AddScoped<IJobService, JobService>();
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddHostedService<SchedulerHostedService>();

        return new SchedulerBuilder(services);
    }

    /// <summary>
    /// Configures the scheduler to use an in-memory database.
    /// All data is lost when the application restarts.
    /// </summary>
    public static ISchedulerBuilder UseInMemoryStorage(this ISchedulerBuilder builder, string dbName = "SchedulerDb")
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(dbName, nameof(dbName));

        var options = new SchedulerOptions
        {
            StorageType = SchedulerStorageType.InMemory,
            InMemoryDatabaseName = dbName
        };

        builder.Services.AddDbContextFactory<SchedulerDbContext>(dbOptions =>
            dbOptions.UseInMemoryDatabase(dbName));
        builder.Services.AddSingleton(options);

        return builder;
    }

    /// <summary>
    /// Configures the scheduler to use SQL Server persistence.
    /// </summary>
    public static ISchedulerBuilder UseSqlServer(this ISchedulerBuilder builder, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));

        var options = new SchedulerOptions
        {
            StorageType = SchedulerStorageType.SqlServer,
            ConnectionString = connectionString
        };

        builder.Services.AddDbContextFactory<SchedulerDbContext>(dbOptions =>
            dbOptions.UseSqlServer(connectionString));
        builder.Services.AddSingleton(options);

        return builder;
    }

    /// <summary>
    /// Applies additional configuration to <see cref="SchedulerOptions"/>.
    /// Call after <see cref="UseInMemoryStorage"/> or <see cref="UseSqlServer"/>.
    /// </summary>
    public static ISchedulerBuilder Configure(this ISchedulerBuilder builder, Action<SchedulerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetService<SchedulerOptions>();
        if (options is not null)
            configure(options);

        return builder;
    }

    /// <summary>
    /// Registers a job handler so the scheduler can dispatch to it by its fully qualified type name.
    /// Handlers are resolved per-execution in their own DI scope, so they may have scoped dependencies.
    /// </summary>
    public static ISchedulerBuilder AddJobHandler<THandler>(this ISchedulerBuilder builder)
        where THandler : class, IJobHandler
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.Services.AddScoped<IJobHandler, THandler>();
        return builder;
    }
}

