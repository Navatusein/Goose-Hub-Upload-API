using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace UploadApi.MassTransit.Events
{
    /// <summary>
    /// Model for AnimeAddContentEvent
    /// </summary>
    [EntityName("movie-api-content-exist")]
    [MessageUrn("ContentExistEvent")]
    public class ContentExistEvent
    {
        /// <summary>
        /// Gets or Sets ContentId
        /// </summary>
        [Required]
        public string ContentId { get; set; } = null!;
    }
}
