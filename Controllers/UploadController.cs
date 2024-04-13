using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UploadApi.Dtos;
using UploadApi.MassTransit.Events;
using UploadApi.MassTransit.Responses;
using UploadApi.Models;
using UploadApi.Services;

namespace UploadApi.Controllers
{
    /// <summary>
    /// Upload Controller
    /// </summary>
    [Route("/v1/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private static Serilog.ILogger Logger => Serilog.Log.ForContext<UploadController>();

        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<ContentExistEvent> _clientContentExist;
        private readonly IRequestClient<EpisodeExistEvent> _clientEpisodeExist;
        private readonly MinioService _minioService;

        /// <summary>
        /// Constructor
        /// </summary>
        public UploadController(IPublishEndpoint publishEndpoint, MinioService minioService, 
            IRequestClient<ContentExistEvent> clientContentExist, IRequestClient<EpisodeExistEvent> clientEpisodeExist)
        {
            _publishEndpoint = publishEndpoint;
            _minioService = minioService;
            _clientContentExist = clientContentExist;
            _clientEpisodeExist = clientEpisodeExist;
        }

        /// <summary>
        /// Upload Movie
        /// </summary>
        /// <param name="uploadContentDto">UploadContentDto</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        [HttpPost]
        [Route("movie/content")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "OK")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorDto), description: "Not Found")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadMovie([FromForm] UploadContentDto uploadContentDto, CancellationToken cancellationToken)
        {
            var contentExistEvent = new ContentExistEvent()
            {
                ContentId = uploadContentDto.ContentId
            };

            var result = await _clientContentExist.GetResponse<ContentExistResponse>(contentExistEvent, cancellationToken);

            if (!result.Message.IsExists)
                return StatusCode(404, new ErrorDto("Content not found", "404"));

            var path = await _minioService.UploadContent(uploadContentDto.File);

            var movieAddContentEvent = new MovieAddContentEvent()
            {
                MovieId = uploadContentDto.ContentId,
                Content = new Content()
                {
                    Quality = ContentQuality.FullHD,
                    Path = path
                }
            };

            await _publishEndpoint.Publish(movieAddContentEvent);

            var url = await _minioService.GetPresignedUrl(path);

            var videoProcessingEvent = new VideoProcessingEvent()
            {
                FileUrl = url,
                FileExtension = Path.GetExtension(uploadContentDto.File.FileName),
                ContentType = uploadContentDto.File.ContentType,
                DataType = DataTypeEnum.Movie,
                ContentId = uploadContentDto.ContentId,
                IsEpisode = false
            };

            videoProcessingEvent.Quality = ContentQuality.HD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            videoProcessingEvent.Quality = ContentQuality.SD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            return StatusCode(200, url);
        }

        /// <summary>
        /// Upload Serial
        /// </summary>
        /// <param name="uploadContentDto">UploadContentDto</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        [HttpPost]
        [Route("serial/content")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "OK")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorDto), description: "Not Found")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadSerial([FromForm] UploadContentDto uploadContentDto, CancellationToken cancellationToken)
        {
            var episodeExistEvent = new EpisodeExistEvent()
            {
                EpisodeId = uploadContentDto.ContentId
            };

            var result = await _clientEpisodeExist.GetResponse<EpisodeExistResponse>(episodeExistEvent, cancellationToken);

            if (!result.Message.IsExists)
                return StatusCode(404, new ErrorDto("Content not found", "404"));

            var path = await _minioService.UploadContent(uploadContentDto.File);

            var serialAddContentEvent = new SerialAddContentEvent()
            {
                EpisodeId = uploadContentDto.ContentId,
                Content = new Content()
                {
                    Quality = ContentQuality.FullHD,
                    Path = path
                }
            };

            await _publishEndpoint.Publish(serialAddContentEvent);

            var url = await _minioService.GetPresignedUrl(path);

            var videoProcessingEvent = new VideoProcessingEvent()
            {
                FileUrl = url,
                FileExtension = Path.GetExtension(uploadContentDto.File.FileName),
                ContentType = uploadContentDto.File.ContentType,
                DataType = DataTypeEnum.Serial,
                ContentId = uploadContentDto.ContentId,
                IsEpisode = false
            };

            videoProcessingEvent.Quality = ContentQuality.HD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            videoProcessingEvent.Quality = ContentQuality.SD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            return StatusCode(200, url);
        }

        /// <summary>
        /// Upload Amine
        /// </summary>
        /// <param name="uploadContentDto">UploadContentDto</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        [HttpPost]
        [Route("anime/content")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "OK")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorDto), description: "Not Found")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadAnime([FromForm] UploadContentDto uploadContentDto, CancellationToken cancellationToken)
        {
            if (uploadContentDto.IsEpisode)
            {
                var episodeExistEvent = new EpisodeExistEvent()
                {
                    EpisodeId = uploadContentDto.ContentId
                };

                var result = await _clientEpisodeExist.GetResponse<EpisodeExistResponse>(episodeExistEvent, cancellationToken);

                if (!result.Message.IsExists)
                    return StatusCode(404, new ErrorDto("Content not found", "404"));
            }
            else
            {
                var contentExistEvent = new ContentExistEvent()
                {
                    ContentId = uploadContentDto.ContentId
                };

                var result = await _clientContentExist.GetResponse<ContentExistResponse>(contentExistEvent, cancellationToken);

                if (!result.Message.IsExists)
                    return StatusCode(404, new ErrorDto("Content not found", "404"));
            }

            var path = await _minioService.UploadContent(uploadContentDto.File);

            var animeAddContentEvent = new AnimeAddContentEvent()
            {
                ContentId = uploadContentDto.ContentId,
                IsEpisode = uploadContentDto.IsEpisode,
                Content = new Content()
                {
                    Quality = ContentQuality.FullHD,
                    Path = path
                }
            };

            await _publishEndpoint.Publish(animeAddContentEvent);

            var url = await _minioService.GetPresignedUrl(path);

            var videoProcessingEvent = new VideoProcessingEvent()
            {
                FileUrl = url,
                FileExtension = Path.GetExtension(uploadContentDto.File.FileName),
                ContentType = uploadContentDto.File.ContentType,
                DataType = DataTypeEnum.Anime,
                ContentId = uploadContentDto.ContentId,
                IsEpisode = uploadContentDto.IsEpisode
            };

            videoProcessingEvent.Quality = ContentQuality.HD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            videoProcessingEvent.Quality = ContentQuality.SD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            return StatusCode(200, url);
        }
    }
}
