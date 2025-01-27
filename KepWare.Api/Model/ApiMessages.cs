using Kepware.Api.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    /// <summary>
    /// Represents the result of an API call.
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Gets or sets the status code of the API result.
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the message of the API result.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets the HTTP status code corresponding to the API result code.
        /// </summary>
        [JsonIgnore]
        public HttpStatusCode HttpStatusCode => (HttpStatusCode)Code;

        /// <summary>
        /// Gets a value indicating whether the status code is in the range 200-299.
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessStatusCode => Code >= 200 && Code < 300;
    }

    /// <summary>
    /// Represents the status of the Configuration API REST service.
    /// </summary>
    public class ApiStatus
    {
        /// <summary>
        /// Gets or sets the name of the server being checked.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the service is running.
        /// </summary>
        public bool Healthy { get; set; } = false;
    }

    /// <summary>
    /// Represents the type of product.
    /// </summary>
    public enum ProductType
    {
        Unknown = 0,
        ThingWorxKepwareEdge = 13,
        KEPServerEX = 12
    }

    /// <summary>
    /// Represents information about a product.
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        [JsonPropertyName("product_name")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product version.
        /// </summary>
        [JsonPropertyName("product_version")]
        public string ProductVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the major version of the product.
        /// </summary>
        [JsonPropertyName("product_version_major")]
        public int ProductVersionMajor { get; set; }

        /// <summary>
        /// Gets or sets the minor version of the product.
        /// </summary>
        [JsonPropertyName("product_version_minor")]
        public int ProductVersionMinor { get; set; }

        /// <summary>
        /// Gets or sets the build version of the product.
        /// </summary>
        [JsonPropertyName("product_version_build")]
        public int ProductVersionBuild { get; set; }

        /// <summary>
        /// Gets or sets the patch version of the product.
        /// </summary>
        [JsonPropertyName("product_version_patch")]
        public int ProductVersionPatch { get; set; }

        /// <summary>
        /// Gets the type of the product.
        /// </summary>
        public ProductType ProductType =>
            int.TryParse(ProductId, out var id) ? (ProductType)id : Enum.TryParse<ProductType>(ProductName, out var prodType) ? prodType : ProductType.Unknown;

        /// <summary>
        /// Gets a value indicating whether the product supports JSON project load service.
        /// </summary>
        [JsonIgnore]
        public bool SupportsJsonProjectLoadService =>
            (ProductType == ProductType.KEPServerEX && (ProductVersionMajor > 6 || (ProductVersionMajor == 6 && ProductVersionMinor >= 17))) ||
            (ProductType == ProductType.ThingWorxKepwareEdge && (ProductVersionMajor > 1 || (ProductVersionMajor == 1 && ProductVersionMinor >= 10)));
    }

    public static class Docs
    {
        [Endpoint("/config/v1/doc/drivers/")]
        public class Driver
        {
            [JsonPropertyName("namespace")]
            public string? Namespace { get; set; }

            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }

            [JsonPropertyName("doc_channels")]
            public string? DocChannels { get; set; }

            [JsonPropertyName("doc_devices")]
            public string? DocDevices { get; set; }

            [JsonPropertyName("doc_meter_groups")]
            public string? DocMeterGroups { get; set; }

            [JsonPropertyName("doc_meters")]
            public string? DocMeters { get; set; }
        }

        public abstract class CollectionDefinition
        {
            public static readonly CollectionDefinition Empty = new EmptyCollectionDefinition();

            [JsonPropertyName("type_definition")]
            public TypeDefinition? TypeDefinition { get; set; }
            [JsonPropertyName("property_definitions")]
            public List<PropertyDefinition>? PropertyDefinitions { get; set; }

            private sealed class EmptyCollectionDefinition : CollectionDefinition
            {
                public EmptyCollectionDefinition()
                {
                    TypeDefinition = new TypeDefinition
                    {
                        Name = string.Empty,
                        Collection = string.Empty,
                        Namespace = string.Empty,
                        CanCreate = false,
                        CanDelete = false,
                        CanModify = false,
                        AutoGenerated = false,
                        RequiresDriver = false,
                        AccessControlled = false,
                        ChildCollections = new List<string>()
                    };
                    PropertyDefinitions = new List<PropertyDefinition>();
                }
            }
        }

        [Endpoint("/config/v1/doc/drivers/{driverName}/channels/")]
        public class Channel : CollectionDefinition { }

        [Endpoint("/config/v1/doc/drivers/{driverName}/devices/")]
        public class Device : CollectionDefinition { }

        public class TypeDefinition
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("collection")]
            public string? Collection { get; set; }

            [JsonPropertyName("namespace")]
            public string? Namespace { get; set; }

            [JsonPropertyName("can_create")]
            public bool CanCreate { get; set; }

            [JsonPropertyName("can_delete")]
            public bool CanDelete { get; set; }

            [JsonPropertyName("can_modify")]
            public bool CanModify { get; set; }

            [JsonPropertyName("auto_generated")]
            public bool AutoGenerated { get; set; }

            [JsonPropertyName("requires_driver")]
            public bool RequiresDriver { get; set; }

            [JsonPropertyName("access_controlled")]
            public bool AccessControlled { get; set; }

            [JsonPropertyName("child_collections")]
            public List<string>? ChildCollections { get; set; }
        }

        public class PropertyDefinition
        {
            [JsonPropertyName("symbolic_name")]
            public string? SymbolicName { get; set; }

            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }

            [JsonPropertyName("display_description")]
            public string? DisplayDescription { get; set; }

            [JsonPropertyName("group_name")]
            public string? GroupName { get; set; }

            [JsonPropertyName("section_name")]
            public string? SectionName { get; set; }

            [JsonPropertyName("read_only")]
            public bool ReadOnly { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("default_value")]
            public object? DefaultValue { get; set; }

            [JsonPropertyName("required")]
            public bool Required { get; set; }

            [JsonPropertyName("server_only")]
            public bool ServerOnly { get; set; }

            [JsonPropertyName("minimum_length")]
            public int MinimumLength { get; set; }

            [JsonPropertyName("maximum_length")]
            public int MaximumLength { get; set; }

            /// <summary>
            /// Gets the default value as a JsonElement.
            /// </summary>
            /// <returns>The default value as a JsonElement.</returns>
            public JsonElement GetDefaultValue()
            {
                if (Type == null)
                    return KepJsonContext.WrapInJsonElement(null);

                if (DefaultValue == null)
                    return KepJsonContext.WrapInJsonElement(null);

                if (DefaultValue is JsonElement jsonElement)
                {
                    // Überprüfen, ob ValueKind mit Type übereinstimmt, ansonsten konvertieren oder Standardwert zurückgeben
                    switch (Type.ToLower())
                    {
                        case "allowdeny":
                        case "enabledisable":
                        case "yesno":
                            if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(false);

                        case "string":
                        case "password":
                        case "localfilespec":
                        case "uncfilespec":
                        case "localpathspec":
                        case "uncpathspec":
                        case "stringwithbrowser":
                            if (jsonElement.ValueKind == JsonValueKind.String)
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(string.Empty);

                        case "stringarray":
                        case "blob":
                            if (jsonElement.ValueKind == JsonValueKind.Array)
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(new string[0]);

                        case "integer":
                        case "hex":
                        case "octal":
                        case "signedinteger":
                        case "timeofday":
                        case "date":
                        case "dateandtime":
                            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out int _))
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(0);

                        case "real4":
                            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetSingle(out float _))
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(0.0f);

                        case "real8":
                            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetDouble(out double _))
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(0.0);

                        case "enumeration":
                            if (jsonElement.ValueKind == JsonValueKind.Number)
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(0);

                        case "proparray":
                            if (jsonElement.ValueKind == JsonValueKind.Object)
                                return jsonElement;
                            return KepJsonContext.WrapInJsonElement(new object[0]);

                        default:
                            return KepJsonContext.WrapInJsonElement(null);
                    }
                }
                else
                {
                    switch (Type.ToLower())
                    {
                        case "allowdeny":
                        case "enabledisable":
                        case "yesno":
                            return KepJsonContext.WrapInJsonElement(Convert.ToBoolean(DefaultValue ?? false));
                        case "string":
                        case "password":
                        case "localfilespec":
                        case "uncfilespec":
                        case "localpathspec":
                        case "uncpathspec":
                        case "stringwithbrowser":
                            return KepJsonContext.WrapInJsonElement(DefaultValue?.ToString() ?? string.Empty);
                        case "stringarray":
                        case "blob":
                            return KepJsonContext.WrapInJsonElement(DefaultValue ?? new string[0]);
                        case "integer":
                        case "hex":
                        case "octal":
                        case "signedinteger":
                        case "timeofday":
                        case "date":
                        case "dateandtime":
                            return KepJsonContext.WrapInJsonElement(Convert.ToInt32(DefaultValue ?? 0));
                        case "real4":
                            return KepJsonContext.WrapInJsonElement(Convert.ToSingle(DefaultValue ?? 0.0f));
                        case "real8":
                            return KepJsonContext.WrapInJsonElement(Convert.ToDouble(DefaultValue ?? 0.0));
                        case "enumeration":
                            return KepJsonContext.WrapInJsonElement(DefaultValue ?? 0);
                        case "proparray":
                            return KepJsonContext.WrapInJsonElement(DefaultValue ?? new object[0]);
                        default:
                            return KepJsonContext.WrapInJsonElement(null);
                    }
                }
            }
        }
    }

    [JsonSerializable(typeof(List<Docs.Driver>))]
    [JsonSerializable(typeof(List<Docs.Channel>))]
    [JsonSerializable(typeof(List<Docs.Device>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class KepDocsJsonContext : JsonSerializerContext
    {
    }
}
