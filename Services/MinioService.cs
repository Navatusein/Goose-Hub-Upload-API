using Minio.DataModel.Args;
using Minio;

namespace UploadApi.Services
{
    /// <summary>
    /// Service for work with Minio
    /// </summary>
    public class MinioService
    {
        private static Serilog.ILogger Logger => Serilog.Log.ForContext<MinioService>();

        private readonly IMinioClient _minioClient;

        private readonly string _contentBucket;

        /// <summary>
        /// Constructor
        /// </summary>
        public MinioService(IConfiguration config)
        {
            var endpoint = config.GetSection("MinIO:Endpoint").Get<string>();
            var useSsl = config.GetSection("MinIO:UseSSL").Get<bool>();
            var region = config.GetSection("MinIO:Region").Get<string>();
            var accessKey = config.GetSection("MinIO:AccessKey").Get<string>();
            var secretKey = config.GetSection("MinIO:SecretKey").Get<string>();

            _contentBucket = config.GetSection("MinIO:ContentBucket").Get<string>()!;

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithRegion(region)
                .WithSSL(useSsl)
                .Build();
        }

        /// <summary>
        /// Upload content ot MinIO
        /// </summary>
        public async Task<string> UploadContent(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            string objectName = Guid.NewGuid().ToString() + extension;

            var stream = file.OpenReadStream();

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_contentBucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

            return objectName;
        }

        /// <summary>
        /// Generate presigned url 
        /// </summary>
        public async Task<string> GetPresignedUrl(string objectPath)
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_contentBucket)
                .WithObject(objectPath)
                .WithExpiry(60 * 60 * 24 * 7);

            var presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);

            return presignedUrl;
        }
    }
}
