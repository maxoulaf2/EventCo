using System.Reflection;
using EventCo.Application.Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddCommandHandlersFromAssembly(assembly);

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<DomainEventCollector>();
        services.AddDomainEventHandlersFromAssembly(assembly);

        return services;
    }

    private static void AddCommandHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes =
            from type in assembly.GetTypes()
            where !type.IsAbstract && !type.IsInterface
            from handlerInterface in type.GetInterfaces()
            where handlerInterface.IsGenericType
                  && (handlerInterface.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                      || handlerInterface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
            select (Interface: handlerInterface, Implementation: type);

        foreach (var (handlerInterface, implementation) in handlerTypes)
            services.AddScoped(handlerInterface, implementation);
    }

    private static void AddDomainEventHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes =
            from type in assembly.GetTypes()
            where !type.IsAbstract && !type.IsInterface
            from handlerInterface in type.GetInterfaces()
            where handlerInterface.IsGenericType
                  && handlerInterface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
            select (Interface: handlerInterface, Implementation: type);

        foreach (var (handlerInterface, implementation) in handlerTypes)
            services.AddScoped(handlerInterface, implementation);
    }
}
