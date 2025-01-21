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

namespace Kepware.Api.Serializer
{
    public class YamlSerializer
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly ILogger<YamlSerializer> _logger;

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

        public async Task<T> LoadFromYaml<T>(string filePath)
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
                var yaml = await System.IO.File.ReadAllTextAsync(filePath);
                entity = _deserializer.Deserialize<T>(yaml);
            }

            if (entity is NamedEntity namedEntity)
            {
                namedEntity.Name = file.DirectoryName!.Split('\\').Last().UnescapeDiskEntry();
            }

            return entity;
        }

        public async Task SaveAsYaml(string filePath, object entity)
        {
            var yaml = _serializer.Serialize(entity);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // Erstelle Verzeichnis, falls es nicht existiert

            if (yaml.Trim().Equals("{}"))
            {
                //don't write empty files
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File {filePath} was empty and has been deleted", filePath);
                }
            }
            else
            {
                if (!File.Exists(filePath) || await File.ReadAllTextAsync(filePath) != yaml)
                {
                    await File.WriteAllTextAsync(filePath, yaml);
                }
                else
                {
                    // Content is the same -> dont rewrite to keep the change date & time
                }
            }
        }
    }

}
