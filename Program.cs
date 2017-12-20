using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace BookStarts
{
    class Program
    {


        static void Main(string[] args)
        {
            var extBooks = readeFromFile("Books.json");
            if (extBooks == null)
            {
                extBooks = new List<Book>();
            }
            var datas = new List<Book>();
            var random = new Random(DateTime.Now.Millisecond);
            for (int i = 10883; i >= 8083; i--)
            {
                var url = string.Format("http://www.zxcs8.com/post/{0}", i);
                Book book = extBooks.Find(b => b.Url == url);
                if (book == null)
                {
                    book = getBook(url);
                }

                var urls = string.Format("http://www.zxcs8.com/content/plugins/cgz_xinqing/cgz_xinqing_action.php?action=mood&id={0}&typee=mood1&m={1}", i, random.NextDouble());
                var docs = HttpHelp.getDocument(urls);
                if (!docs.StartsWith("Error:"))
                {
                    book.Starts = docs;
                }

                datas.Add(book);

                if (datas.Count % 10 == 0)
                {


                    extBooks.AddRange(datas);
                    string jsw = JsonConvert.SerializeObject(extBooks);

                    writeToFile(jsw, "Books.json");
                    datas.Clear();
                    Console.WriteLine("Writed!");
                }

                Thread.Sleep(300);
            }

            string js = JsonConvert.SerializeObject(extBooks);

            writeToFile(js, "Books.json");
            Console.WriteLine("Ok!");
            Console.ReadLine();
        }

        private static Book getBook(string url)
        {
            var doc = HttpHelp.getDocument(url);
            if (doc.StartsWith("Error:"))
            {
                Thread.Sleep(200);
            }

            Regex reg = new Regex(@"<h1>(.|\n)*?</h1>");

            string[] taas = reg.Match(doc).Value.Replace("<h1>", "").Replace("</h1>", "").Split("作者：");

            reg = new Regex(@"<p\sclass=""date"">(.|\n)*?</p>");

            string kwp = reg.Match(doc).Value;

            reg = new Regex(@"<a(.|\n)*?</a>");

            var kwl = new List<string>();

            foreach (Match item in reg.Matches(kwp))
            {
                kwl.Add(Helper.GetTitleContent(item.Value, "a"));
            }
            if (kwl.Count > 0)
            {
                kwl.RemoveAt(0);
            }

            var keywords = string.Join(',', kwl);


            Book book = new Book() { Url = url, KeyWords = keywords };
            book.Title = taas[0];
            if (taas.Length > 1)
            {
                book.Author = taas[1];
            }
            return book;
        }

        private static void writeToFile(string js, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
            {
                using (StreamWriter stream = new StreamWriter(fs))
                {
                    stream.Write(js);
                }
            }
        }

        private static List<Book> readeFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return new List<Book>();
            }
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    return JsonConvert.DeserializeObject<List<Book>>(sr.ReadToEnd());
                }
            }
        }

        private static string getTestDoc()
        {
            using (FileStream fs = new FileStream("test.html", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
    /*
     <p>
【TXT大小】：11.8 MB<br/>
【内容简介】：<br/>　　诸天万界，百家林立！
<br/>　　叛逆少年化身史上最强弟子，修道家降妖除魔，练兵家神兵战阵，学儒道浩然正气；
<br/>　　一符降至强鬼神，一刀斩山河日月，一阵困万古龙神，运筹帷幄化万千兵马；
<br/>　　诸天神圣尽在脚下，万恶俯首，众神至尊！
<br/><br/></p>
     */
    public class Book
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string KeyWords { get; set; }

        public string Starts { get; set; }
    }

    public static class HttpHelp
    {
        public static string getDocument(string url)
        {
            string document = string.Empty;
            try
            {
                var request = HttpWebRequest.CreateHttp(url);
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream s = response.GetResponseStream())
                    {
                        using (StreamReader fs = new StreamReader(s))
                        {
                            document = fs.ReadToEnd();
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                document = "Error:" + exc.Message;
            }
            return document;
        }
    }
}
