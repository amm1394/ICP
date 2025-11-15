using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Interfaces.Repositories;
using Core.Icp.Domain.Interfaces.Services;

namespace Core.Icp.Application.Services.Projects;

/// <summary>
/// پیاده‌سازی سرویس پروژه‌ها با استفاده از UnitOfWork و Repositoryها.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _unitOfWork.Projects.GetAllAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        // برای خواندن پروژه همراه با Samples از متد مخصوص استفاده می‌کنیم
        return await _unitOfWork.Projects.GetWithSamplesAsync(id);
    }

    public async Task<IEnumerable<Project>> GetRecentProjectsAsync(int count)
    {
        return await _unitOfWork.Projects.GetRecentProjectsAsync(count);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project is null)
            return false;

        await _unitOfWork.Projects.DeleteAsync(project);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public Task<string> SaveProjectToFileAsync(Guid projectId, string filePath)
    {
        // TODO: در فازهای بعدی با لایه Files پیاده‌سازی می‌شود
        throw new NotImplementedException();
    }

    public Task<Project> LoadProjectFromFileAsync(string filePath)
    {
        // TODO: در فازهای بعدی با لایه Files پیاده‌سازی می‌شود
        throw new NotImplementedException();
    }
}
