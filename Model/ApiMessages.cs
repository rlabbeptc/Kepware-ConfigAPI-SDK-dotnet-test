using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KepwareSync.Model
{
    public class ApiResult
    {

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode => (HttpStatusCode)Code;


        /// <summary>
        /// true if System.Net.Http.HttpResponseMessage.StatusCode was in the range 200-299
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessStatusCode => Code >= 200 && Code < 300;
    }

    public class ApiStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Healthy { get; set; } = false;
    }

    public enum ProductType
    {
        Unknown = 0,
        ThingWorxKepwareEdge = 13,
        KEPServerEX = 12
    }

    public class ProductInfo
    {
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("product_version")]
        public string ProductVersion { get; set; } = string.Empty;

        [JsonPropertyName("product_version_major")]
        public int ProductVersionMajor { get; set; }

        [JsonPropertyName("product_version_minor")]
        public int ProductVersionMinor { get; set; }

        [JsonPropertyName("product_version_build")]
        public int ProductVersionBuild { get; set; }

        [JsonPropertyName("product_version_patch")]
        public int ProductVersionPatch { get; set; }

        public ProductType ProductType =>
            int.TryParse(ProductId, out var id) ? (ProductType)id : Enum.TryParse<ProductType>(ProductName, out var prodType) ? prodType : ProductType.Unknown;

        /// <summary>
        ///  Added to Kepware Server v6.17 / Kepware Edge v1.10 and later builds
        /// </summary>
        [JsonIgnore]
        public bool SupportsJsonProjectLoadService =>
            (ProductType == ProductType.KEPServerEX && (ProductVersionMajor > 6 || (ProductVersionMajor == 6 && ProductVersionMinor >= 17))) ||
            (ProductType == ProductType.ThingWorxKepwareEdge && (ProductVersionMajor > 1 || (ProductVersionMajor == 1 && ProductVersionMinor >= 10)));
    }
}
