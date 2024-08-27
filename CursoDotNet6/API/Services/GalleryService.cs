using API.Entities;
using API.Entities.ViewModels;
using API.Infra;
using AutoMapper;

namespace API.Services
{
    public class GalleryService
    {
        private readonly IMapper _mapper;

        private readonly IMongoRepository<Gallery> _gallery;
        private readonly ICacheService _cacheService;
        private readonly string keyForCache = "gallery";

        public GalleryService( IMongoRepository<Gallery> gallery, IMapper mapper, ICacheService cacheService )
        {
            _mapper = mapper;
            _gallery = gallery;
            _cacheService = cacheService;
        }

        public Result<GalleryViewModel> Get( int page, int qtd )
        {
            var keyCache = $"{keyForCache}/{page}/{qtd}";
            var gallery = _cacheService.Get<Result<GalleryViewModel>>( keyCache );

            if ( gallery is null )
            {
                gallery = _mapper.Map<Result<GalleryViewModel>>( _gallery.Get( page, qtd ) );
                _cacheService.Set( keyCache, gallery );
            }

            return gallery;
        }

        public GalleryViewModel Get( string id )
        {
            var cacheKey = $"{keyForCache}/{id}";

            var galery = _cacheService.Get<GalleryViewModel>( cacheKey );

            if ( galery is null )
            {
                galery = _mapper.Map<GalleryViewModel>( _gallery.Get( id ) );
                _cacheService.Set( cacheKey, galery );
            }

            return galery;
        }

        public GalleryViewModel GetBySlug( string slug )
        {
            var cacheKey = $"{keyForCache}/{slug}";

            var galery = _cacheService.Get<GalleryViewModel>( cacheKey );

            if ( galery is null )
            {
                galery = _mapper.Map<GalleryViewModel>( _gallery.GetBySlug( slug ) );
                _cacheService.Set( cacheKey, galery );
            }

            return galery;
        }

        public GalleryViewModel Create( GalleryViewModel gallery )
        {
            var entity = new Gallery( gallery.Title, gallery.Legend, gallery.Author, gallery.Tags, gallery.Status, gallery.GalleryImages, gallery.Thumb );
            entity.PublishDate = DateTime.Now;
            _gallery.Create( entity );

            var cacheKey = $"{keyForCache}/{entity.Slug}";
            _cacheService.Set( cacheKey, entity );

            return Get( entity.Id );
        }

        public void Update( string id, GalleryViewModel galleryIn )
        {
            var cacheKey = $"{keyForCache}/{id}";
            _gallery.Update( id, _mapper.Map<Gallery>( galleryIn ) );

            _cacheService.Remove( cacheKey );
            _cacheService.Set( cacheKey, galleryIn );
        }

        public void Remove( string id )
        {
            var cacheKey = $"{keyForCache}/{id}";
            _gallery.Remove( cacheKey );

            var gallery = Get( id );
            cacheKey = $"{keyForCache}/{gallery.Slug}";
            _gallery.Remove( cacheKey );

            _cacheService.Remove( cacheKey );
        }
    }
}