using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    public partial class DataTypeEnumConverterProvider
    {
        //Valid for modbus, siemens, ...
        private static Dictionary<int, string> s_defaultDataTypeEnum = new()
        {
                { -1, "Default" },
                { 0, "String" },
                { 1, "Boolean" },
                { 2, "Char" },
                { 3, "Byte" },
                { 4, "Short" },
                { 5, "Word" },
                { 6, "Long" },
                { 7, "DWord" },
                { 8, "Float" },
                { 9, "Double" },
                { 10, "BCD" },
                { 11, "LBCD" },
                { 12, "Date" },
                { 13, "LLong" },
                { 14, "QWord" },
                { 20, "String Array" },
                { 21, "Boolean Array" },
                { 22, "Char Array" },
                { 23, "Byte Array" },
                { 24, "Short Array" },
                { 25, "Word Array" },
                { 26, "Long Array" },
                { 27, "DWord Array" },
                { 28, "Float Array" },
                { 29, "Double Array" },
                { 30, "BCD Array" },
                { 31, "LBCD Array" },
                { 32, "Date Array" },
                { 33, "LLong Array" },
                { 34, "QWord Array" }
            };
    }
}
