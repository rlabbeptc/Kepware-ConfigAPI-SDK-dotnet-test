using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using Xunit;

namespace Kepware.Api.Test.ApiClient
{
    public class KepwareApiServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKepwareApiClients_ShouldRegisterClients()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new List<KeyValuePair<string, KepwareApiClientOptions>>
            {
                new KeyValuePair<string, KepwareApiClientOptions>("Client1", new KepwareApiClientOptions { HostUri = new Uri("http://localhost") }),
                new KeyValuePair<string, KepwareApiClientOptions>("Client2", new KepwareApiClientOptions { HostUri = new Uri("http://localhost") })
            };

            // Act
            services.AddKepwareApiClients(options);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var clients = serviceProvider.GetService<List<KepwareApiClient>>() ?? [];
            clients.ShouldNotBeNull();
            clients.ShouldContain(c => c.ClientName == "Client1");
            clients.ShouldContain(c => c.ClientName == "Client2");
        }

        [Fact]
        public void AddKepwareApiClient_ShouldRegisterClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new KepwareApiClientOptions { HostUri = new Uri("http://localhost") };

            // Act
            services.AddKepwareApiClient("Client1", options);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var client = serviceProvider.GetService<KepwareApiClient>();
            client.ShouldNotBeNull();
        }

        [Fact]
        public void AddKepwareApiClient_WithInvalidName_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new KepwareApiClientOptions { HostUri = new Uri("http://localhost") };

            // Act & Assert
            Should.Throw<ArgumentException>(() => services.AddKepwareApiClient("", options));
        }

        [Fact]
        public void AddKepwareApiClient_WithNullOptions_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => services.AddKepwareApiClient("Client1", null));
        }

        [Fact]
        public void AddKepwareApiClient_WithInvalidUri_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new KepwareApiClientOptions { HostUri = new Uri("/relative", UriKind.Relative) };

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => services.AddKepwareApiClient("Client1", options));
        }
    }
}
