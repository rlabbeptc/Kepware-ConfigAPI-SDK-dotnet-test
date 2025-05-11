using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Kepware.Api.Model;
using Kepware.Api.Serializer;
using Shouldly;

namespace Kepware.Api.Test.Serializer
{

    public class YamlSerializerTests
    {
        private static YamlSerializer CreateSerializer() =>
            new YamlSerializer(new Mock<ILogger<YamlSerializer>>().Object);

        [Fact]
        public async Task LoadFromYaml_Should_Set_Name_From_Directory()
        {
            var serializer = CreateSerializer();
            var path = Path.Combine(Path.GetTempPath(), "Channel1");
            var file = Path.Combine(path, "channel.yaml");

            Directory.CreateDirectory(path);
            await File.WriteAllTextAsync(file, "description: test");

            try
            {
                var result = await serializer.LoadFromYaml<Channel>(file);
                result.Name.ShouldBe("Channel1");
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
        }

        [Fact]
        public async Task LoadFromYaml_Should_Create_Default_Entity_When_File_Missing()
        {
            var serializer = CreateSerializer();
            var path = Path.Combine(Path.GetTempPath(), "ChannelMissing");
            var file = Path.Combine(path, "missing.yaml");
            Directory.CreateDirectory(path);

            try
            {
                var result = await serializer.LoadFromYaml<Channel>(file);
                result.ShouldNotBeNull();
                result.Name.ShouldBe("ChannelMissing");
            }
            finally
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
        }

        [Fact]
        public async Task SaveAsYaml_Should_Not_Overwrite_When_Content_Is_Same()
        {
            var serializer = CreateSerializer();
            var path = Path.Combine(Path.GetTempPath(), "SameContent");
            var file = Path.Combine(path, "file.yaml");

            var entity = new Channel { Name = "Test" };
            await serializer.SaveAsYaml(file, entity);

            var initialWriteTime = File.GetLastWriteTimeUtc(file);
            await Task.Delay(100); // Ensure timestamp could change

            await serializer.SaveAsYaml(file, entity);
            File.GetLastWriteTimeUtc(file).ShouldBe(initialWriteTime); // Should not have changed
        }

        [Fact]
        public async Task SaveAsYaml_Should_Write_File_When_Not_Exists()
        {
            var serializer = CreateSerializer();
            var path = Path.Combine(Path.GetTempPath(), "WriteNew");
            var file = Path.Combine(path, "new.yaml");

            if (Directory.Exists(path)) Directory.Delete(path, true);
            var entity = new Channel { Name = "New" };
            entity.Description = "New description";

            await serializer.SaveAsYaml(file, entity);
            File.Exists(file).ShouldBeTrue();

            if (File.Exists(file)) File.Delete(file);
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }


        [Fact]
        public async Task SaveAsYaml_Should_Delete_File_When_Default_Values()
        {
            var serializer = CreateSerializer();
            var path = Path.Combine(Path.GetTempPath(), "WriteNew");
            var file = Path.Combine(path, "new.yaml");

            if (Directory.Exists(path)) Directory.Delete(path, true);
            var entity = new Channel { Name = "New" };
            entity.Description = "New description";

            await serializer.SaveAsYaml(file, entity);
            File.Exists(file).ShouldBeTrue();

            if (File.Exists(file)) File.Delete(file);
            if (Directory.Exists(path)) Directory.Delete(path, true);

            entity.Description = null; // Set to default value

            await serializer.SaveAsYaml(file, entity);
            File.Exists(file).ShouldBeFalse();
        }
    }
}
