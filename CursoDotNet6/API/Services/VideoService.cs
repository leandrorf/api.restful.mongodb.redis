using API.Entities;
using API.Entities.ViewModels;
using API.Infra;
using AutoMapper;

namespace API.Services
{
    public class VideoService
    {
        private readonly IMapper _mapper;

        private readonly IMongoRepository<Video> _video;
        private readonly ICacheService _cacheService;
        private readonly string keyForCache = "video";

        public VideoService( IMongoRepository<Video> video, IMapper mapper, ICacheService cacheService )
        {
            _mapper = mapper;
            _video = video;
            _cacheService = cacheService;
        }

        public Result<VideoViewModel> Get( int page, int qtd )
        {
            var keyCache = $"{keyForCache}/{page}/{qtd}";
            var news = _cacheService.Get<Result<VideoViewModel>>( keyCache );

            if ( news is null )
            {
                news = _mapper.Map<Result<VideoViewModel>>( _video.Get( page, qtd ) );
                _cacheService.Set( keyCache, news );
            }

            return news;
        }

        public VideoViewModel Get( string id )
        {
            var cacheKey = $"{keyForCache}/{id}";

            var video = _cacheService.Get<VideoViewModel>( cacheKey );

            if ( video is null )
            {
                video = _mapper.Map<VideoViewModel>( _video.Get( id ) );
                _cacheService.Set( cacheKey, video );
            }

            return video;
        }

        public VideoViewModel GetBySlug( string slug )
        {
            var cacheKey = $"{keyForCache}/{slug}";

            var video = _cacheService.Get<VideoViewModel>( cacheKey );

            if ( video is null )
            {
                video = _mapper.Map<VideoViewModel>( _video.GetBySlug( slug ) );
                _cacheService.Set( cacheKey, video );
            }

            return video;
        }

        public VideoViewModel Create( VideoViewModel video )
        {
            var entity = new Video( video.Hat, video.Title, video.Author, video.Thumbnail, video.UrlVideo, video.Status );
            entity.PublishDate = DateTime.Now;
            _video.Create( entity );

            var cacheKey = $"{keyForCache}/{entity.Slug}";
            _cacheService.Set( cacheKey, entity );

            return Get( entity.Id );
        }

        public void Update( string id, VideoViewModel video )
        {
            var cacheKey = $"{keyForCache}/{id}";
            _video.Update( id, _mapper.Map<Video>( video ) );

            _cacheService.Remove( cacheKey );
            _cacheService.Set( cacheKey, video );
        }

        public void Remove( string id )
        {
            var cacheKey = $"{keyForCache}/{id}";
            _video.Remove( cacheKey );

            var gallery = Get( id );
            cacheKey = $"{keyForCache}/{gallery.Slug}";
            _video.Remove( cacheKey );

            _cacheService.Remove( cacheKey );
        }
    }
}