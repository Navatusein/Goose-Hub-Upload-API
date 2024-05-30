using System.ComponentModel.DataAnnotations;

namespace UploadApi.Dtos
{
    /// <summary>
    /// Model for upload content
    /// </summary>
    public class UploadContentDto
    {
        /// <summary>
        /// Gets or Sets MovieId
        /// </summary>
        [Required]
        public string ContentId { get; set; } = null!;

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        public bool IsEpisode { get; set; }

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
