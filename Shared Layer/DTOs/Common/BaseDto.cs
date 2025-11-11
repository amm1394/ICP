namespace Shared.Icp.DTOs.Common
{
    public abstract class BaseDto
    {
        public Guid Id { get; set; }  // ✅ باید Guid باشه
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}