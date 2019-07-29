using Crawler.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Crawler.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(MainPage model)
        {
            BlogContext bc = new BlogContext();

            //For Suport Https
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


            var filename =Path.GetFileName(model.Attachment.FileName);
            string str = "";
            
            /*
            Read file: 1.read bytes 2.encoding and get string
            */
            using (var reader=new System.IO.BinaryReader(model.Attachment.InputStream))
            {
                var content=reader.ReadBytes(model.Attachment.ContentLength);
                str= System.Text.Encoding.UTF8.GetString(content, 0, content.Length);
            }

            str=str.Trim();


            bc.URLInfos.RemoveRange(bc.URLInfos);
            bc.SaveChanges();

            //Seperate string lines
            string[] stringseperator = new string[] { "\r\n" };
            string[] lines = str.Split(stringseperator, StringSplitOptions.None);


            /*
            if there aren't any tuple in database equals url and don't read html(status=0) , add this to database 
            */
            foreach (var item in lines)
            {
                if (item != "")
                {
                    if (!bc.URLInfos.Any(t => t.Url == item && t.status==0))
                    {
                        bc.URLInfos.Add(new URLInfo
                        {
                            Url = item,
                            Depth = model.CrawlDepth,
                        });
                        bc.SaveChanges();
                    }
                }
            }

           //while there aren't any tuple with status=0
            while (bc.URLInfos.Any(t => t.status == 0))
            {
                //get first item with status=0
                var item = bc.URLInfos.FirstOrDefault(t => t.status == 0);

                Uri uriitem = new Uri(item.Url);

                //get html of url 
                var html=Utility.GetHtml(item.Url);
                HtmlDocument doc = new HtmlDocument();

                if (html != null)
                {
                    doc.LoadHtml(html);

                    if (doc != null)
                    {
                        //get html tags with <a href= ...
                        var docnode = doc.DocumentNode.SelectNodes(@"//a[@href]");
                        if (docnode != null)
                        {
                            foreach (HtmlNode link in docnode)
                            {
                                //get this tags href
                                HtmlAttribute att = link.Attributes["href"];

                                if (att == null) continue;
                                string href = att.Value;

                                if (href.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase)) continue;
                                // ignore javascript on buttons using a tags

                                //relative:sepahan - absolute:sepahan with domain
                                //this url is relative or absolute?
                                Uri urlNext = new Uri(href, UriKind.RelativeOrAbsolute);

                                // Make it absolute if it's relative
                                //create absolute url if its relative
                                if (!urlNext.IsAbsoluteUri)
                                {
                                    urlNext = new Uri(uriitem, urlNext);
                                }

                                // if there aren't any this url in database 
                                var urlnextstr = urlNext.ToString();
                                if (!bc.URLInfos.Any(t => t.Url == urlnextstr))
                                {
                                    //instagram.com isn't base of varzesh3.com
                                    if (uriitem.IsBaseOf(urlNext) && item.Depth - 1 >= 0)
                                    {
                                        bc.URLInfos.Add(new URLInfo
                                        {
                                            Url = urlNext.ToString(),
                                            Depth = item.Depth - 1,
                                            status = 0,
                                        });
                                        bc.SaveChanges();
                                    }

                                }
                            }
                        }
                        //create directory for this link that read its html completely
                        Directory.CreateDirectory(Server.MapPath(@"~\files\" + item.id));

                        long i = 0;
                        //get scripts <script src=...
                        var scriptnode = doc.DocumentNode.SelectNodes(@"//script[@src]");
                        if (scriptnode != null)
                        {
                            foreach (HtmlNode script in scriptnode)
                            {
                                //get src of this script
                                HtmlAttribute att = script.Attributes["src"];

                                if (att == null) continue;
                                string scrp = att.Value;

                                //get all script text with request
                                var getscript = Utility.GetHtml(scrp);

                                //save script text in .js file
                                System.IO.File.WriteAllText(Server.MapPath(@"~\files\" + item.id + "\\" + ++i + ".js"), getscript);

                                //reference this .js file to script src
                                script.Attributes["src"].Value = i + ".js";
                            }
                        }

                        //get style <link rel=...
                        var stylenode = doc.DocumentNode.SelectNodes(@"//link[@href]");
                        if (stylenode != null)
                        {
                            foreach (HtmlNode link in stylenode)
                            {
                                //get rel of this link
                                HtmlAttribute att = link.Attributes["rel"];
                                if (att == null) continue;

                                //we want stylesheet attribute
                                if (att.Value != "stylesheet") continue;

                                //get href value
                                att = link.Attributes["href"];
                                if (att == null) continue;
                                string scrp = att.Value;

                                //get text of style
                                var getscript = Utility.GetHtml(scrp);

                                //save style in .css file
                                System.IO.File.WriteAllText(Server.MapPath(@"~\files\" + item.id + "\\" + ++i + ".css"), getscript);
                                link.Attributes["href"].Value = i + ".css";
                            }
                        }


                        var imgnode = doc.DocumentNode.SelectNodes(@"//img[@src]");
                        if (imgnode != null)
                        {
                            foreach (HtmlNode img in imgnode)
                            {
                                //get rel of this link
                                HtmlAttribute att = img.Attributes["src"];
                                if (att == null) continue;
                                
                                //get src value
                                string scrp = att.Value;

                                //get text of style
                                Utility.GetImage(scrp, Server.MapPath(@"~\files\" + item.id + "\\" + ++i + ".jpg"));

                                img.Attributes["src"].Value = i + ".jpg";
                            }
                        }



                        //save html file
                        doc.Save(Server.MapPath(@"~\files\" + item.id + "\\" + item.id + ".html"));

                        //read this url comletely
                        bc.URLInfos.FirstOrDefault(t => t.id == item.id).status = 1;
                        bc.SaveChanges();
                    }
                }  
            }
            
            return View();
        }


        public ActionResult Search()
        {
          
            return View();
        }

        [HttpPost]
        public ActionResult Search(Search model)
        {
            BlogContext bc = new BlogContext();
            if (bc.URLInfos.Any(t => t.Url == model.GetURL))
            {
                var url = bc.URLInfos.FirstOrDefault(t => t.Url == model.GetURL);
                ViewBag.Url = @"\files\" + url.id + "\\"+url.id+".html";
                return View();
            }
            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}