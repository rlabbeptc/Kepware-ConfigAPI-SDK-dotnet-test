using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using KepwareSync.Model;

namespace KepwareSync
{

    public class CsvTagSerializer
    {
        private static class CsvHeaders
        {
            public const string TagName = "Tag Name";
            public const string Address = "Address";
            public const string DataType = "Data Type";
            public const string RespectDataType = "Respect Data Type";
            public const string ClientAccess = "Client Access";
            public const string ScanRate = "Scan Rate";
            public const string Scaling = "Scaling";
            public const string RawLow = "Raw Low";
            public const string RawHigh = "Raw High";
            public const string ScaledLow = "Scaled Low";
            public const string ScaledHigh = "Scaled High";
            public const string ScaledDataType = "Scaled Data Type";
            public const string ClampLow = "Clamp Low";
            public const string ClampHigh = "Clamp High";
            public const string EngUnits = "Eng Units";
            public const string Description = "Description";
            public const string NegateValue = "Negate Value";
        }

        private readonly string[] _headers =
        {
                CsvHeaders.TagName,
                CsvHeaders.Address,
                CsvHeaders.DataType,
                CsvHeaders.RespectDataType,
                CsvHeaders.ClientAccess,
                CsvHeaders.ScanRate,
                CsvHeaders.Scaling,
                CsvHeaders.RawLow,
                CsvHeaders.RawHigh,
                CsvHeaders.ScaledLow,
                CsvHeaders.ScaledHigh,
                CsvHeaders.ScaledDataType,
                CsvHeaders.ClampLow,
                CsvHeaders.ClampHigh,
                CsvHeaders.EngUnits,
                CsvHeaders.Description,
                CsvHeaders.NegateValue
            };

        private readonly HashSet<string> quotedField = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                CsvHeaders.TagName, CsvHeaders.Address, CsvHeaders.Description
            };

        private static Dictionary<string, object?> CreateTagDictionary(Tag tag, IDataTypeEnumConverter dataTypeEnumConverter)
        {
            bool scaling = tag.GetDynamicProperty<int>(Properties.Tag.ScalingType) != 0;
            return new Dictionary<string, object?>
                {
                    { CsvHeaders.TagName, tag.Name },
                    { CsvHeaders.Address, tag.GetDynamicProperty<string>(Properties.Tag.Address) },
                    { CsvHeaders.DataType, dataTypeEnumConverter.ConvertToString(tag.GetDynamicProperty<int>(Properties.Tag.DataType)) },
                    { CsvHeaders.RespectDataType, "1" }, // Assuming this aligns
                    { CsvHeaders.ClientAccess, tag.GetDynamicProperty<int>(Properties.Tag.ReadWriteAccess) == 1 ? "R/W" : "RO" },
                    { CsvHeaders.ScanRate, tag.GetDynamicProperty<int>(Properties.Tag.ScanRateMilliseconds) },
                    { CsvHeaders.Scaling, tag.GetDynamicProperty<int>(Properties.Tag.ScalingType) },
                    { CsvHeaders.RawLow, scaling ? tag.GetDynamicProperty<int>(Properties.Tag.ScalingRawLow) : null },
                    { CsvHeaders.RawHigh, scaling ? tag.GetDynamicProperty<int>(Properties.Tag.ScalingRawHigh) : null },
                    { CsvHeaders.ScaledLow, scaling ? tag.GetDynamicProperty<int>(Properties.Tag.ScalingScaledLow) : null },
                    { CsvHeaders.ScaledHigh, scaling ? tag.GetDynamicProperty<int>(Properties.Tag.ScalingScaledHigh) : null },
                    { CsvHeaders.ScaledDataType, scaling ? tag.GetDynamicProperty<int>(Properties.Tag.ScalingScaledDataType) : null },
                    { CsvHeaders.ClampLow, scaling ? tag.GetDynamicProperty<bool>(Properties.Tag.ScalingClampLow) : null },
                    { CsvHeaders.ClampHigh, scaling ? tag.GetDynamicProperty<bool>(Properties.Tag.ScalingClampHigh) : null },
                    { CsvHeaders.EngUnits, tag.GetDynamicProperty<string>(Properties.Tag.ScalingUnits) },
                    { CsvHeaders.Description, tag.Description },
                    { CsvHeaders.NegateValue, scaling ? tag.GetDynamicProperty<bool>(Properties.Tag.ScalingNegateValue) : null }
                };
        }

