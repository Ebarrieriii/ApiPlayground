# FastEndpoints vs Controller API: Performance Testing

This repo compares the performance of traditional Controller APIs and FastEndpoints in .NET through load testing, benchmarks, and code examples.

## ⚡ TL;DR Summary

- **FastEndpoints** is 10-20% faster than **Controller APIs** for smaller datasets (100, 500 records).
- For larger datasets (1000, 5000 records), the performance gap narrows, but **FastEndpoints** remains generally faster.
- **FastEndpoints** is more consistent in response times, especially when caching is applied.

## Performance Breakdown

### 1. For Smaller Datasets (100, 500 records)
- **FastEndpoints** consistently outperforms **Controller APIs** by 10-20% in average and peak response times.

### 2. For Larger Datasets (1000, 5000 records)
- **FastEndpoints** remains faster but with a smaller margin.
- **Controller APIs** may perform slightly better with **AsNoTracking** for 1000 records, but **FastEndpoints** is more consistent.

### 3. Caching and AsNoTracking
- With caching, both APIs become significantly faster, but **FastEndpoints** stays marginally faster.

## Key Takeaways
- **FastEndpoints** provides better performance for smaller datasets and more consistency with larger ones.
- **Controller APIs** can outperform with specific optimizations like **AsNoTracking**, but **FastEndpoints** generally holds the edge.

## Conclusion
If you're paginating large datasets, **FastEndpoints** is typically the best choice, especially with caching and optimized queries. The choice of API type (Controller, FastEndpoints, or Minimal APIs) depends on personal preference and your application's needs.

## Next Steps:
- PostgreSQL
- Dapper
- Minimal APIs

## Endpoint Implementations

The same logic is implemented for both **FastEndpoints** and **Controller APIs** for a direct comparison.
### FastEndpoints Setup
```csharp
        public override async Task HandleAsync(GetPostRequestDto req, CancellationToken ct)
        {
            var posts = await context.Posts
                .Skip((req.page.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response = posts;
        }
```

### Controller API Setup
```csharp
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] int pageSize,[FromQuery] int page = 1)
        {
            var posts = await _context.Posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts;
        }
```

## Local Benchmarking

Requests sent via Postman, averaged over 10 runs per row.

| Records | 🟣 FastEndpoints (ms) | 🔵 Controller API (ms) | 🏁 Winner |
| ------- | --------------------- | ---------------------- | -------- |
| 100     | 83.2                  | 93.4                   | ✅ FastEndpoints |
| 500     | 85.3                  | 105.6                  | ✅ FastEndpoints |
| 1000    | 113.9                 | 103.3                  | ✅ Controller |
| 5000    | 145.1                 | 153.9                  | ✅ FastEndpoints |

## ⚙️ Load Test Setup

- Ran using **k6**
- 2-minute duration per test
- 100 virtual users hitting both endpoints

### Key Observations
- **AsNoTracking()**: Mixed results
  - Boosted performance for **Controller API**
  - Slowed **FastEndpoints** with larger payloads
- **Caching**: Significantly improved performance, with **FastEndpoints** still leading.

## Cached vs Non-Cached Performance
### Added Caching to FastEndpoints
```csharp
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
```
### Added Caching to Controller
```csharp
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] int pageSize,[FromQuery] int page = 1)
        {
            var cacheKey = $"posts_page_{page}_size_{pageSize}";
            bool cacheHit = true;

            if (!_cache.TryGetValue(cacheKey, out List<Post> posts))
            {
                cacheHit = false;
                posts = await _context.Posts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, posts, cacheEntryOptions);
            }

            _logger.LogDebug($"[Cache {(cacheHit ? "HIT" : "MISS")}] - {cacheKey}");

            return posts;
        }
```

| Records | Controller No Cache (ms) | Controller Cached (ms) | % Faster | FastEndpoints No Cache (ms) | FastEndpoints Cached (ms) | % Faster |
| ------- | ------------------------ | ----------------------- | -------- | -------------------------- | ------------------------- | -------- |
| 100     | 159.11                   | 3.92                    | 97.5%    | 116.65                     | 3.30                      | 97.2%    |
| 500     | 630.06                   | 15.37                   | 97.6%    | 614.90                     | 11.71                     | 98.1%    |
| 1000    | 945.86                   | 325.49                  | 65.6%    | 1203.53                    | 300.18                    | 75.1%    |
| 5000    | 4242.65                  | 1654.80                 | 61.0%    | 6036.33                    | 1444.51                   | 76.1%    |

## Final Thoughts

- **AsNoTracking()**: Worth testing per use case.
- **In-memory caching**: Has the biggest impact on performance. Make it a priority for any high-performance API.
- **FastEndpoints** generally performs better, but both APIs can be used to build a high-performing API suite.
