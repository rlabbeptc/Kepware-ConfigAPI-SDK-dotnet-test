using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    public interface IDataTypeEnumConverter
    {
        int ConvertFromString(string value);
        string ConvertToString(int value);
    }
    public partial class DataTypeEnumConverterProvider
    {
        private readonly IReadOnlyDictionary<string, DataTypeConverter> _converters;
        private readonly IDataTypeEnumConverter _defaultConverter = new DataTypeConverter(s_defaultDataTypeEnum);


        public DataTypeEnumConverterProvider()
        {
            _converters = new Dictionary<string, DataTypeConverter>(StringComparer.OrdinalIgnoreCase)
            {
                //{ "modbus",new( s_defaultDataTypeEnum) }
            };
        }


        public IDataTypeEnumConverter GetDataTypeEnumConverter(string? drivename)
        {
            if (drivename != null && _converters.TryGetValue(drivename, out DataTypeConverter? converter))
                return converter;

            return _defaultConverter;
        }

        private class NoopTypeConverter : IDataTypeEnumConverter
        {
            public int ConvertFromString(string value)
            {
                if (int.TryParse(value, out int result))
                {
                    return result;
                }
                return 0;
            }

            public string ConvertToString(int value) => value.ToString();
        }

        private class DataTypeConverter : IDataTypeEnumConverter
        {
            private readonly IReadOnlyDictionary<int, string> _lookupString;
            private readonly IReadOnlyDictionary<string, int> _lookupInt;

            public DataTypeConverter(Dictionary<int, string> map)
            {
                _lookupString = map;
                _lookupInt = map.ToDictionary(key => key.Value, val => val.Key, StringComparer.OrdinalIgnoreCase);
            }

            public int ConvertFromString(string value)
                => _lookupInt[value];

            public string ConvertToString(int value)
                => _lookupString[value];
        }
    }
}
