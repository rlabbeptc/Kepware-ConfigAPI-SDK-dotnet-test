using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public class KepApiOptionsBinder : BinderBase<KepApiOptions>
    {
        private readonly Option<string> _primaryUserNameOption;
        private readonly Option<string> _primaryPasswordOption;
        private readonly Option<string> _primaryHostOption;
        private readonly Option<int?> _httpTimeoutOption;
        private readonly Option<bool?> _httpDisableCertCheck;

        private readonly Option<string[]> _secondaryHostsOption;

        private readonly int m_nDefaultTimeout;
        private readonly bool m_bDefaultDisableCertCheck;

        public KepApiOptionsBinder(IConfiguration configuration)
        {
            var kepApiSection = configuration.GetSection("Kepware");
            // Defaultwerte aus der appsettings.json lesen
            var primaryOption = kepApiSection.GetSection("Primary").Get<KepApiHost>();

            _primaryUserNameOption = new Option<string>("--primary-kep-api-username", "Primary KepApi Username");
            _primaryPasswordOption = new Option<string>("--primary-kep-api-password", "Primary KepApi Password");
            _primaryHostOption = new Option<string>("--primary-kep-api-host", "Primary KepApi Host URL");
            _secondaryHostsOption = new Option<string[]>("--secondary-kep-api", "List of Secondary KepApi configurations in the format username:password@host");
            _httpTimeoutOption = new Option<int?>("--http-timeout", "HTTP Timeout in seconds");
            _httpDisableCertCheck = new Option<bool?>("--http-disable-cert-check", "Disable Certificate Validation");

            // Defaultwerte setzen
            m_nDefaultTimeout = kepApiSection.GetValue<int?>("TimeoutInSeconds") ?? 60;
            m_bDefaultDisableCertCheck = kepApiSection.GetValue<bool>("DisableCertificateValidation");
            _httpTimeoutOption.SetDefaultValue(m_nDefaultTimeout);
            _httpDisableCertCheck.SetDefaultValue(m_bDefaultDisableCertCheck);

            if (primaryOption != null)
            {
                _primaryPasswordOption.SetDefaultValue(primaryOption.Password);
                _primaryUserNameOption.SetDefaultValue(primaryOption.UserName);
                _primaryPasswordOption.IsRequired = string.IsNullOrEmpty(primaryOption.UserName);
                _primaryHostOption.SetDefaultValue(primaryOption.Host);
                _primaryHostOption.IsRequired = string.IsNullOrEmpty(primaryOption.Host);
            }
            else
            {
                _primaryPasswordOption.IsRequired = true;
                _primaryUserNameOption.IsRequired = true;
                _primaryHostOption.IsRequired = true;
            }

            var secondaryOptions = kepApiSection.GetSection("Secondary").Get<List<KepApiHost>>() ?? [];

            _secondaryHostsOption.SetDefaultValue(secondaryOptions
                     .Select(opt => $"{opt.UserName}:{opt.Password}@{opt.Host}")
                     .ToArray());
        }

        public void BindTo(Command command)
        {
            command.AddOption(_primaryUserNameOption);
            command.AddOption(_primaryPasswordOption);
            command.AddOption(_primaryHostOption);
            command.AddOption(_httpTimeoutOption);
            command.AddOption(_secondaryHostsOption);
            command.AddOption(_httpDisableCertCheck);
        }


        protected override KepApiOptions GetBoundValue(BindingContext bindingContext)
        {
            // Primäre Konfiguration
            var primary = new KepApiHost
            {
                UserName = bindingContext.ParseResult.GetValueForOption(_primaryUserNameOption) ?? string.Empty,
                Password = bindingContext.ParseResult.GetValueForOption(_primaryPasswordOption) ?? string.Empty,
                Host = bindingContext.ParseResult.GetValueForOption(_primaryHostOption) ?? string.Empty,
            };

            // Sekundäre Konfigurationen aus Kommaseparierter Liste
            var secondaryList = bindingContext.ParseResult.GetValueForOption(_secondaryHostsOption)
                ?.Select(ParseSecondaryHost)
                .ToList() ?? new List<KepApiHost>();

            return new KepApiOptions
            {
                Primary = primary,
                Secondary = secondaryList,
                TimeoutInSeconds = bindingContext.ParseResult.GetValueForOption(_httpTimeoutOption) ?? m_nDefaultTimeout,
                DisableCertificateValidation = bindingContext.ParseResult.GetValueForOption(_httpDisableCertCheck) ?? m_bDefaultDisableCertCheck
            };
        }

        // Parser für sekundäre Konfigurationen im Format username:password@host
        private static KepApiHost ParseSecondaryHost(string input)
        {
            var userPwAndHost = input.Split('@', StringSplitOptions.RemoveEmptyEntries);

            if (userPwAndHost.Length != 2)
            {
                throw new FormatException("Invalid secondary KepApi format. Expected: username:password@host");
            }

            var parts = userPwAndHost[0].Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new FormatException("Invalid secondary KepApi format. Expected: username:password@host");
            }

            return new KepApiHost
            {
                UserName = parts[0],
                Password = parts[1],
                Host = userPwAndHost[1]
            };
        }
    }
}
