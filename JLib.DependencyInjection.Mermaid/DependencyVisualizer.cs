using System.Text;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JLib.DependencyInjection.Mermaid;

public class DependencyVisualizer
{
    private enum ServiceConstructionKind
    {
        Factory,
        Instance,
        Type,
        Unknown
    }
    private class TypeIdRegistry
    {
        private readonly Dictionary<Type, int> _registry = new();
        private int _counter = 0;
        public int GetId(Type type) => _registry.GetValueOrAdd(type, () => _counter++);
    }
    private class ServiceInfo
    {
        public Type ServiceType { get; init; } = null!;
        public Type? ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; init; }
        public ServiceConstructionKind ConstructionKind { get; init; }
        public Dictionary<Type, ServiceInfo> ReferencedServices { get; } = new();
    }
    private readonly Dictionary<Type, ServiceInfo> _dependencies = new();
    private readonly IServiceCollection _serviceCollection;
    private TypeIdRegistry _typeIdRegistry = new();

    public DependencyVisualizer(IServiceCollection serviceCollection)
    {
        this._serviceCollection = ReformatCollection(serviceCollection);
    }

    IServiceCollection ReformatCollection(IServiceCollection originalCollection)
    {
        var callStack = new List<ServiceInfo>();
        using var exceptions = new ExceptionBuilder(nameof(ReformatCollection));
        return new ServiceCollection().Add(
            originalCollection.Select(descriptor =>
            {
                try
                {
                    Func<IServiceProvider, object> factory = _ => "unsupported factory";

                    if (descriptor.ServiceType.IsGenericTypeDefinition)
                        return descriptor;

                    if (descriptor.ImplementationFactory is not null)
                        factory = descriptor.ImplementationFactory;
                    if (descriptor.ImplementationInstance is not null)
                        factory = _ => descriptor.ImplementationInstance;
                    if (descriptor.ImplementationType is not null)
                        factory = provider => ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);


                    return (ServiceDescriptor?)new(descriptor.ServiceType, provider =>
                    {
                        var serviceInfo = _dependencies.GetValueOrAdd(descriptor.ServiceType, () => new()
                        {
                            ServiceType = descriptor.ServiceType,
                            Lifetime = descriptor.Lifetime,
                            ConstructionKind = descriptor.ImplementationInstance is not null
                                ? ServiceConstructionKind.Instance
                                : descriptor.ImplementationFactory is not null
                                ? ServiceConstructionKind.Factory
                                : descriptor.ImplementationType is not null
                                ? ServiceConstructionKind.Type
                                : ServiceConstructionKind.Unknown
                        });
                        var parent = callStack.LastOrDefault();
                        parent?.ReferencedServices.TryAdd(serviceInfo.ServiceType, serviceInfo);
                        callStack.Add(serviceInfo);
                        var res = factory(provider);
                        serviceInfo.ImplementationType = res.GetType();
                        callStack.RemoveAt(callStack.LastIndexOf(serviceInfo));
                        return res;
                    }, ServiceLifetime.Transient);

                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    return null;
                }
            }).WhereNotNull()
        );
    }

    public string AnalyzeService(params Type[] services)
    {
        var result = new StringBuilder();
        _dependencies.Clear();
        _typeIdRegistry = new();

        using var p = _serviceCollection.BuildServiceProvider();
        using var scope = p.CreateScope();
        var provider = scope.ServiceProvider;
        foreach (var service in services)
            provider.GetService(service);

        PrintHeader();
        PrintClasses();
        PrintRelations();

        return result.ToString();

        void PrintHeader()
        {
            result.AppendLine("---");
            result.Append("title: ").AppendJoin(", ", services.Select(x => x.FullName())).AppendLine(" injection hierarchy");
            result.AppendLine("---");
            result.AppendLine("flowchart LR");

        }

        void PrintClasses()
        {
            foreach (var type in _dependencies.SelectMany(x =>
                         new Type?[] { x.Value.ImplementationType, (Type?)x.Value.ServiceType })
                         .WhereNotNull().ToHashSet())
            {
                result.Append(_typeIdRegistry.GetId(type)).Append("[\"`").AppendLine(type.FullName());
                var serviceInfo = _dependencies.GetValueOrDefault(type);
                if (serviceInfo is not null)
                    result.AppendLine(serviceInfo.Lifetime.ToString());
                result.AppendLine("`\"]");
            }

            result.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        void PrintRelations()
        {
            foreach (var dependency in _dependencies.Select(x => x.Value))
            {
                if (dependency.ImplementationType is not null && dependency.ImplementationType != dependency.ServiceType)
                {
                    result.Append(_typeIdRegistry.GetId(dependency.ServiceType))
                        .Append("==>")
                        .AppendLine(_typeIdRegistry.GetId(dependency.ImplementationType).ToString());
                }

                var from = dependency.ImplementationType ?? dependency.ServiceType;
                foreach (var reference in dependency.ReferencedServices)
                {
                    if (reference.Value.ServiceType == from)
                        continue;
                    result
                        .Append(_typeIdRegistry.GetId(from))
                        .Append(" -- ").Append(reference.Value.ConstructionKind).Append(" --> ")
                        .AppendLine(_typeIdRegistry.GetId(reference.Value.ServiceType).ToString());
                }
            }
        }

    }

}