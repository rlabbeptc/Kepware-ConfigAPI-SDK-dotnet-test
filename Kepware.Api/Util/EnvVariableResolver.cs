using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kepware.Api.Util
{
    public static partial class EnvVariableResolver
    {
        [GeneratedRegex(@"\$\{([^\}]+)\}", RegexOptions.Compiled)]
        private static partial Regex EnvVarRegex();

        public static string ResolveEnvironmentVariables(string value)
        {
            return EnvVarRegex().Replace(value, match =>
            {
                var envVarName = match.Groups[1].Value;
                var envValue = Environment.GetEnvironmentVariable(envVarName);
                return envValue ?? match.Value; // Falls nicht gesetzt, bleibt der Platzhalter erhalten
            });
        }
    }
}
