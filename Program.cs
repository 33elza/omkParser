using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace omkParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        
            List<string> tendLinks;

                Repository rep = new Repository();
                DataParser parser = new DataParser();
                string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
                HtmlDocument doc = rep.CreateDoc(txt);
                tendLinks = parser.CollectActiveLinks(doc);
                rep.WriteFile(tendLinks.First());

                foreach (string link in tendLinks)
                {
                    Console.WriteLine("[INFO] >>> Get tender: {0}", "http://omk.zakupim.ru" + link);
                    string tend = rep.LoadPage("http://omk.zakupim.ru" + link);
                    HtmlDocument tendDoc = rep.CreateDoc(tend);
                    string jsonModel = parser.CreateNotModel(parser.ParseTender(tendDoc, link));
                     rep.SendToRedis(jsonModel);
                }

                while (true)
                {
                                      
                    string lasttender = rep.ReadFile().Substring(rep.ReadFile().LastIndexOf("/") + 1);

                    foreach (string link in tendLinks)
                    {
                        if (lasttender!=null)
                        {
                            if (Convert.ToInt32(link.Substring(link.LastIndexOf("/") + 1)) > Convert.ToInt32(lasttender))
                            {
                                Console.WriteLine("[INFO] >>> Get tender: {0}", "http://omk.zakupim.ru" + link);
                                string tend = rep.LoadPage("http://omk.zakupim.ru" + link);
                                HtmlDocument tendDoc = rep.CreateDoc(tend);
                                string jsonModel = parser.CreateNotModel(parser.ParseTender(tendDoc, link));
                                rep.SendToRedis(jsonModel);
                                rep.WriteFile(link);
                            }
                        }
                        else
                        {
                            rep.WriteFile("/view/23960");
                        }
                    }
                   
                    Thread.Sleep(3600000);
                    Console.ReadLine();
                }
            }
            
            //для проверки
            //Repository rep = new Repository();
            //DataParser parser = new DataParser();
            //string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
            //HtmlDocument doc = rep.CreateDoc(txt);

            //string tend = rep.LoadPage("http://omk.zakupim.ru/view/23924");
            //HtmlDocument tendDoc = rep.CreateDoc(tend);
            //parser.ParseTender(tendDoc, "/view/23960");
            //Debug.WriteLine(parser.CreateNotModel(parser.ParseTender(tendDoc, "/view/23032")));
        
    }
}
