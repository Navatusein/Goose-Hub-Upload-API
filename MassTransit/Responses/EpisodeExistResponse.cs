using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace UploadApi.MassTransit.Responses
{
    /// <summary>
    /// Model for response on EpisodeExistEvent
    /// </summary>
    [MessageUrn("EpisodeExistResponse")]
    public class EpisodeExistResponse
    {
        /// <summary>
        /// Gets or Sets EpisodeId
        /// </summary>
        [Required]
        public string EpisodeId { get; set; } = null!;

        /// <summary>
        /// Gets or Sets ContentId
        /// </summary>
        [Required]
        public bool IsExists { get; set; }
    }
}
