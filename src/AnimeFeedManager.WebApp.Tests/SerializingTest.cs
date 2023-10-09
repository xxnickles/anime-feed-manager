using System.Collections.Immutable;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnimeFeedManager.WebApp.Tests
{
    public record ProblemDetails(string Type, string Title, string Instance, HttpStatusCode Status,
        ImmutableDictionary<string, string[]>? Errors, string? Detail);

    public record NotMatch(int Value);

    public class SerializingTest
    {
        [Fact]
        public async Task Should_Serialize_From_A()
        {
            const string problemDetails = @"{
            ""Errors"": {
            ""Email"": [
            ""Email is already registered in the system""
            ]
            },
            ""Type"": ""https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422"",
            ""Title"": ""One or more validation errors occurred."",
            ""Instance"": ""http://localhost:4280/api/user"",
            ""Status"": 422
        }";

            var bytes = Encoding.UTF8.GetBytes(problemDetails);
            var stream = new MemoryStream(bytes);
            var sut = await JsonSerializer.DeserializeAsync<ProblemDetails>(stream);
            sut?.Errors.Should().NotBeNullOrEmpty();
            sut?.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Should_Serialize_From_B()
        {
            const string problemDetails = @"{            
            ""Type"": ""https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422"",
            ""Title"": ""One or more validation errors occurred."",
            ""Instance"": ""http://localhost:4280/api/user"",
            ""Status"": 422,
            ""Detail"": ""An Error""
        }";

            var bytes = Encoding.UTF8.GetBytes(problemDetails);
            var stream = new MemoryStream(bytes);
            var sut = await JsonSerializer.DeserializeAsync<ProblemDetails>(stream);
            sut?.Detail.Should().NotBeNullOrEmpty();
            sut?.Errors.Should().BeNull();
        }
    }
}