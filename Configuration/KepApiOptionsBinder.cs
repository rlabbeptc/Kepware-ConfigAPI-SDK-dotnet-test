using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.Configuration
{
    public class KepApiOptionsBinder : BinderBase<KepApiOptions>
    {
        private readonly Option<string> _userNameOption;
        private readonly Option<string> _passwordOption;
        private readonly Option<string> _hostOption;

        public KepApiOptionsBinder()
        {
            _userNameOption = new Option<string>("--kep-api-username", "KepApi Username");
            _passwordOption = new Option<string>("--kep-api-password", "KepApi Password");
            _hostOption = new Option<string>("--kep-api-host", "KepApi Host URL");
        }
        public void BindTo(Command command)
        {
            command.AddOption(_userNameOption);
            command.AddOption(_passwordOption);
            command.AddOption(_hostOption);
        }

        protected override KepApiOptions GetBoundValue(BindingContext bindingContext)
        {
            return new KepApiOptions
            {
                UserName = bindingContext.ParseResult.GetValueForOption(_userNameOption),
                Password = bindingContext.ParseResult.GetValueForOption(_passwordOption),
                Host = bindingContext.ParseResult.GetValueForOption(_hostOption)
            };
        }
    }
}
