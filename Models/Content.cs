namespace UploadApi.Models
{
    /// <summary>
    /// Content quality enum
    /// </summary>
    public enum ContentQuality
    {
        /// <summary>
        /// 480p
        /// </summary>
        SD = 480,
        /// <summary>
        /// 720p
        /// </summary>
        HD = 720,
        /// <summary>
        /// 1080p
        /// </summary>
        FullHD = 1080
    }

    /// <summary>
    /// Model for episode, movie data
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Gets or Sets Quality
        /// </summary>
        public ContentQuality Quality { get; set; }

        /// <summary>
        /// Gets or Sets Path
        /// </summary>
        public string Path { get; set; } = null!;
    }
}