        private static Tag CreateTagFromDictionary(Dictionary<string, object?> tagDict, IDataTypeEnumConverter dataTypeEnumConverter)
        {
            var tag = new Tag
            {
                Name = tagDict[CsvHeaders.TagName] as string ?? string.Empty,
                Description = tagDict[CsvHeaders.Description] as string ?? string.Empty
            };
            tag.SetDynamicProperty(Properties.Tag.Address, tagDict.GetValue<string>(CsvHeaders.Address));
            tag.SetDynamicProperty(Properties.Tag.DataType, dataTypeEnumConverter.ConvertFromString(tagDict.GetValue<string>(CsvHeaders.DataType)));
            tag.SetDynamicProperty(Properties.Tag.ReadWriteAccess, tagDict.GetValue<string>(CsvHeaders.ClientAccess) == "R/W" ? 1 : 0);
            tag.SetDynamicProperty(Properties.Tag.ScanRateMilliseconds, tagDict.GetValue<int>(CsvHeaders.ScanRate));
            tag.SetDynamicProperty(Properties.Tag.ScalingType, tagDict.GetValue<int>(CsvHeaders.Scaling));
            tag.SetDynamicProperty(Properties.Tag.ScalingRawLow, tagDict.GetValue<int>(CsvHeaders.RawLow));
            tag.SetDynamicProperty(Properties.Tag.ScalingRawHigh, tagDict.GetValue<int>(CsvHeaders.RawHigh));
            tag.SetDynamicProperty(Properties.Tag.ScalingScaledLow, tagDict.GetValue<int>(CsvHeaders.ScaledLow));
            tag.SetDynamicProperty(Properties.Tag.ScalingScaledHigh, tagDict.GetValue<int>(CsvHeaders.ScaledHigh));
            tag.SetDynamicProperty(Properties.Tag.ScalingScaledDataType, tagDict.GetValue<int>(CsvHeaders.ScaledDataType));
            tag.SetDynamicProperty(Properties.Tag.ScalingClampLow, tagDict.GetValue<bool>(CsvHeaders.ClampLow));
            tag.SetDynamicProperty(Properties.Tag.ScalingClampHigh, tagDict.GetValue<bool>(CsvHeaders.ClampHigh));
            tag.SetDynamicProperty(Properties.Tag.ScalingUnits, tagDict.GetValue<string>(CsvHeaders.EngUnits));
            tag.SetDynamicProperty(Properties.Tag.ScalingNegateValue, tagDict.GetValue<bool>(CsvHeaders.NegateValue));
            return tag;
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CsvHelper.Expressions.RecordManager))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CsvHelper.Expressions.RecordCreatorFactory))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CsvHelper.Expressions.RecordHydrator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CsvHelper.Expressions.ExpressionManager))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CsvHelper.TypeConversion.StringConverter))]
        public CsvTagSerializer()
        {

        }

        public Task ExportTagsAsync(string filePath, List<Tag> tags, IDataTypeEnumConverter dataTypeEnumConverter)
            => ExportTagsAsync(filePath, tags
                .Where(tag => !tag.IsAutogenerated)
                .Select(tag => CreateTagDictionary(tag, dataTypeEnumConverter)));

        public async Task ExportTagsAsync(string filePath, IEnumerable<Dictionary<string, object?>> tags)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                ShouldQuote = args => (args.Field != null && quotedField.Contains(args.Field)) || ConfigurationFunctions.ShouldQuote(args)
            });

            // Write header
            if (tags.Any())
            {
                foreach (var header in _headers)
                {
                    csv.WriteField(header);
                }
                await csv.NextRecordAsync();

                // Write rows
                foreach (var tag in tags)
                {
                    foreach (var header in _headers)
                    {
                        csv.WriteField(tag.ContainsKey(header) ? tag[header] : null);
                    }
                    await csv.NextRecordAsync();
                }
            }
        }

        public Task<List<Tag>> ImportTagsAsync(string filePath, IDataTypeEnumConverter dataTypeEnumConverter)
        {
            var tags = new List<Tag>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            });

            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                var dict = ((IDictionary<string, object?>)record).ToDictionary(k => k.Key, v => v.Value);
                tags.Add(CreateTagFromDictionary(dict, dataTypeEnumConverter));
            }

            return Task.FromResult(tags);
        }
    }
}
