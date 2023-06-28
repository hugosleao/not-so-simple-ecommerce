﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SimpleEcommerceV2.Order.Middlewares;
using SimpleEcommerceV2.MessageHandler.Models;
using SimpleEcommerceV2.Order.Modules;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(applicationBuilder =>
{
    applicationBuilder.RegisterModule<DomainModule>();
    applicationBuilder.RegisterModule<SqsModule>();
    applicationBuilder.RegisterModule(new InfrastructureModule(builder.Configuration));

    applicationBuilder
        .RegisterType<GlobalErrorHandlerMiddleware>()
        .SingleInstance();
});

builder.Services.Configure<AwsSqsMessageParams>(
    "AwsSqsMessageSenderParams01",
     builder.Configuration.GetSection("Order:AwsSqsMessageSenderParams01")
);
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.Map("/order", applicationBuilder =>
{
    applicationBuilder.UseSwagger();
    applicationBuilder.UseSwaggerUI();
    applicationBuilder.UseRouting();
    applicationBuilder.UseMiddleware<GlobalErrorHandlerMiddleware>();

    applicationBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health",
            new HealthCheckOptions
            {
                Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
    });
});

app.Run();
