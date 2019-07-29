using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Crawler
{
    public static class Utility
    {
        public static string GetHtml(string urladdress)
        {
            var htmlcode = "";

            try
            {
                using (var wc = new CustomWebClient())
                {
                    htmlcode = wc.DownloadString(urladdress);
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return htmlcode;
        }

        public static void GetImage(string urladdress,string filename)
        {
            try {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(urladdress, filename);
                }
            }
            catch(Exception e)
            {
            }
        }

    }
    public class CustomWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            if (request != null)
            {
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                this.Encoding = Encoding.UTF8;
            }
            return request;
        }
    }
}
