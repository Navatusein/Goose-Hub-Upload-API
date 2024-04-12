using MassTransit;
using System.ComponentModel.DataAnnotations;
using UploadApi.Models;

namespace UploadApi.MassTransit.Events
{
    /// <summary>
    /// Content data type enum
    /// </summary>
    public enum DataTypeEnum
    {
        /// <summary>
        /// Movie
        /// </summary>
        Movie = 1,

        /// <summary>
        /// Serial
        /// </summary>
        Serial = 2,

        /// <summary>
        /// Anime
        /// </summary>
        Anime = 3
    }

    /// <summary>
    /// Model for VideoProcessingEvent
    /// </summary>
    [EntityName("video-processing-service-video-processing-job")]
    [MessageUrn("VideoProcessingEvent")]
    public class VideoProcessingEvent
    {
        /// <summary>
        /// Gets or Sets FileUrl
        /// </summary>
        [Required]
        public string FileUrl { get; set; } = null!;

        /// <summary>
        /// Gets or Sets FileExtension
        /// </summary>
        [Required]
        public string FileExtension { get; set; } = null!;

        /// <summary>
        /// Gets or Sets ContentType
        /// </summary>
        [Required]
        public string ContentType { get; set; } = null!;

        /// <summary>
        /// Gets or Sets Quality
        /// </summary>
        [Required]
        public ContentQuality Quality { get; set; }

        /// <summary>
        /// Gets or Sets DataType
        /// </summary>
        [Required]
        public DataTypeEnum DataType { get; set; }

        /// <summary>
        /// Gets or Sets MovieId
        /// </summary>
        [Required]
        public string ContentId { get; set; } = null!;

        /// <summary>
        /// Gets or Sets IsEpisode
        /// </summary>
        public bool IsEpisode { get; set; }
    }
}
