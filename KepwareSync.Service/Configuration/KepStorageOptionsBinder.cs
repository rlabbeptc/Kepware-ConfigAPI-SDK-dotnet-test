using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Kepware.SyncService.Configuration
{
    public class KepStorageOptionsBinder : BinderBase<KepStorageOptions>
    {
        private readonly Option<string> _directoryOption;
        private readonly Option<bool> _persistDefaultValueOption;

        public KepStorageOptionsBinder(IConfiguration configuration)
        {
            _directoryOption = new Option<string>("--directory", "Storage Directory");
            _persistDefaultValueOption = new Option<bool>("--persist-default-value", "Persist Default Values");

            var settingValue = configuration.GetSection("Storage").Get<KepStorageOptions>();

            if (settingValue != null)
            {
                _directoryOption.SetDefaultValue(settingValue.Directory);
                _persistDefaultValueOption.SetDefaultValue(settingValue.PersistDefaultValue);
                _directoryOption.IsRequired = string.IsNullOrEmpty(settingValue.Directory);
            }
            else
            {
                _directoryOption.IsRequired = true;
            }

        }

        public void BindTo(Command command)
        {
            command.AddOption(_directoryOption);
            command.AddOption(_persistDefaultValueOption);
        }

        protected override KepStorageOptions GetBoundValue(BindingContext bindingContext) =>
            new KepStorageOptions
            {
                Directory = bindingContext.ParseResult.GetValueForOption(_directoryOption),
                PersistDefaultValue = bindingContext.ParseResult.GetValueForOption(_persistDefaultValueOption)
            };

    }
}
