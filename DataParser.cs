﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;
using Newtonsoft.Json;
using omkParser.Models;
using System.Globalization;

namespace omkParser
{
    class DataParser
    {
        public List<String> CollectLinks(HtmlDocument doc)
        {
            List<String> links = new List<String>();
            var table = doc.DocumentNode.SelectSingleNode("//table[@class='framer pt8']");
            if (table != null)
            {
                var trNodes = table.ChildNodes.Where(x => x.Name == "tr").Skip(2);
                foreach (var item in trNodes)
                {
                    var tdNodes = item.ChildNodes.Where(x => x.Name == "td").ToArray(); 
                    if (tdNodes.Count() != 0)
                    {
                        foreach(var node in tdNodes)
                        {
                            if (node.ChildNodes.Where(x=> x.Name == "a").Count() != 0)
                            {
                                var link = node.ChildNodes.Where(x => x.Name == "a").FirstOrDefault().Attributes["href"].Value;
                                links.Add(link);
                            }
                        }                       
                    }                                                                                                                                                                                                                                                                                                                                                                                                                                    
                }
            }
            else
            {
                Debug.WriteLine("не найдено!!!");
            }
            return links;
        }
        //статус: новый, на рассмотрении, продлённый
        public List<String> CollectActiveLinks(HtmlDocument doc)
        {
            List<String> links = new List<String>();
            var table = doc.DocumentNode.SelectSingleNode("//table[@class='framer pt8']");
            if (table != null)
            {
                var trNodes = table.ChildNodes.Where(x => x.Name == "tr").Skip(2);
                foreach (var item in trNodes)
                {
                    var tdNodes = item.ChildNodes.Where(x => x.Name == "td").ToArray();
                    if (tdNodes.Count() != 0)
                    {
                        string status = tdNodes[0].InnerText;
                        if (status == "на рассмотрении" || status == "новый" || status == "продлённый")
                        {
                            foreach (var node in tdNodes)
                            {
                                if (node.ChildNodes.Where(x => x.Name == "a").Count() != 0)
                                {
                                    var link = node.ChildNodes.Where(x => x.Name == "a").FirstOrDefault().Attributes["href"].Value;
                                    links.Add(link);
                                }
                            }
                        }                                                
                    }
                }
            }
            else
            {
                Debug.WriteLine("не найдено!!!");
            }
            return links;
        }

