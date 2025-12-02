using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebUI.Services;

// ============================================
// Project DTOs
// ============================================

public class ProjectDto
{
    // From import jobs response
    [JsonPropertyName("jobId")]
    public Guid JobId { get; set; }
    
    [JsonPropertyName("resultProjectId")]
    public Guid? ResultProjectId { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }
    
    [JsonPropertyName("state")]
    public int State { get; set; }
    
    [JsonPropertyName("totalRows")]
    public int TotalRows { get; set; }
    
    [JsonPropertyName("processedRows")]
    public int ProcessedRows { get; set; }
    
    [JsonPropertyName("percent")]
    public int Percent { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    // Helper properties for UI compatibility
    public Guid ProjectId => ResultProjectId ?? JobId;
    public string Owner => "";
    public DateTime LastModifiedAt => UpdatedAt;
    public int RowCount => TotalRows;
    
    // Dashboard helpers
    public Guid Id => ProjectId;
    public string Name => ProjectName ?? $"Project-{JobId.ToString().Substring(0, 8)}";
    public int SampleCount => RowCount;
    public DateTime LastAccessed => LastModifiedAt;
    
    // Status helpers
    public bool IsCompleted => State == 2;
    public bool IsQueued => State == 0;
    public bool IsProcessing => State == 1;
    public bool IsFailed => State == 3;
}

public class ProjectListResult
{
    [JsonPropertyName("items")]
    public List<ProjectDto> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    public int TotalCount => Total;
}

// ============================================
// Project Service
// ============================================

public class ProjectService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectService> _logger;
    private readonly AuthService _authService;

    // Current selected project
    private static Guid? _currentProjectId;
    private static ProjectDto? _currentProject;

    public ProjectService(IHttpClientFactory clientFactory, ILogger<ProjectService> logger, AuthService authService)
    {
        _httpClient = clientFactory.CreateClient("Api");
        _logger = logger;
        _authService = authService;
    }

    public Guid? CurrentProjectId => _currentProjectId;
    public ProjectDto? CurrentProject => _currentProject;

    public void SetCurrentProject(ProjectDto? project)
    {
        _currentProject = project;
        _currentProjectId = project?.ProjectId;
    }

    public void SetCurrentProject(Guid projectId)
    {
        _currentProjectId = projectId;
    }

    /// <summary>
    /// Get list of all projects for Dashboard (simplified)
    /// </summary>
    public async Task<ServiceResult<List<ProjectDto>>> GetProjectsAsync()
    {
        var result = await GetProjectsAsync(1, 100, null);
        if (result.Succeeded && result.Data != null)
        {
            return ServiceResult<List<ProjectDto>>.Success(result.Data.Items);
        }
        return ServiceResult<List<ProjectDto>>.Fail(result.Message ?? "Failed to get projects");
    }

    /// <summary>
    /// Get list of all projects
    /// </summary>
    public async Task<ServiceResult<ProjectListResult>> GetProjectsAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        try
        {
            SetAuthHeader();

            var url = $"projects/import/jobs?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Projects response: {Content}", content);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResult<ProjectListResult>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Succeeded == true && result.Data != null)
                {
                    // Filter to show only completed projects by default if search not specified
                    if (string.IsNullOrEmpty(search))
                    {
                        result.Data.Items = result.Data.Items.Where(p => p.IsCompleted).ToList();
                    }
                    return ServiceResult<ProjectListResult>.Success(result.Data);
                }

                return ServiceResult<ProjectListResult>.Fail(result?.Message ?? "Failed to load projects");
            }

            return ServiceResult<ProjectListResult>.Fail($"Server error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading projects");
            return ServiceResult<ProjectListResult>.Fail($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a single project by ID
    /// </summary>
    public async Task<ServiceResult<ProjectDto>> GetProjectAsync(Guid projectId)
    {
        try
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync($"projects/{projectId}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResult<ProjectDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Succeeded == true && result.Data != null)
                {
                    return ServiceResult<ProjectDto>.Success(result.Data);
                }

                return ServiceResult<ProjectDto>.Fail(result?.Message ?? "Project not found");
            }

            return ServiceResult<ProjectDto>.Fail($"Server error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading project {ProjectId}", projectId);
            return ServiceResult<ProjectDto>.Fail($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid projectId)
    {
        try
        {
            SetAuthHeader();

            var response = await _httpClient.DeleteAsync($"projects/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                if (_currentProjectId == projectId)
                {
                    _currentProjectId = null;
                    _currentProject = null;
                }
                return ServiceResult<bool>.Success(true);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResult<bool>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return ServiceResult<bool>.Fail(result?.Message ?? $"Failed to delete project");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }

    private void SetAuthHeader()
    {
        var token = _authService.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
