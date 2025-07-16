using System.Diagnostics.CodeAnalysis;
using EventSourcingDotNet.KurrentDB;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

// ReSharper disable once InconsistentNaming
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class KurrentDBRegistrationExtensions
{
    public static IServiceCollection ConfigureKurrentDB(this IServiceCollection services, string connectionString)
        => services
            .AddSingleton(Options.Create(KurrentDBClientSettings.Create(connectionString)));

    public static EventSourcingBuilder UseKurrentDB(
        this EventSourcingBuilder builder, 
        string connectionString,
        JsonSerializerSettings? serializerSettings = null)
        => builder.UseKurrentDB(KurrentDBClientSettings.Create(connectionString), serializerSettings);

    public static EventSourcingBuilder UseKurrentDB(
        this EventSourcingBuilder builder,
        KurrentDBClientSettings clientSettings,
        JsonSerializerSettings? serializerSettings = null)
        => builder.UseEventStoreProvider(
            new KurrentDBProvider(
                clientSettings, 
                new EventSerializerSettings(serializerSettings)));
}