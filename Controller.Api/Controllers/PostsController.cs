using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiPlayground.Data.Context;
using ApiPlayground.Data.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Controller.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PostsController> _logger;

        public PostsController(AppDbContext context, IMemoryCache cache, ILogger<PostsController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

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


        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
