using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Tests;

public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Save -> Load project works")]
    public async Task Save_Then_Load_Project()
    {
        var projectId = Guid.NewGuid();
        var saveBody = new
        {
            ProjectName = "IT_SaveLoad",
            Owner = "test",
            RawRows = new[] { new { ColumnData = "{\"Fe\":1.2}", SampleId = "S1" } },
            StateJson = "{}"
        };

        var saveRes = await _client.PostAsJsonAsync($"/api/projects/{projectId}/save", saveBody);
        saveRes.EnsureSuccessStatusCode();

        var loadRes = await _client.GetAsync($"/api/projects/{projectId}/load");
        loadRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var loadContent = await loadRes.Content.ReadFromJsonAsync<JsonElement>();
        var loadData = loadContent.GetProperty("data");
        loadData.GetProperty("projectName").GetString().Should().Be("IT_SaveLoad");
    }

    [Fact(DisplayName = "Import CSV (sync) creates project and rows")]
    public async Task ImportCsv_Sync_CreatesProject()
    {
        // prepare csv content
        var csv = "SampleId,Fe,Cu\nS1,12.3,0.5\nS2,11.9,0.6\n";
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        content.Add(fileContent, "file", "data.csv");
        content.Add(new StringContent("ImportedFromTest"), "projectName");
        content.Add(new StringContent("dev"), "owner");

        var res = await _client.PostAsync("/api/projects/import", content);
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        bool succeeded;
        if (body.TryGetProperty("succeeded", out var succProp))
            succeeded = succProp.GetBoolean();
        else if (body.TryGetProperty("success", out var succProp2))
            succeeded = succProp2.GetBoolean();
        else
            throw new InvalidOperationException("Response JSON does not contain 'succeeded' or 'success' property.");

        succeeded.Should().BeTrue();

        var bodyData = body.GetProperty("data");
        Guid projectId = Guid.Parse(bodyData.GetProperty("projectId").GetString()!);

        // load and verify that project is accessible
        var loadRes = await _client.GetAsync($"/api/projects/{projectId}/load");
        loadRes.StatusCode.Should().Be(HttpStatusCode.OK);
        // اگر بخواهی می‌توانی اینجا هم JSON را بخوانی و روی فیلدهایی مثل projectName/rows Assert بزنی
    }

    [Fact(DisplayName = "Process project computes and saves a ProjectState")]
    public async Task ProcessProject_ProducesState()
    {
        // 1) create a project by calling save endpoint
        var projectId = Guid.NewGuid();
        var saveBody = new
        {
            ProjectName = "IT_Process",
            Owner = "test",
            RawRows = new[]
            {
                new { ColumnData = "{\"Fe\":1.2}", SampleId = "S1" },
                new { ColumnData = "{\"Fe\":2.3}", SampleId = "S2" },
                new { ColumnData = "{\"Fe\":3.4}", SampleId = "S3" }
            },
            StateJson = "{}"
        };

        var saveRes = await _client.PostAsJsonAsync($"/api/projects/{projectId}/save", saveBody);
        saveRes.EnsureSuccessStatusCode();

        // 2) call process endpoint
        var procRes = await _client.PostAsync($"/api/projects/{projectId}/process", null);
        procRes.EnsureSuccessStatusCode();

        var procBody = await procRes.Content.ReadFromJsonAsync<JsonElement>();

        bool procSucceeded;
        if (procBody.TryGetProperty("succeeded", out var procSuccProp))
            procSucceeded = procSuccProp.GetBoolean();
        else if (procBody.TryGetProperty("success", out var procSuccProp2))
            procSucceeded = procSuccProp2.GetBoolean();
        else
            throw new InvalidOperationException("Process response JSON does not contain 'succeeded' or 'success' property.");

        procSucceeded.Should().BeTrue();
        // ProjectStateId returned
        var stateId = procBody.GetProperty("data").GetProperty("projectStateId").GetInt32();

        // 3) fetch latest state via load endpoint and check LatestStateJson exists
        var loadRes = await _client.GetAsync($"/api/projects/{projectId}/load");
        loadRes.EnsureSuccessStatusCode();

        var loadBody = await loadRes.Content.ReadFromJsonAsync<JsonElement>();
        var loadData2 = loadBody.GetProperty("data");
        loadData2.GetProperty("projectName").GetString().Should().Be("IT_Process");
        loadData2.GetProperty("latestStateJson").GetString().Should().NotBeNullOrEmpty();
    }
}
