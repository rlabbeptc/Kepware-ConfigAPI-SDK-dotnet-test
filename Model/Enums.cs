using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.Model
{
    public enum TagDataType
    {
        /// <summary>
        /// Default – Uses the driver's default data type
        /// </summary>
        Default = 0,
        /// <summary>
        /// Boolean – Binary value of true or false
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// Char – 8-bit signed integer data
        /// </summary>
        Char = 2,
        /// <summary>
        /// Byte – 8-bit unsigned integer data
        /// </summary>
        Byte = 3,
        /// <summary>
        /// Short – 16-bit signed integer data
        /// </summary>
        Short = 4,
        /// <summary>
        /// Word – 16-bit unsigned integer data
        /// </summary>
        Word = 5,
        /// <summary>
        /// Long – 32-bit signed integer data
        /// </summary>
        Long = 6,
        /// <summary>
        /// DWord – 32-bit unsigned integer data
        /// </summary>
        DWord = 7,
        /// <summary>
        /// LLong – 64-bit signed integer data
        /// </summary>
        LLong = 8,
        /// <summary>
        /// QWord – 64-bit unsigned integer data
        /// </summary>
        QWord = 9,
        /// <summary>
        /// Float – 32-bit real value of the IEEE-754 standard definition
        /// </summary>
        Float = 10,
        /// <summary>
        /// Double – 64-bit real value of the IEEE-754 standard definition
        /// </summary>
        Double = 11,
        /// <summary>
        /// String – Null-terminated Unicode string
        /// </summary>
        String = 12,
        /// <summary>
        /// BCD – Packed BCD with two bytes, value range is 0-9999
        /// </summary>
        BCD = 13,
        /// <summary>
        /// LBCD – Packed BCD with four bytes, value range is 0-99999999
        /// </summary>
        LBCD = 14,
        /// <summary>
        /// Date – 8-byte floating point number (see Microsoft® Knowledge Base)
        /// </summary>
        Date = 15
    }
}
