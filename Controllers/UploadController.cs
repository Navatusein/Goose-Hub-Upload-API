using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
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
    [Route("v1/upload")]
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

            await SendTranscodeTaskAsync(uploadContentDto, DataTypeEnum.Movie);

            return StatusCode(200);
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

            await SendTranscodeTaskAsync(uploadContentDto, DataTypeEnum.Serial);

            return StatusCode(200);
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

            await SendTranscodeTaskAsync(uploadContentDto, DataTypeEnum.Anime);

            return StatusCode(200);
        }

        private async Task SendTranscodeTaskAsync(UploadContentDto uploadContent, DataTypeEnum dataType)
        {
            var fileName = Guid.NewGuid().ToString();
            var path = await _minioService.UploadContentAsync(uploadContent.File, fileName);
            var url = await _minioService.GetUrlAsync(path);

            var videoProcessingEvent = new VideoProcessingEvent()
            {
                FileUrl = url,
                DataType = dataType,
                FileName = fileName,
                ContentId = uploadContent.ContentId,
                IsEpisode = uploadContent.IsEpisode
            };

            videoProcessingEvent.Quality = ContentQuality.FullHD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            videoProcessingEvent.Quality = ContentQuality.HD;
            await _publishEndpoint.Publish(videoProcessingEvent);

            videoProcessingEvent.Quality = ContentQuality.SD;
            await _publishEndpoint.Publish(videoProcessingEvent);
        }
    }
}
