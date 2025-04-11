using ApiPlayground.Data.Context;
using ApiPlayground.Data.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FakeDataGenerator;

public class Job
    (
        AppDbContext context,
        ILogger<Job> log
    )
{
    public void Main()
    {
        try
        {
            var blogFaker = new Faker<Blog>()
                .RuleFor(b => b.Name, f => f.Company.CompanyName())
                .RuleFor(b => b.Posts, f =>
                {
                    var postFaker = new Faker<Post>()
                        .RuleFor(p => p.Title, f => f.Lorem.Sentence())
                        .RuleFor(p => p.Content, f => f.Lorem.Paragraph());

                    return postFaker.Generate(f.Random.Int(5, 10));
                });

            var blogs = blogFaker.Generate(1000);

            context.Blogs.AddRange(blogs);
            context.SaveChanges();

            log.LogInformation($"Successfully saved {blogs.Count} blogs with posts.");
        }
        catch (Exception ex)
        {
            log.LogError($"Couldn't Save Messages: {ex}", ex);
            throw;
        }
    }
}
