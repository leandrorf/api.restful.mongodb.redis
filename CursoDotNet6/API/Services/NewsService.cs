using API.Entities;
using API.Entities.ViewModels;
using API.Infra;
using AutoMapper;

namespace API.Services
{
    public class NewsService
    {
        private readonly IMapper _mapper;

        private readonly IMongoRepository<News> _news;
        private readonly ICacheService _cacheService;
        private readonly string keyForCache = "news";

        public NewsService( IMongoRepository<News> news, IMapper mapper, ICacheService cacheService )
        {
            _mapper = mapper;
            _news = news;
            _cacheService = cacheService;
        }

        public Result<NewsViewModel> Get( int page, int qtd )
        {
            var keyCache = $"{keyForCache}/{page}/{qtd}";
            var news = _cacheService.Get<Result<NewsViewModel>>( keyCache );

            if ( news is null )
            {
                news = _mapper.Map<Result<NewsViewModel>>( _news.Get( page, qtd ) );
                _cacheService.Set( keyCache, news );
            }

            return news;
        }

        public NewsViewModel Get( string id )
        {
            var cacheKey = $"{keyForCache}/{id}";

            var news = _cacheService.Get<NewsViewModel>( cacheKey );

            if ( news is null )
            {
                news = _mapper.Map<NewsViewModel>( _news.Get( id ) );
                _cacheService.Set( cacheKey, news );
            }

            return news;
        }

        public NewsViewModel GetBySlug( string slug )
        {
            var cacheKey = $"{keyForCache}/{slug}";

            var news = _cacheService.Get<NewsViewModel>( cacheKey );

            if ( news is null )
            {
                news = _mapper.Map<NewsViewModel>( _news.GetBySlug( slug ) );
                _cacheService.Set( cacheKey, news );
            }

            return news;
        }

        public NewsViewModel Create( NewsViewModel news )
        {
            var entity = new News( news.Hat, news.Title, news.Text, news.Author, news.Img, news.Status )
            {
                PublishDate = DateTime.Now
            };

            _news.Create( entity );

            var cacheKey = $"{keyForCache}/{entity.Slug}";
            _cacheService.Set( cacheKey, entity );

            return Get( entity.Id );
        }

        public void Update( string id, NewsViewModel galleryIn )
        {
            var cacheKey = $"{keyForCache}/{id}";
            _news.Update( id, _mapper.Map<News>( galleryIn ) );

            _cacheService.Remove( cacheKey );
            _cacheService.Set( cacheKey, galleryIn );
        }

        public void Remove( string id )
        {
            var gallery = Get( id );

            var cacheKey = $"{keyForCache}/{id}";
            _cacheService.Remove( cacheKey );

            cacheKey = $"{keyForCache}/{gallery.Slug}";
            _cacheService.Remove( cacheKey );

            _news.Remove( id );
        }
    }
}