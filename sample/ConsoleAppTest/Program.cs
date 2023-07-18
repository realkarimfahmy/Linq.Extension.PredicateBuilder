using System.Globalization;
using Linq.Extension.PredicateBuilder;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var predicate = new PostViewComponentModel
            {
                Search = new List<Search>
                {
                    new Search
                    {
                        Field = new Field{ Value = "PublishedAt" },
                        Operator = new Operator { Value = OperatorComparer.Between},
                        Value = new Value{ value =  "07/08/2023" , value2 = "07/18/2023"}
                    },
                    new Search
                    {
                        Field = new Field{ Value = "Name" },
                        Operator = new Operator { Value = OperatorComparer.BeginsWith },
                        Value = new Value{ value = "p" }
                    },
                    new Search
                    {
                        Field = new Field{ Value = "Status" },
                        Operator = new Operator { Value = OperatorComparer.In },
                        Value = new Value{ value = "0,2" }
                    }
                },
                PageNumber = 1,
                PageSize = 20
                
            }; 
            var result = new Post().GetPosts().BuildQuery(predicate.PredicateBuilderFilterRule
                , new PredicateBuilderOptions() { CultureInfo = CultureInfo.CurrentCulture }).ToPaged(predicate);
        }
    }

    public enum Status
    {
        New,
        Pending,
        Completed
    }

    public class PostViewComponentModel : PredicateBuilderInvokerBase<Post>
    {
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
        public Status Status { get; set; }

        public IQueryable<Post> GetPosts()
        {
            var posts = new List<Post>
            {
                new Post {Id = 1, Name = "POST1" , Description = "post1 description", IsPublished = true, 
                    PublishedAt = DateTime.Now.AddMinutes(-3), TotalViews = 50, TotalComments = 4 , Status = 0},

                new Post {Id = 2, Name = "POST2" , Description = "post2 description", IsPublished = false,
                    PublishedAt = DateTime.Now.AddDays(-1), TotalViews = 10, TotalComments = 1 , Status = (Status)1},

                new Post {Id = 3, Name = "POST3" , Description = "post3 description", IsPublished = true,
                    PublishedAt = DateTime.Now.AddDays(-8), TotalViews = 120, TotalComments = 8, Status = (Status)2},

                new Post {Id = 4, Name = "POST4" , Description = "post4 description", IsPublished = true,
                    PublishedAt = DateTime.Now.AddDays(-1), TotalViews = 40, TotalComments = 5, Status = (Status)1},

                new Post {Id = 5, Name = "POST5" , Description = "post5 description", IsPublished = true,
                    PublishedAt = DateTime.Now, TotalViews = 0, TotalComments = 0 , Status = (Status)2},

                new Post {Id = 6, Name = "POST6" , Description = "post6 description", IsPublished = true,
                    PublishedAt = DateTime.Now.AddDays(-10), TotalViews = 150, TotalComments = 10, Status = (Status)2},

                new Post {Id = 7, Name = "POST7" , Description = "post7 description", IsPublished = false,
                    PublishedAt = DateTime.Now, TotalViews = 0, TotalComments = 0, Status = (Status)2},

                new Post {Id = 8, Name = "POST8" , Description = "post8 description", IsPublished = true,
                    PublishedAt = DateTime.Now.AddDays(-20), TotalViews = 250, TotalComments = 15, Status = 0},

            };

            return posts.AsQueryable();
        }
    }

}