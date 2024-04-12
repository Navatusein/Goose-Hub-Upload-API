using MassTransit;
using System.ComponentModel.DataAnnotations;
using UploadApi.Models;

namespace UploadApi.MassTransit.Events
{
    /// <summary>
    /// Model for AnimeAddContentEvent
    /// </summary>
    [EntityName("movie-api-anime-add-content")]
    [MessageUrn("AnimeAddContentEvent")]
    public class AnimeAddContentEvent
    {
        /// <summary>
        /// Gets or Sets ContentId
        /// </summary>
        [Required]
        public string ContentId { get; set; } = null!;

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        [Required]
        public bool IsEpisode { get; set; }

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        [Required]
        public Content Content { get; set; } = null!;
    }
}
