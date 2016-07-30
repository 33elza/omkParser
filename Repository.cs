using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client;

namespace omkParser
{
    class Repository
    {
        public string LoadPage(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.Default);
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            resp.Close();
            reader.Close();
            byte[] stringBytes = Encoding.Default.GetBytes(output.ToString());
            var outputEncode = Encoding.GetEncoding("windows-1251").GetString(stringBytes);

            //Debug.WriteLine(output);
            return outputEncode;
        }

        public HtmlDocument CreateDoc(string text)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(text);

           // Debug.WriteLine(doc.DocumentNode.OuterHtml);
            return doc;
        }

        public void SendToRedis(string message)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "123456";
            factory.Protocol = Protocols.DefaultProtocol;
            factory.HostName = "10.0.1.77";
            factory.Port = 5672;
            IConnection connection = factory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                //channel.QueueDeclare(queue: ConfigurationManager.AppSettings["qeueuname"],
                //                     durable: false,
                //                     exclusive: false,
                //                     autoDelete: false,
                //                     arguments: null);


                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: ConfigurationManager.AppSettings["qeueuname"],
                                     basicProperties: null,
                                     body: body);
              // Console.WriteLine(" [x] Sent {0}", message);
            }
        }
       
    }
}
