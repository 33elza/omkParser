using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;

namespace omkParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Repository rep = new Repository();
            DataParser parser = new DataParser();
            string txt = rep.LoadPage("http://omk.zakupim.ru/view_table_jx?sort=&up=&flt=8&flt_from=2008-1-1&flt_from_chk=true&flt_to=2016-7-27&flt_to_chk=false&09261455562063892");
            HtmlDocument doc = rep.CreateDoc(txt);
            List<string> tendLinks = parser.CollectLinks(doc);
            foreach (string link in tendLinks)
            {
                string tend = rep.LoadPage("http://omk.zakupim.ru" + link);
                HtmlDocument tendDoc = rep.CreateDoc(tend);
                string jsonModel = parser.CreateNotModel( parser.ParseTender(tendDoc, "http://omk.zakupim.ru" + link));
                rep.SendToRedis(jsonModel);
            }

            //string tend = rep.LoadPage("http://omk.zakupim.ru/view/23960");
            //HtmlDocument tendDoc = rep.CreateDoc(tend);
            //parser.ParseTender(tendDoc, "http://omk.zakupim.ru/view/23960");
            //Debug.WriteLine( parser.CreateNotModel(parser.ParseTender(tendDoc, "http://omk.zakupim.ru/view/23960")) );
        }
    }
}
