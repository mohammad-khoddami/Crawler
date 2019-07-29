using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Crawler.Models
{
    public class MainPage
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="Required Field")]
        [Display(Name ="Crawl Depth")]
        [Range(1,500,ErrorMessage ="range between 1 - 500")]
        public int CrawlDepth { get; set; }

        public HttpPostedFileBase Attachment { get; set; }
    }

    public class Search
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Required Field")]
        [Display(Name = "Search")]
        public string GetURL { get; set; }

    }


}