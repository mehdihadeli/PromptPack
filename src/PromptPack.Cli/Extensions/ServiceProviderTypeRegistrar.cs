using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace PromptPack.Extensions;

public sealed class ServiceProviderTypeRegistrar(IServiceProvider serviceProvider) : ITypeRegistrar
{
    private readonly Dictionary<Type, Func<object>> registrations = [];

    public void Register(Type service, Type implementation)
    {
        registrations[service] = () =>
            ActivatorUtilities.CreateInstance(serviceProvider, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        registrations[service] = () => implementation;
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        registrations[service] = factory;
    }

    public ITypeResolver Build() => new ServiceProviderTypeResolver(serviceProvider, registrations);

    private sealed class ServiceProviderTypeResolver(
        IServiceProvider serviceProvider,
        IReadOnlyDictionary<Type, Func<object>> registrations
    ) : ITypeResolver
    {
        public object? Resolve(Type? type)
        {
            if (type is null)
                return null;

            if (registrations.TryGetValue(type, out var factory))
                return factory();

            return serviceProvider.GetService(type);
        }
    }
}
