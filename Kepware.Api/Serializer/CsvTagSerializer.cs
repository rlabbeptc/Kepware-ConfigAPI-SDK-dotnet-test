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
using Kepware.Api.Model;
using Kepware.Api.Util;
using Microsoft.Extensions.Logging;

namespace Kepware.Api.Serializer
{
    /// <summary>
    /// Serializes and deserializes tags to and from CSV files
    /// </summary>
    public class CsvTagSerializer
    {
        public static class CsvHeaders
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



        private readonly ILogger<CsvTagSerializer> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CsvTagSerializer"/>
        /// </summary>
        /// <param name="logger"></param>
        public CsvTagSerializer(ILogger<CsvTagSerializer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exports tags to a CSV file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tags"></param>
        /// <param name="dataTypeEnumConverter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ExportTagsAsync(string filePath, List<Tag>? tags, IDataTypeEnumConverter dataTypeEnumConverter, CancellationToken cancellationToken = default)
            => ExportTagsAsync(filePath, tags?
                .Select(tag => CreateTagDictionary(tag, dataTypeEnumConverter)), cancellationToken);

        /// <summary>
        /// Exports tags to a CSV file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tags"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExportTagsAsync(string filePath, IEnumerable<Dictionary<string, object?>>? tags, CancellationToken cancellationToken = default)
        {
            if (tags?.Any() == true)
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
                            csv.WriteField(tag.TryGetValue(header, out object? value) ? value : null);
                        }
                        await csv.NextRecordAsync();
                    }
                }
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File {FilePath} was empty and has been deleted", filePath);
            }
        }

        /// <summary>
        /// Imports tags from a CSV file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataTypeEnumConverter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<List<Tag>> ImportTagsAsync(string filePath, IDataTypeEnumConverter dataTypeEnumConverter, CancellationToken cancellationToken = default)
        {
            var tags = new List<Tag>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            });

            csv.Read();
            csv.ReadHeader();

            while (csv.Read() && !cancellationToken.IsCancellationRequested)
            {

                csv.TryGetField<string>(CsvHeaders.TagName, out var tagName);
                try
                {
                    var tag = new Tag
                    {
                        Name = tagName ?? string.Empty,
                        Description = csv.GetField(CsvHeaders.Description)
                    };

                    tag.SetDynamicProperty(Properties.Tag.Address, csv.GetField(CsvHeaders.Address));
                    var strDataType = csv.GetField(CsvHeaders.DataType);
                    if (strDataType != null)
                        tag.SetDynamicProperty(Properties.Tag.DataType, dataTypeEnumConverter.ConvertFromString(strDataType));
                    tag.SetDynamicProperty(Properties.Tag.ReadWriteAccess, csv.GetField(CsvHeaders.ClientAccess) == "R/W" ? 1 : 0);

                    tag.SetDynamicProperty(Properties.Tag.ScanRateMilliseconds, csv.TryGetField<int>(CsvHeaders.ScanRate, out var scanRate) ? scanRate : 0);
                    tag.SetDynamicProperty(Properties.Tag.ScalingType, csv.TryGetField<int>(CsvHeaders.Scaling, out var scaling) ? scaling : 0);

                    if (scaling != 0)
                    {
                        tag.SetDynamicProperty(Properties.Tag.ScalingRawLow, csv.TryGetField<int>(CsvHeaders.RawLow, out var rawLow) ? rawLow : 0);
                        tag.SetDynamicProperty(Properties.Tag.ScalingRawHigh, csv.TryGetField<int>(CsvHeaders.RawHigh, out var rawHigh) ? rawHigh : 0);
                        tag.SetDynamicProperty(Properties.Tag.ScalingScaledLow, csv.TryGetField<int>(CsvHeaders.ScaledLow, out var scaledLow) ? scaledLow : 0);
                        tag.SetDynamicProperty(Properties.Tag.ScalingScaledHigh, csv.TryGetField<int>(CsvHeaders.ScaledHigh, out var scaledHigh) ? scaledHigh : 0);
                        tag.SetDynamicProperty(Properties.Tag.ScalingScaledDataType, csv.TryGetField<int>(CsvHeaders.ScaledDataType, out var scaledDataType) ? scaledDataType : 0);
                        tag.SetDynamicProperty(Properties.Tag.ScalingClampLow, csv.TryGetField<bool>(CsvHeaders.ClampLow, out var clampLow) && clampLow);
                        tag.SetDynamicProperty(Properties.Tag.ScalingClampHigh, csv.TryGetField<bool>(CsvHeaders.ClampHigh, out var clampHigh) && clampHigh);
                        tag.SetDynamicProperty(Properties.Tag.ScalingUnits, csv.GetField(CsvHeaders.EngUnits));
                        tag.SetDynamicProperty(Properties.Tag.ScalingNegateValue, csv.TryGetField<bool>(CsvHeaders.NegateValue, out var negateValue) && negateValue);
                    }

                    tags.Add(tag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while reading index: {CurrentIndex}, tag {TagName} from CSV {FileName}", csv.CurrentIndex, tagName, filePath);
                }
            }

            return Task.FromResult(tags);
        }
    }
}
