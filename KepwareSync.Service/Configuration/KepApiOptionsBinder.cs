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
        private readonly Option<string> _primaryPasswordFileOption;
        private readonly Option<string> _primaryOverwriteFileOption;
        private readonly Option<string[]> _secondaryPasswordFilesOption;
        private readonly Option<string[]> _secondaryOverwriteFilesOption;
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
            var primaryOption = kepApiSection.GetSection("Primary").Get<KepwareSyncTarget>();

            _primaryUserNameOption = new Option<string>("--primary-kep-api-username", "Primary KepApi Username");
            _primaryPasswordOption = new Option<string>("--primary-kep-api-password", "Primary KepApi Password");
            _primaryPasswordFileOption = new Option<string>("--primary-kep-api-password-file", "A path, to a file containing the Primary KepApi Password");
            _primaryHostOption = new Option<string>("--primary-kep-api-host", "Primary KepApi Host URL");
            _primaryOverwriteFileOption = new Option<string>("--primary-overwrite-file", "A path to a YAML File containing overwrite to apply to the configuration loaded from disk. Use this to change properties like IP-Address of a device while using a centrlized configuration.");

            _secondaryHostsOption = new Option<string[]>("--secondary-kep-api", "List of Secondary KepApi configurations in the format username:password@host, if using the --secondary-password-files username@host");
            _secondaryPasswordFilesOption = new Option<string[]>("--secondary-password-files", "List of paths to files containing the Secondary KepApi Passwords");
            _secondaryOverwriteFilesOption = new Option<string[]>("--secondary-overwrite-files", "List of paths to YAML Files containing overwrite to apply to the configuration loaded from disk. Use this to change properties like IP-Address of a device while using a centrlized configuration.");
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
                _primaryPasswordFileOption.SetDefaultValue(primaryOption.PasswordFile);
                _primaryPasswordOption.IsRequired = string.IsNullOrEmpty(primaryOption.UserName) && !string.IsNullOrEmpty(primaryOption.PasswordFile);
                _primaryOverwriteFileOption.SetDefaultValue(primaryOption.OverwriteConfigFile);
                _primaryHostOption.SetDefaultValue(primaryOption.Host);
                _primaryHostOption.IsRequired = string.IsNullOrEmpty(primaryOption.Host);
            }
            else
            {
                _primaryUserNameOption.IsRequired = true;
                _primaryHostOption.IsRequired = true;
            }

            var secondaryOptions = kepApiSection.GetSection("Secondary").Get<List<KepwareSyncTarget>>() ?? [];

            _secondaryHostsOption.SetDefaultValue(secondaryOptions
                     .Select(opt => $"{opt.UserName}:{opt.Password}@{opt.Host}")
                     .ToArray());

            _secondaryPasswordFilesOption.SetDefaultValue(secondaryOptions
                     .Select(opt => opt.PasswordFile)
                     .ToArray());

            _secondaryOverwriteFilesOption.SetDefaultValue(secondaryOptions
                   .Select(opt => opt.OverwriteConfigFile)
                   .ToArray());
        }

        public void BindTo(Command command)
        {
            command.AddOption(_primaryHostOption);
            command.AddOption(_primaryUserNameOption);
            command.AddOption(_primaryPasswordFileOption);
            command.AddOption(_primaryPasswordOption);
            command.AddOption(_primaryOverwriteFileOption);

            command.AddOption(_secondaryHostsOption);
            command.AddOption(_httpDisableCertCheck);
            command.AddOption(_secondaryPasswordFilesOption);
            command.AddOption(_secondaryOverwriteFilesOption);

            command.AddOption(_httpTimeoutOption);
        }


        protected override KepApiOptions GetBoundValue(BindingContext bindingContext)
        {

            var password = bindingContext.ParseResult.GetValueForOption(_primaryPasswordOption);
            var passwordFile = bindingContext.ParseResult.GetValueForOption(_primaryPasswordFileOption);
            if (!string.IsNullOrEmpty(passwordFile))
            {
                password = File.ReadAllText(passwordFile);
            }

            // Primäre Konfiguration
            var primary = new KepwareSyncTarget
            {
                UserName = bindingContext.ParseResult.GetValueForOption(_primaryUserNameOption) ?? string.Empty,
                Password = password ?? string.Empty,
                Host = bindingContext.ParseResult.GetValueForOption(_primaryHostOption) ?? string.Empty,
                OverwriteConfigFile = bindingContext.ParseResult.GetValueForOption(_primaryOverwriteFileOption) ?? string.Empty
            };


            string[] secondaryHosts = bindingContext.ParseResult.GetValueForOption(_secondaryHostsOption) ?? [];
            string[] secondaryPasswordFiles = bindingContext.ParseResult.GetValueForOption(_secondaryPasswordFilesOption) ?? [];
            string[] secondaryOverwriteFiles = bindingContext.ParseResult.GetValueForOption(_secondaryOverwriteFilesOption) ?? [];


            if (secondaryPasswordFiles.Length <= 0)
                secondaryPasswordFiles = [.. Enumerable.Repeat(string.Empty, secondaryHosts.Length)];
            else if (secondaryHosts.Length > secondaryPasswordFiles.Length)
                throw new ArgumentException("Secondary Password Files must be provided for each secondary host");

            if (secondaryOverwriteFiles.Length <= 0)
                secondaryOverwriteFiles = [.. Enumerable.Repeat(string.Empty, secondaryHosts.Length)];
            else if (secondaryHosts.Length > secondaryOverwriteFiles.Length)
                throw new ArgumentException("Secondary Overwrite Files must be provided for each secondary host");

            // Sekundäre Konfigurationen aus Kommaseparierter Liste
            var secondaryList = secondaryHosts.Zip(secondaryPasswordFiles, secondaryOverwriteFiles)
                .Select(ParseSecondaryHost).ToList() ?? new List<KepwareSyncTarget>();

            return new KepApiOptions
            {
                Primary = primary,
                Secondary = secondaryList,
                TimeoutInSeconds = bindingContext.ParseResult.GetValueForOption(_httpTimeoutOption) ?? m_nDefaultTimeout,
                DisableCertificateValidation = bindingContext.ParseResult.GetValueForOption(_httpDisableCertCheck) ?? m_bDefaultDisableCertCheck
            };
        }

        // Parser für sekundäre Konfigurationen im Format username:password@host
        private static KepwareSyncTarget ParseSecondaryHost((string host, string passwordFile, string overwriteFile) options)
        {
            var userPwAndHost = options.host.Split('@', StringSplitOptions.RemoveEmptyEntries);

            if (userPwAndHost.Length != 2)
            {
                throw new FormatException("Invalid secondary KepApi format. Expected: username:password@host");
            }
            string password, username;

            if (string.IsNullOrEmpty(options.passwordFile))
            {
                var parts = userPwAndHost[0].Split(':', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    throw new FormatException("Invalid secondary KepApi format. Expected: username:password@host");
                }

                username = parts[0];
                password = parts[1];
            }
            else
            {
                username = userPwAndHost[0];
                password = System.IO.File.ReadAllText(options.passwordFile);
            }


            return new KepwareSyncTarget
            {
                UserName = username,
                Password = password,
                Host = userPwAndHost[1],
                OverwriteConfigFile = options.overwriteFile
            };
        }
    }
}
