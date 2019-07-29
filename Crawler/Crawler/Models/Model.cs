using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crawler.Models
{
    public class BlogContext : DbContext
    {
        public BlogContext():base("DefaultConnection") { }

        public static BlogContext Create()
        {
            return new BlogContext();
        }
        public DbSet<URLInfo> URLInfos { get; set; }
    }

    public class URLInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        
        public string Url { get; set; }

        public int Depth { get; set; }

        public int status { get; set; }
    }



}