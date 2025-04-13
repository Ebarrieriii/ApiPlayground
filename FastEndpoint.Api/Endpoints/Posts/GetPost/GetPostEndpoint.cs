using ApiPlayground.Data.Context;
using ApiPlayground.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpoints.Api.Endpoints.Posts.GetPost
{
    public class GetPostEndpoint
        (
            AppDbContext context,
            IMemoryCache cache,
            ILogger<GetPostEndpoint> log
        )
        : Endpoint<GetPostRequestDto, List<Post>>
    {
        public override void Configure()
        {
            Get("/posts");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetPostRequestDto req, CancellationToken ct)
        {
            var cacheKey = $"posts_page_{req.page}_size_{req.pageSize}";
            bool cacheHit = true;

            if (!cache.TryGetValue(cacheKey, out List<Post> posts))
            {
                cacheHit = false;

                posts = await context.Posts
                    .Skip((req.page - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                cache.Set(cacheKey, posts, cacheEntryOptions);
            }

            log.LogDebug($"[Cache {(cacheHit ? "HIT" : "MISS")}] - {cacheKey}");

            Response = posts;
        }
    }
}