        public string ParseTender(HtmlDocument doc, string linkToTender)
        {
            var info = doc.DocumentNode.SelectSingleNode("//td[@class='bodypanel']");

            var nodes = doc.DocumentNode.SelectNodes("//td[@class='bodypanel'] //b").ToList();

            string LotNumber, State, Customer, LotName, Requirements, RequirementsTMC, OtherRequirements, CriterialChoiceSuppliers, FIO, Contacts, DatePublication, DateExtension, BiddingEnd, DeliveryTime;
            Contacts = ""; FIO = ""; DateExtension = ""; DatePublication = ""; BiddingEnd = ""; LotName = ""; DeliveryTime = ""; State = ""; LotNumber = ""; Requirements = ""; RequirementsTMC = ""; OtherRequirements = ""; CriterialChoiceSuppliers = "";
            Customer = "";
            OmkModel omkModel = new OmkModel();
            foreach (HtmlNode node in nodes)
            {
                switch (node.InnerText)
                {
                    case "Номер лота:": LotNumber = node.NextSibling.InnerText;
                        break;
                    case "Статус:": State = node.NextSibling.InnerText;
                        break;
                    case "Запрос предложений на поставку МТР для:": Customer = node.NextSibling.InnerText;
                        break;
                    case "Наименование лота:": LotName = node.NextSibling.InnerText;
                        break;
                    case "Требования к поставщику:": Requirements = node.NextSibling.InnerText;
                        break;
                    case "Требования к ТМЦ:": RequirementsTMC = node.NextSibling.InnerText;
                        break;
                    case "Прочие требования:": OtherRequirements = node.NextSibling.InnerText;
                        break;
                    case "Критерии выбора поставщика:": CriterialChoiceSuppliers = node.NextSibling.InnerText;
                        break;
                    case "Закупщик:": FIO = node.NextSibling.InnerText;
                        break;
                    case "Контакты:": Contacts = node.NextSibling.InnerText;
                        break;
                    case "Дата размещения запроса:": DatePublication = node.NextSibling.InnerText;
                        break;
                    case "Дата последнего продления запроса:": DateExtension = node.NextSibling.InnerText;
                        break;
                    case "Дата окончания приема предложений:": BiddingEnd = node.NextSibling.NextSibling.InnerText;
                        break;
                    case "Требуемый срок поставки:": DeliveryTime = node.NextSibling.InnerText;
                        break;
                }
            }

            var emailscript = doc.DocumentNode.SelectNodes("//td[@class='bodypanel'] //script").FirstOrDefault().InnerHtml;

            var doclinks = doc.DocumentNode.SelectNodes("//td[@class='bodypanel'] //a");

            HtmlNode lotTable = doc.DocumentNode.SelectSingleNode("//table[@class='framer pt8']");

            string phone = Contacts.Trim().Substring(Contacts.Trim().IndexOf(';') + 1).Replace(";", "").Trim();

            omkModel.Categories = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                       { "Header" , "Заголовок" },
                       { "General", "Основная информация" },
                       { "Object", "Информация о лоте" },
                       { "ProcedureOrder", "Порядок размещения заказа" },
                       { "Organization", "Организация" },
                       { "ContactsInfo", "Контактная информация" },
                       { "Documents", "Документы" }
                }
            };
            omkModel.Header = new List<Model>
            {
                new Model { FieldName = "Type", FieldType = "String", FieldValue = "Запрос предложений", FieldDisplayName = "Тип процедуры", Position = 1 },
                new Model { FieldName = "State", FieldType = "String", FieldValue = State.Trim(), FieldDisplayName = "Состояние процедуры", Position = 2 },
                new Model { FieldName = "Platfom", FieldType = "String", FieldValue = "OMK", FieldDisplayName = "Площадка", Position = 3 },
                new Model { FieldName = "Region", FieldType = "String", FieldValue = DefineRegion(Customer.Trim()), FieldDisplayName = "Регион", Position = 4 },
                new Model { FieldName = "LinkToPlatform", FieldType = "String", FieldValue = "http://omk.zakupim.ru/", FieldDisplayName = "OMK", Position = 5 },
                new Model { FieldName = "LinkToTender", FieldType = "String", FieldValue = "http://omk.zakupim.ru"+linkToTender, FieldDisplayName = LotNumber.Trim(), Position = 6 }
            };
            omkModel.General = new List<Model>
            {
                new Model { FieldName = "Name", FieldType = "String", FieldValue = LotName.Trim(), FieldDisplayName = "Наименование лота", Position = 1 },
                new Model { FieldName = "Customer", FieldType = "String", FieldValue = Customer.Trim(), FieldDisplayName = "Организация заказчик", Position = 2 },
                new Model { FieldName = "DeliveryTime", FieldType = "DateTime", FieldValue = DeliveryTime.Trim(), FieldDisplayName = "Сроки поставки", Position = 3 },
                new Model { FieldName = "Requirements", FieldType = "String", FieldValue = Requirements.Trim(), FieldDisplayName = "Требования к поставщику", Position = 4 },
                new Model { FieldName = "Requirements TMC", FieldType = "String", FieldValue = RequirementsTMC.Trim(), FieldDisplayName = "Требования к ТМЦ", Position = 5 },
                new Model { FieldName = "Other Requirements", FieldType = "String", FieldValue = OtherRequirements.Trim(), FieldDisplayName = "Прочие требования", Position = 6 },
                new Model { FieldName = "CriterialChoiceSuppliers", FieldType = "String", FieldValue = CriterialChoiceSuppliers.Trim(), FieldDisplayName = "Критерии выбора поставщика", Position = 7 }

            };
            omkModel.ContactsInfo = new List<Model>
            {
                new Model { FieldName = "FIO", FieldType = "String", FieldValue = FIO.Trim(), FieldDisplayName = "ФИО контактного лица", Position = 1 },
                new Model { FieldName = "Phone", FieldType = "String", FieldValue = phone, FieldDisplayName = "Номер телефона", Position = 2 },
                new Model { FieldName = "Email", FieldType = "String", FieldValue = CreateEmail(emailscript), FieldDisplayName = "Электронная почта", Position = 3 },
                new Model { FieldName = "Adress", FieldType = "String", FieldValue = Contacts.Trim().Substring(0, Contacts.Trim().IndexOf(';')).Trim(), FieldDisplayName = "Адрес", Position = 4 }
            };
            omkModel.ProcedureOrder = new List<Model>
            {
                new Model { FieldName = "DatePublication", FieldType = "DataTime", FieldValue = DatePublication.Trim(), FieldDisplayName = "Дата размещения запроса", Position = 1 },
                new Model { FieldName = "DateExtension", FieldType = "DataTime", FieldValue = DateExtension.Trim(), FieldDisplayName = "Дата последнего продления запроса", Position = 2 },
                new Model { FieldName = "BiddingEnd", FieldType = "DataTime", FieldValue = BiddingEnd, FieldDisplayName = "Дата окончания приема предложений", Position = 3 }
            };

