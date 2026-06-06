using JobOrchestrator.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Resolves the correct <see cref="IJobHandler"/> for a given full type name.
/// Handlers are resolved via DI so they can have scoped/transient dependencies.
/// </summary>
public interface IJobHandlerRegistry
{
    /// <summary>Returns the handler registered for <paramref name="handlerTypeName"/> (full type name), or null if none.</summary>
    IJobHandler? Resolve(string handlerTypeName);

    /// <summary>Returns the raw JobType strings for all registered handlers.</summary>
    IReadOnlyList<string> GetRegisteredJobTypes();

    /// <summary>Returns all registered handlers with their metadata (type name and friendly name) for UI dropdowns.</summary>
    IReadOnlyList<(string TypeName, string Name)> GetRegisteredHandlers();
}

/// <summary>
/// Implementation of IJobHandlerRegistry.
/// This is an internal implementation detail - use IJobHandlerRegistry interface for dependency injection.
/// </summary>
internal class JobHandlerRegistry : IJobHandlerRegistry
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance of the job handler registry.
    /// </summary>
    public JobHandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Resolves a job handler for the given full type name.
    /// </summary>
    public IJobHandler? Resolve(string handlerTypeName)
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider
            .GetServices<IJobHandler>()
            .FirstOrDefault(h => string.Equals(h.GetType().FullName, handlerTypeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the list of all registered job types (handler full type names).
    /// </summary>
    public IReadOnlyList<string> GetRegisteredJobTypes()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider
            .GetServices<IJobHandler>()
            .Select(h => h.GetType().FullName ?? string.Empty)
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    /// <summary>
    /// Gets all registered handlers with their metadata for UI dropdowns.
    /// </summary>
    public IReadOnlyList<(string TypeName, string Name)> GetRegisteredHandlers()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider
            .GetServices<IJobHandler>()
            .Select(h => (h.GetType().FullName ?? string.Empty, h.Name))
            .Where(t => !string.IsNullOrEmpty(t.Item1))
            .ToList();
    }
}
