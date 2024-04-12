using MassTransit;
using System.ComponentModel.DataAnnotations;
using UploadApi.Models;

namespace UploadApi.MassTransit.Events
{
    /// <summary>
    /// Model for MovieAddContentEvent
    /// </summary>
    [EntityName("movie-api-movie-add-content")]
    [MessageUrn("MovieAddContentEvent")]
    public class MovieAddContentEvent
    {
        /// <summary>
        /// Gets or Sets MovieId
        /// </summary>
        [Required]
        public string MovieId { get; set; } = null!;

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        [Required]
        public Content Content { get; set; } = null!;
    }
}
