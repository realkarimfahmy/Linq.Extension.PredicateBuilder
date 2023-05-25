using Linq.Extension.PredicateBuilder;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var searchFilters = new List<SearchFilter> {
                new SearchFilter{PropertyName = "Name", Operation = OperatorComparer.BeginsWith, Value = "post" },
                new SearchFilter{PropertyName = "IsPublished", Operation = OperatorComparer.Equal, Value = "true" },
                new SearchFilter{PropertyName = "TotalViews", Operation = OperatorComparer.GreaterOrEqual, Value = "150" },
            };

            var predicate = PredicateBuilder.Compile<Post>(searchFilters);

            var filteredPostsResult = new Post().GetPosts().Where(predicate).ToList();
        }
    }
    public class Post
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime PublishedAt { get; set; }
        public int TotalViews { get; set; }
        public int TotalComments { get; set; }

        public List<Post> GetPosts()
        {
            var posts = new List<Post>
            {
                new Post {Id = 1, Name = "post1" , Description = "post1 description", IsPublished = true, PublishedAt = DateTime.Now, TotalViews = 50, TotalComments = 4},
                new Post {Id = 2, Name = "post2" , Description = "post2 description", IsPublished = false, PublishedAt = DateTime.Now.AddDays(-3), TotalViews = 10, TotalComments = 1},
                new Post {Id = 3, Name = "post3" , Description = "post3 description", IsPublished = true, PublishedAt = DateTime.Now.AddDays(-8), TotalViews = 120, TotalComments = 8},
                new Post {Id = 4, Name = "post4" , Description = "post4 description", IsPublished = true, PublishedAt = DateTime.Now.AddDays(-1), TotalViews = 40, TotalComments = 5},
                new Post {Id = 5, Name = "post5" , Description = "post5 description", IsPublished = true, PublishedAt = DateTime.Now, TotalViews = 0, TotalComments = 0},
                new Post {Id = 6, Name = "post6" , Description = "post6 description", IsPublished = true, PublishedAt = DateTime.Now.AddDays(-10), TotalViews = 150, TotalComments = 10},
                new Post {Id = 7, Name = "post7" , Description = "post7 description", IsPublished = false, PublishedAt = DateTime.Now, TotalViews = 0, TotalComments = 0},
                new Post {Id = 8, Name = "post8" , Description = "post8 description", IsPublished = true, PublishedAt = DateTime.Now.AddDays(-20), TotalViews = 250, TotalComments = 15},

            };
            return posts;
        }
    }
}