            if (doclinks != null)
            {
                omkModel.Documents = new List<Model>();
                for (int i = 0; i < doclinks.Count; i++)
                {
                    omkModel.Documents.Add(new Model { FieldName = "Document", FieldType = "String", FieldValue = "http://omk.zakupim.ru/" + doclinks[i].Attributes["href"].Value, FieldDisplayName = doclinks[i].InnerText, Position = i + 1 });
                }
            }
            omkModel.Organization = new List<Model>
          {
               new Model { FieldName = "Name", FieldType = "String", FieldValue = Customer.Trim(), FieldDisplayName = "Название организации", Position = 1 },
               new Model { FieldName = "Address", FieldType = "String", FieldValue = DefineOrgAddress(Customer.Trim()), FieldDisplayName = "Адрес", Position = 2 }
          };

            ObjectValueModel obValMod = new ObjectValueModel();
            obValMod.Head = new List<string>
            {
                "Номер позиции",
			    "Наименование позиции", 
				"Ном.номер", 
				"ГОСТ\\ТУ",
				"Ед. изм",
				"Кол-во к закупу",
				"требуемая дата поставки"
            };
            obValMod.Body = collectLots(lotTable);
            ObjectModel obMod = new ObjectModel { FieldName = "LotList", FieldType = "Table", FieldValue = obValMod };
            omkModel.Object = obMod;

            string json = JsonConvert.SerializeObject(omkModel);

