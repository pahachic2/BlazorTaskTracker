using System.Reflection;

namespace TaskTracker.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapEndpointGroups(this WebApplication app)
    {
        // Автоматически находим и регистрируем все классы, реализующие IEndpointGroup
        var endpointGroupTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IEndpointGroup)))
            .ToList();

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is IEndpointGroup instance)
            {
                instance.MapEndpoints(app);
            }
        }

        return app;
    }
} 