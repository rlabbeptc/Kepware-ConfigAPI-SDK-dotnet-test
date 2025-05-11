using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Microsoft.Extensions.Logging;
using Kepware.Api.Util;
using System.IO;

namespace Kepware.Api.Serializer
{
    /// <summary>
    /// Provides methods for serializing and deserializing YAML data.
    /// </summary>
    public class YamlSerializer
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly ILogger<YamlSerializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSerializer"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging information.</param>
        public YamlSerializer(ILogger<YamlSerializer> logger)
        {
            _logger = logger;

            var context = new KepYamlContext();

            var converter = new BaseEntityYamlTypeConverter(Properties.NonSerialized.AsHashSet);

            _serializer = new StaticSerializerBuilder(context)
                .WithTypeConverter(converter)
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // Optional
                .Build();

            _deserializer = new StaticDeserializerBuilder(context)
                .WithTypeConverter(new BaseEntityYamlTypeConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // Optional
                .Build();
        }

        /// <summary>
        /// Loads an entity from a YAML file.
        /// </summary>
        /// <typeparam name="T">The type of the entity to load.</typeparam>
        /// <param name="filePath">The path to the YAML file.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The loaded entity.</returns>
        public async Task<T> LoadFromYaml<T>(string filePath, CancellationToken cancellationToken = default)
            where T : BaseEntity, new()
        {
            FileInfo file = new FileInfo(filePath);
            T entity;

            if (!file.Exists)
            {
                entity = new T();
            }
            else
            {
                var yaml = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                entity = _deserializer.Deserialize<T>(yaml);
            }

            if (entity is NamedEntity namedEntity)
            {
                if (file.Directory == null)
                    throw new InvalidOperationException($"Directory of file {filePath} is null");

                // Use DirectoryInfo.Name for robust, platform-independent directory name extraction
                namedEntity.Name = file.Directory.Name.UnescapeDiskEntry();
            }

            return entity;
        }

        /// <summary>
        /// Saves an entity as a YAML file.
        /// </summary>
        /// <param name="filePath">The path to the YAML file.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        public async Task SaveAsYaml(string filePath, object entity, CancellationToken cancellationToken = default)
        {
            var yaml = _serializer.Serialize(entity);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // Create directory if it doesn't exist

            if (yaml.Trim().Equals("{}"))
            {
                //don't write empty files
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File {FilePath} was empty and has been deleted", filePath);
                }
            }
            else
            {
                if (!File.Exists(filePath) || await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false) != yaml)
                {
                    await File.WriteAllTextAsync(filePath, yaml, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Content is the same -> dont rewrite to keep the change date & time
                }
            }
        }
    }

}
