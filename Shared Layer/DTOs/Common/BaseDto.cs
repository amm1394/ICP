namespace Shared.Icp.DTOs.Common
{
    /// <summary>
    /// Base data transfer object providing common metadata for all DTOs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Standardizes identifier and timestamp fields across DTOs, enabling consistent handling in APIs,
    /// clients, and logs. Derive your presentation models from this type to inherit the common
    /// <see cref="Id"/>, <see cref="CreatedAt"/>, and <see cref="UpdatedAt"/> fields.
    /// </para>
    /// <para>
    /// Guidelines:
    /// - <see cref="Id"/> is a GUID to ensure global uniqueness across services and processes.
    /// - <see cref="CreatedAt"/> should be recorded in UTC and serialized as ISO-8601 in JSON.
    /// - <see cref="UpdatedAt"/> is null when the resource has not been modified since creation; otherwise a UTC timestamp.
    /// - User-facing messages (validation/errors/success) should remain Persian elsewhere; this type only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example of a derived DTO
    /// public sealed class ProjectSummaryDto : BaseDto
    /// {
    ///     public string Name { get; set; } = string.Empty;
    /// }
    ///
    /// // Typical usage
    /// var dto = new ProjectSummaryDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     UpdatedAt = null,
    ///     Name = "Sample Project"
    /// };
    /// </code>
    /// </example>
    public abstract class BaseDto
    {
        /// <summary>
        /// Globally unique identifier for the resource represented by the DTO.
        /// </summary>
        /// <remarks>
        /// Use a stable value to correlate resources across services and logs. When mapping from domain entities,
        /// this typically mirrors the entity's primary key.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Guid"/> value that uniquely identifies the resource.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid Id { get; set; }

        /// <summary>
        /// The UTC timestamp when the resource was created.
        /// </summary>
        /// <remarks>
        /// Should be stored and transferred in UTC (ISO-8601 in JSON) to avoid time zone inconsistencies.
        /// </remarks>
        /// <value>
        /// A <see cref="System.DateTime"/> value in UTC.
        /// </value>
        /// <example>
        /// 2025-04-01T12:30:00Z
        /// </example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The UTC timestamp when the resource was last updated, or null if never updated.
        /// </summary>
        /// <remarks>
        /// Nullable to distinguish between never-updated resources and those with modification history. When present,
        /// the value should be in UTC and serialized as ISO-8601 in JSON.
        /// </remarks>
        /// <value>
        /// A <see cref="System.DateTime"/> value in UTC, or <c>null</c>.
        /// </value>
        /// <example>
        /// 2025-04-10T08:20:00Z
        /// </example>
        public DateTime? UpdatedAt { get; set; }
    }
}