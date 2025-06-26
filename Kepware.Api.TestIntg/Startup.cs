using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kepware.Api.TestIntg
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Check if the test integration flag is set
            var _testIntegration = bool.TryParse(configuration["TestSettings:IntegrationTest"], out var testIntegration) && testIntegration;

            var TEST_ENDPOINT = $"{configuration["TestSettings:TestServer:Host"]}:{configuration["TestSettings:TestServer:Port"]}" ?? "http://localhost:57412";

            services
                .AddKepwareApiClient(
                    name: "TestClient",
                    baseUrl: TEST_ENDPOINT,
                    apiUserName: $"{configuration["TestSettings:TestServer:UserName"]}",
                    apiPassword: $"{configuration["TestSettings:TestServer:Password"]}",
                    disableCertificateValidation: true
                    )
                .AddTransient(s => { return s.GetRequiredService<KepwareApiClient>(); });
        }
    }
}