            Debug.WriteLine(json);
            return json;
        }

        public List<List<Model>> collectLots(HtmlNode table)
        {
            HtmlDocument docTable = new HtmlDocument();
            docTable.LoadHtml(table.OuterHtml);
            
            List<List<Model>> body = new List<List<Model>>();
            string[] trsep = {"<tr" } ;
            string[] tdsep = { "<td>" };
            var trnodes = docTable.DocumentNode.OuterHtml.Split(trsep, StringSplitOptions.None).Skip(2);
            foreach (var n in trnodes)
            {
                List<Model> itemBody = new List<Model>();
                var tdnodes = n.Split(tdsep, StringSplitOptions.None).Skip(1);
                List<string> t = new List<string>();
                foreach (var td in tdnodes)
                {
                    t.Add(td);
                }

                itemBody.Add(new Model { FieldName = "Number", FieldType = "Int", FieldValue = t[0].Substring(0, t[0].IndexOf("\n")).Trim() });
                    itemBody.Add(new Model { FieldName = "Name", FieldType = "String", FieldValue = t[1].Substring(0, t[1].IndexOf("<script>")).Trim() });
                    itemBody.Add(new Model { FieldName = "NormNumber", FieldType = "String", FieldValue = t[2].Substring(0, t[2].IndexOf("\n")).Trim() });
                    itemBody.Add(new Model { FieldName = "Ghost", FieldType = "String", FieldValue = t[3].Substring(0, t[3].IndexOf("\n")).Trim() });
                    itemBody.Add(new Model { FieldName = "Measure", FieldType = "String", FieldValue = t[4].Substring(0, t[4].IndexOf("\n")).Trim() });
                    itemBody.Add(new Model { FieldName = "Count", FieldType = "Int", FieldValue = t[5].Substring(0, t[5].IndexOf("\n")).Trim() });
                    itemBody.Add(new Model { FieldName = "DeliveryTime", FieldType = "DataTime", FieldValue = t[6].Substring(0, t[6].IndexOf("\n")).Trim() });

                    body.Add(itemBody);
                   // Debug.WriteLine(t[1]);
            }
           
            //var trs = docTable.DocumentNode.SelectNodes("//tr").Skip(1);
            //foreach (var tr in trs)
            //{
            //    //Debug.WriteLine(tr.OuterHtml);
            //    //Debug.WriteLine("______________");
            //    List<Model> itemBody = new List<Model>();
            //    var tdNodes = tr.ChildNodes.Where(x => x.Name == "td").ToArray();
            //    if (tdNodes.Count() != 0)
            //    {
            //        var tdchildnodes = tdNodes[0].ChildNodes.Where(x => x.Name == "td").ToArray();

            //        itemBody.Add(new Model { FieldName = "Number", FieldType = "Int", FieldValue = tdNodes[0].ChildNodes[0].InnerText.Trim() });
            //        itemBody.Add(new Model { FieldName = "Name", FieldType = "String", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[0].InnerText.Trim() });
            //       // itemBody.Add(new Model { FieldName = "NormNumber", FieldType = "String", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes[0].InnerText.Trim() });
            //       // itemBody.Add(new Model { FieldName = "NormNumber", FieldType = "String", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].InnerText.Trim() });
            //       // itemBody.Add(new Model { FieldName = "Ghost", FieldType = "String", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].InnerText.Trim() });
            //       // itemBody.Add(new Model { FieldName = "Measure", FieldType = "String", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[0].InnerText.Trim() });
            //        //itemBody.Add(new Model { FieldName = "Count", FieldType = "Int", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].InnerText.Trim() });
            //       // itemBody.Add(new Model { FieldName = "DeliveryTime", FieldType = "DataTime", FieldValue = tdNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].InnerText.Trim() });

            //        body.Add(itemBody);
            //        //Debug.WriteLine(tdNodes[0].ChildNodes[1].OuterHtml);
            //    }
            //}
            return body;
        }

        public string CreateEmail(string emailscript)
        {
            string s_r_v = emailscript.Substring(emailscript.IndexOf("s_r_v") + 9);
            s_r_v = s_r_v.Remove(s_r_v.IndexOf("'"));
            string n_a_m_e = emailscript.Substring(emailscript.IndexOf("n_a_m_e") + 11);
            n_a_m_e = n_a_m_e.Remove(n_a_m_e.IndexOf("'"));
            string s_f_x = emailscript.Substring(emailscript.IndexOf("s_f_x") + 9);
            s_f_x = s_f_x.Remove(s_f_x.IndexOf("'"));
            string email = n_a_m_e + "@" + s_r_v + "." + s_f_x;

            return email;
        }
        public string DefineRegion(string orgName)
        {
            switch(orgName)
            {
                case "ЗАО ОМК": return "Московская область";
                case "ОАО \"АТЗ\"": return "Республика Татарстан";
                case "ОАО \"БАЗ\"": return "Республика Башкортостан";
                case "ОАО \"ВМЗ\"": return "Нижегородская область";
                case "ОАО \"Губахинский Кокс\"": return "Пермский край";
                case "ОАО \"ОМК-Сталь\" (ЛПК)": return "Нижегородская область";
                case "ОАО \"Трубодеталь\"": return "Челябинская область";
                case "ОАО \"ЧМЗ\"": return "Пермский край";
                case "ОАО \"Щелмет\"": return "Московская область";
                case "ООО \"БГДК\"": return "Белгородская область";
                case "ООО \"ВМЗ-Техно\"": return "Нижегородская область";
                case "ООО \"ОМК-Проект\"": return "Нижегородская область";
                case "Филиал ОАО \"Трубодеталь\" в г.Чусовой": return "Пермский край";
                default: return "";
            }            
        }
        public string DefineOrgAddress(string orgName)
        {
            switch (orgName)
            {
                case "ЗАО ОМК": return "115184, Москва, Озерковская наб., д. 28, стр. 2";
                case "ОАО \"АТЗ\"": return "Ул. Индустриальная, д. 35, г. Альметьевск, Республика Татарстан, Россия, 423450";
                case "ОАО \"БАЗ\"": return "Ул. Седова, д. 1, г. Благовещенск, Республика Башкортостан, 453430";
                case "ОАО \"ВМЗ\"": return "Ул. Бр. Баташевых, 45, г. Выкса, Нижегородская обл., Россия, 607060";
                case "ОАО \"Губахинский Кокс\"": return "618250, Пермский край, г Губаха, ул Торговая, 1";
                case "ОАО \"ОМК-Сталь\" (ЛПК)": return "Проммикрорайон-7, Выксунский район, Нижегородская обл., Россия, 607060";
                case "ОАО \"Трубодеталь\"": return "Ул. Челябинская, д. 23, г. Челябинск, Россия, 454904";
                case "ОАО \"ЧМЗ\"": return "ул. Трудовая, д.13, г. Чусовой, Пермский край, Россия, 618200";
                case "ОАО \"Щелмет\"": return "141112, Московская обл, р-н Щелковский, г Щелково, ул Октябрьская, 21";
                case "ООО \"БГДК\"": return "г. Белгород, Свято-Троицкий бульвар, 11, оф.2";
                case "ООО \"ВМЗ-Техно\"": return "607061, Нижегородская обл, г Выкса, ул Братьев Баташевых, 45 ";
                case "ООО \"ОМК-Проект\"": return "607060, Нижегородская обл., г. Выкса, ул. Братьев Баташевых, д. 45";
                case "Филиал ОАО \"Трубодеталь\" в г.Чусовой": return "618200, Пермский край, Чусовской район, город Чусовой, Трудовая улица, дом 13";
                default: return "";
            }         
        }
        public OrganisationId CreateOrgId(string orgName)
        {
            switch(orgName)
            {
                case "ЗАО ОМК": return new OrganisationId { Inn = "7736030085", Kpp = "770501001" };
                case "ОАО \"АТЗ\"": return new OrganisationId { Inn = "1644006532", Kpp = "164401001" };
                case "ОАО \"БАЗ\"": return new OrganisationId { Inn = "0258001489", Kpp = "025801001" };
                case "ОАО \"ВМЗ\"": return new OrganisationId { Inn = "5247004695", Kpp = "524701001" };
                case "ОАО \"Губахинский Кокс\"": return new OrganisationId { Inn = "5913004822", Kpp = "591301001" };
                case "ОАО \"ОМК-Сталь\" (ЛПК)": return new OrganisationId { Inn = "7705893229 ", Kpp = "770501001" };
                case "ОАО \"Трубодеталь\"": return new OrganisationId { Inn = "7451047011 ", Kpp = "745101001" };
                case "ОАО \"ЧМЗ\"": return new OrganisationId { Inn = "5921002018 ", Kpp = "592101001" };
                case "ОАО \"Щелмет\"": return new OrganisationId { Inn = "5050008290 ", Kpp = "505001001" };
                case "ООО \"БГДК\"": return new OrganisationId { Inn = "3123155916 ", Kpp = "312301001" };
                case "ООО \"ВМЗ-Техно\"": return new OrganisationId { Inn = "5247046110 ", Kpp = "524701001" };
                case "ООО \"ОМК-Проект\"": return new OrganisationId { Inn = "5247048004 ", Kpp = "524701001" };
                case "Филиал ОАО \"Трубодеталь\" в г.Чусовой": return new OrganisationId { Inn = "7451047011", Kpp = "592143001" };
                default: return null;
            }           
        }
        public string CreateOrganisationName(string orgName)
        {
             switch(orgName)
            {
                case "ЗАО ОМК": return "ЗАО Объединенная Металлургическая Компания ЗАО ОМК";
                case "ОАО \"АТЗ\"": return "ОАО Альметьевский Трубный Завод ОАО \"АТЗ\"";
                case "ОАО \"БАЗ\"": return "ОАО Благовещенский Арматурный Завод ОАО \"БАЗ\"";
                case "ОАО \"ВМЗ\"": return "ОАО Выксунский Металлургический Завод ОАО \"ВМЗ\"";
                case "ОАО \"Губахинский Кокс\"": return "ОАО \"Губахинский Кокс\"";
                case "ОАО \"ОМК-Сталь\" (ЛПК)": return "Литейно-прокатный комплекс ОАО \"ОМК-Сталь\" (ЛПК)";
                case "ОАО \"Трубодеталь\"": return "ОАО \"Трубодеталь\"";
                case "ОАО \"ЧМЗ\"": return "ОАО Чусовский Металлургический Завод ОАО \"ЧМЗ\"";
                case "ОАО \"Щелмет\"": return "ОАО \"Щелмет\"";
                case "ООО \"БГДК\"": return "ООО \"БГДК\"";
                case "ООО \"ВМЗ-Техно\"": return "ООО \"ВМЗ-Техно\"";
                case "ООО \"ОМК-Проект\"": return "ООО \"ОМК-Проект\"";
                case "Филиал ОАО \"Трубодеталь\" в г.Чусовой": return "Филиал ОАО \"Трубодеталь\" в г.Чусовой";
                default: return "";
            }      
        }
        public int DefineRegionCode(string orgName)
        {
            switch (orgName)
            {
                case "ЗАО ОМК": return 50;
                case "ОАО \"АТЗ\"": return 16;
                case "ОАО \"БАЗ\"": return 2;
                case "ОАО \"ВМЗ\"": return 52;
                case "ОАО \"Губахинский Кокс\"": return 59;
                case "ОАО \"ОМК-Сталь\" (ЛПК)": return 52;
                case "ОАО \"Трубодеталь\"": return 74;
                case "ОАО \"ЧМЗ\"": return 59;
                case "ОАО \"Щелмет\"": return 50;
                case "ООО \"БГДК\"": return 31;
                case "ООО \"ВМЗ-Техно\"": return 52;
                case "ООО \"ОМК-Проект\"": return 52;
                case "Филиал ОАО \"Трубодеталь\" в г.Чусовой": return 59;
                default: return 0;
            }
        }
        public string CreateNotModel(string json)
        {
            OmkModel omkmod = JsonConvert.DeserializeObject<OmkModel>(json);
            NotificationModel notmodel = new NotificationModel();
            if (json != null)
            {               
                notmodel.OrderName = omkmod.General[0].FieldValue;
                notmodel.SubmissionCloseDateTime = GetDateTime(omkmod.ProcedureOrder[2].FieldValue);
                notmodel.PublicationDateTime = GetDateTime(omkmod.ProcedureOrder[0].FieldValue);
                notmodel.PlacingWayId = 5000;
                notmodel.Multilot = omkmod.Object.FieldValue.Body.Count > 1 ? true : false;
                notmodel.Json = json;
                notmodel.Organisations = CreateOrganisationName(omkmod.Organization[0].FieldValue);
                notmodel.Customers = new List<OrganisationId> { CreateOrgId(omkmod.Organization[0].FieldValue) };
                notmodel.Type = 28;
                notmodel.Id = new { NM = "OMK" + omkmod.Header[5].FieldDisplayName };
                notmodel.NotificationNumber = "OMK" + omkmod.Header[5].FieldDisplayName;
                notmodel.RegionCode = DefineRegionCode(omkmod.Organization[0].FieldValue);
               
                notmodel.Text = notmodel.OrderName + " 5000 " + notmodel.Organisations + " " +notmodel.Customers.First().Inn + " " + notmodel.Customers.First().Kpp;
                notmodel.KeySearch = notmodel.Organisations + notmodel.OrderName + " ";
                foreach (var lot in omkmod.Object.FieldValue.Body)
                {
                    notmodel.KeySearch = notmodel.KeySearch + lot.Where(x => x.FieldName == "Name").Select(x => x.FieldValue);

                }
            }
            string notModelJson = JsonConvert.SerializeObject(notmodel);
            return notModelJson;
        }

        public DateTime GetDateTime(string time)
        {
            DateTime date;
            var success = DateTime.TryParseExact(time.Trim(), "dd MMMM yyyy hh:mm:ss",
                CultureInfo.GetCultureInfo("ru-Ru"), DateTimeStyles.None, out date);
            return date;
        }

    }

    
}
