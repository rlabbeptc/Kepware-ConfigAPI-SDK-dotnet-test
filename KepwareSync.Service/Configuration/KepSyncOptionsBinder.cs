using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public class KepSyncOptionsBinder : BinderBase<KepSyncOptions>
    {
        private readonly Option<SyncDirection> _syncDirection;
        private readonly Option<SyncMode> _syncMode;
        private readonly Option<int> _syncThortteling;

        public KepSyncOptionsBinder(IConfiguration configuration)
        {
            _syncDirection = new Option<SyncDirection>("--kep-sync-direction", "The primary sync direction (kepware -> disk oder disk -> kepware)");
            _syncMode = new Option<SyncMode>("--kep-sync-mode", "The sync mode (one- or twoway)");
            _syncThortteling = new Option<int>("--kep-sync-throtteling", "The throtteling time in milliseconds after a event has been detected before a sync starts");

            var settingValue = configuration.GetSection("Sync").Get<KepSyncOptions>();

            if (settingValue != null)
            {
                _syncDirection.SetDefaultValue(settingValue.SyncDirection);
                _syncMode.SetDefaultValue(settingValue.SyncMode);
                _syncThortteling.SetDefaultValue(settingValue.SyncThrottlingMs);
            }
        }
        public void BindTo(Command command)
        {
            command.AddOption(_syncDirection);
            command.AddOption(_syncMode);
            command.AddOption(_syncThortteling);
        }

        protected override KepSyncOptions GetBoundValue(BindingContext bindingContext)
        {
            return new KepSyncOptions
            {
                SyncDirection = bindingContext.ParseResult.GetValueForOption(_syncDirection),
                SyncMode = bindingContext.ParseResult.GetValueForOption(_syncMode),
                SyncThrottlingMs = bindingContext.ParseResult.GetValueForOption(_syncThortteling)
            };
        }
    }
}
