﻿using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Services.Customers.Customers;

internal static class MassTransitExtensions
{
    internal static void AddCustomerPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<CustomerCreated>(e =>
            e.SetEntityName($"{nameof(CustomerCreated).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<CustomerCreated>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<CustomerCreated>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
