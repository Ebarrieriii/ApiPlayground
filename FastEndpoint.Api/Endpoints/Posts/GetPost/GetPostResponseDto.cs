using ApiPlayground.Data.Entities;

namespace FastEndpoints.Api.Endpoints.Posts.GetPost
{
    public class GetPostResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
    }
}
