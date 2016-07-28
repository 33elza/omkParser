using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omkParser
{
    class OmkModel
    {
        public List<Dictionary<string, string>> Categories { get; set; }
        public List<Model> Header { get; set; }
        public List<Model> General { get; set; }
        public ObjectModel Object { get; set; }
        public List<Model> ProcedureOrder { get; set; }
        public List<Model> Organization { get; set; }
        public List<Model> ContactsInfo { get; set; }
        public List<Model> Documents { get; set; }


    }
}
