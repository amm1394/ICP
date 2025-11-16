using System.Collections.Generic;
using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Models.Projects
{
    public class PagedProjectListResult
    {
        public IReadOnlyList<Project> Items { get; set; } = new List<Project>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
