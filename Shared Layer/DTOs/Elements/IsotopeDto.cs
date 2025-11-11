using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO برای نمایش ایزوتوپ
    /// </summary>
    public class IsotopeDto : BaseDto
    {
        public Guid ElementId { get; set; }
        public int MassNumber { get; set; }
        public decimal Abundance { get; set; }
        public bool IsStable { get; set; }
    }
}