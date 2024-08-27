using API.Infra;
using API.Mappers;
using API.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers( );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer( );
builder.Services.AddSwaggerGen( c =>
{
    c.AddSecurityDefinition( "bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    } );
    c.AddSecurityRequirement( new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" }
            },
            new string[] {}
        }
    } );
} );

#region [Database]

builder.Services.Configure<DatabaseSettings>( builder.Configuration.GetSection( nameof( DatabaseSettings ) ) );
builder.Services.AddSingleton<IDatabaseSettings>( sp => sp.GetRequiredService<IOptions<DatabaseSettings>>( ).Value );

#endregion [Database]

#region [Cache]

builder.Services.AddStackExchangeRedisCache( options =>
{
    options.Configuration = builder.Configuration.GetSection( "Redis:ConnectionString" ).Value;
} );

#endregion [Cache]

#region [Healthcheck]

builder.Services.AddHealthChecks( )
    .AddRedis( builder.Configuration.GetSection( "Redis:ConnectionString" ).Value, tags: new string[ ] { "db", "data" } )
    .AddMongoDb( builder.Configuration.GetSection( "DatabaseSettings:ConnectionString" ).Value + "/" + builder.Configuration.GetSection( "DatabaseSettings:db_portal" ).Value, name: "mongodb", tags: new string[ ] { "db", "data" } );

builder.Services.AddHealthChecksUI( opt =>
{
    opt.SetEvaluationTimeInSeconds( 15 ); //time in seconds between check
    opt.MaximumHistoryEntriesPerEndpoint( 60 ); //maximum history of checks
    opt.SetApiMaxActiveRequests( 1 ); //api requests concurrency

    opt.AddHealthCheckEndpoint( "default api", "/health" ); //map health check api
} ).AddInMemoryStorage( );

#endregion [Healthcheck]

#region [DI]

builder.Services.AddSingleton( typeof( IMongoRepository<> ), typeof( MongoRepository<> ) );
builder.Services.AddSingleton<NewsService>( );
builder.Services.AddSingleton<VideoService>( );
builder.Services.AddTransient<UploadService>( );
builder.Services.AddTransient<GalleryService>( );

builder.Services.AddSingleton<ICacheService, CacheRedisService>( );

#endregion [DI]

#region [AutoMapper]

builder.Services.AddAutoMapper( typeof( EntityToViewModelMapping ), typeof( ViewModelToEntityMapping ) );

#endregion [AutoMapper]

#region [Cors]

builder.Services.AddCors( );

#endregion [Cors]

#region [JWT]

builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
    .AddJwtBearer( options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey( Encoding.ASCII
                .GetBytes( builder.Configuration.GetSection( "tokenManagement:secret" ).Value ) ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    } );

#endregion [JWT]

var app = builder.Build( );

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment( ) )
{
    app.UseSwagger( );
    app.UseSwaggerUI( );
}

#region [Healthcheck]

app.UseHealthChecks( "/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
} ).UseHealthChecksUI( h => h.UIPath = "/healthui" );

#endregion [Healthcheck]

#region [Cors]

app.UseCors( c =>
{
    c.AllowAnyHeader( );
    c.AllowAnyMethod( );
    c.AllowAnyOrigin( );
} );

#endregion [Cors]

#region [StaticFiles]

app.UseStaticFiles( new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine( builder.Environment.ContentRootPath, "Medias" ) ),
    RequestPath = "/medias"
} );

#endregion [StaticFiles]

app.UseAuthentication( );
app.UseAuthorization( );

app.MapControllers( );

app.Run( );