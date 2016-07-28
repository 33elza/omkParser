using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omkParser.Models
{
    class NotificationModel
    {
        public object Id { get; set; }
        public List<OrganisationId> Customers { get; set; }
        public int Type { get; set; }
        public DateTime TineStamp { get; set; }
        public string NitificationNumber { get; set; }
        public string OrderName { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? SubmissionCloseDateTime { get; set; }
        public DateTime? PublicationDateTime { get; set; }
        public double? GuaranteeApp { get; set; }
        public double? GuaranteeContract { get; set; }
        public int PlacingWayId { get; set; }
        public int RegionCode { get; set; }
        public string Okpd { get; set; }
        public string Okpd2 { get; set; }
        public string Organisations { get; set; }
        public string Text { get; set; }
        public string KeySearch { get; set; }
        public bool Version { get; set; }
        public bool Multilot { get; set; }
        public string Json { get; set; }
    }
}
