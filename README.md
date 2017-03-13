# MassTransit.ServiceFabric
Service Fabric bits and bobs for MassTransit

# MassTransitListener

The MassTransit listener coordinates starting and stopping the bus when the service starts, stops or changes role.

## Usage

You can use the MassTransit listener with a stateful or stateless service.

## Stateless

To plumb in the listener in to a stateless service we will have to override the `CreateServiceInstanceListeners` method on the `StatelessService`, we then need to create a `ServiceInstanceListener` and pass in a delegate that will create our `MassTransitListener`.

```csharp
public class MyStatelessService : StatelessService
{
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
        return new []
        {
            new ServiceInstanceListener(_ => new MassTransitListener(CreateBus()))
        };
    }

    private static IBusControl CreateBus()
    {
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(new Uri("rabbitmq://localhost/"), h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        });

        return busControl;
    }
}
```

## Stateful

To plumb in the listener in to a stateful service we will have to override the `CreateServiceReplicaListeners` method on the `StatefulService`, we then need to create a `ServiceReplicaListener` and pass in a delegate that will create our `MassTransitListener`.

```csharp
public class MyStatefulService : StatefulService
{
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return new[]
        {
            new ServiceReplicaListener(_ => new MassTransitListener(CreateBus()))
        };
    }

    private static IBusControl CreateBus()
    {
        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(new Uri("rabbitmq://localhost/"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            return busControl;
        }
    }
```

## Contributing

1. Fork
1. Hack!
1. Pull Request