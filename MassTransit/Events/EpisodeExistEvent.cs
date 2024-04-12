using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace UploadApi.MassTransit.Events
{
    /// <summary>
    /// Model for EpisodeExistEvent
    /// </summary>
    [EntityName("movie-api-episode-exist")]
    [MessageUrn("EpisodeExistEvent")]
    public class EpisodeExistEvent
    {
        /// <summary>
        /// Gets or Sets EpisodeId
        /// </summary>
        [Required]
        public string EpisodeId { get; set; } = null!;
    }
}
