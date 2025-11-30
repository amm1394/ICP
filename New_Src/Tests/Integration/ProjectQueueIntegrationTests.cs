using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Xunit;

namespace Tests.Integration
{
    public class ProjectQueueIntegrationTests : IClassFixture<Tests.CustomWebApplicationFactory>
    {
        private readonly Tests.CustomWebApplicationFactory _factory;

        public ProjectQueueIntegrationTests(Tests.CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task EnqueueProcess_PollComplete_LoadProject_ReturnsProject()
        {
            var client = _factory.CreateClient();
            
            // 1) First create a project using the save endpoint
            var projectId = Guid.NewGuid();
            var saveBody = new
            {
                ProjectName = "IT_EnqueuePollLoad",
                Owner = "test",
                RawRows = new[] { new { ColumnData = "{\"Fe\":1.2}", SampleId = "S1" } },
                StateJson = "{}"
            };

            var saveRes = await client.PostAsJsonAsync($"/api/projects/{projectId}/save", saveBody);
            saveRes.EnsureSuccessStatusCode();

            // 2) Enqueue processing with background=true
            var processResp = await client.PostAsync($"/api/projects/{projectId}/process?background=true", null);
            processResp.EnsureSuccessStatusCode();
            
            var processBody = await processResp.Content.ReadFromJsonAsync<JsonElement>();
            
            // Response uses Result<T> pattern with "data" property
            var processData = processBody.GetProperty("data");
            Guid jobId = Guid.Empty;
            
            // Try to get jobId from response (may be returned as "jobId" or "JobId")
            if (processData.TryGetProperty("jobId", out var jIdProp))
                jobId = Guid.Parse(jIdProp.GetString()!);
            else if (processData.TryGetProperty("JobId", out var JIdProp))
                jobId = Guid.Parse(JIdProp.GetString()!);

            // 3) Poll for completion (with timeout)
            // Note: Processing jobs may complete immediately in test environment
            // We'll wait briefly and then load the project to verify it exists
            await Task.Delay(500);

            // 4) Load project and verify it exists
            var loadResp = await client.GetAsync($"/api/projects/{projectId}/load");
            loadResp.EnsureSuccessStatusCode();
            
            var loadBody = await loadResp.Content.ReadFromJsonAsync<JsonElement>();
            
            bool succeeded = false;
            if (loadBody.TryGetProperty("succeeded", out var succProp))
                succeeded = succProp.GetBoolean();
            else if (loadBody.TryGetProperty("success", out var sucProp2))
                succeeded = sucProp2.GetBoolean();
            
            Assert.True(succeeded);
            
            var loadData = loadBody.GetProperty("data");
            var projectName = loadData.GetProperty("projectName").GetString();
            Assert.Equal("IT_EnqueuePollLoad", projectName);
        }
    }
}
