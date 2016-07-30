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

            Console.WriteLine("Выберете нужное: ");
            Console.WriteLine("1 - все тендеры");
            Console.WriteLine("2 - активные тендеры");
            int choice = Convert.ToInt32(Console.ReadLine());
            List<string> tendLinks;

            if (choice == 1)
            {
                Repository rep = new Repository();
                DataParser parser = new DataParser();
                string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
                //File.WriteAllText("test.txt", txt);
                HtmlDocument doc = rep.CreateDoc(txt);
                tendLinks = parser.CollectLinks(doc);
              
                foreach (string link in tendLinks)
                {
                    Console.WriteLine("[INFO] >>> Get tender: {0}", "http://omk.zakupim.ru" + link);
                    string tend = rep.LoadPage("http://omk.zakupim.ru" + link);
                    HtmlDocument tendDoc = rep.CreateDoc(tend);
                    string jsonModel = parser.CreateNotModel(parser.ParseTender(tendDoc,link));
                    rep.SendToRedis(jsonModel);
                }
            }
            while (true)
            {
                Repository rep = new Repository();
                DataParser parser = new DataParser();
                string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
                HtmlDocument doc = rep.CreateDoc(txt);
                tendLinks = parser.CollectActiveLinks(doc);
               
                foreach (string link in tendLinks)
                {
                    Console.WriteLine("[INFO] >>> Get tender: {0}", "http://omk.zakupim.ru" + link);
                    string tend = rep.LoadPage("http://omk.zakupim.ru" + link);
                    HtmlDocument tendDoc = rep.CreateDoc(tend);
                    string jsonModel = parser.CreateNotModel(parser.ParseTender(tendDoc,  link));
                    rep.SendToRedis(jsonModel);
                }
                Thread.Sleep(3600000);
                Console.ReadLine();
            }

            //для проверки
            //Repository rep = new Repository();
            //DataParser parser = new DataParser();
            //string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
            //HtmlDocument doc = rep.CreateDoc(txt);

            //string tend = rep.LoadPage("http://omk.zakupim.ru/view/23960");
            //HtmlDocument tendDoc = rep.CreateDoc(tend);
            //parser.ParseTender(tendDoc, "/view/23960");
            //Debug.WriteLine(parser.CreateNotModel(parser.ParseTender(tendDoc, "/view/23960")));
        }
    }
}
