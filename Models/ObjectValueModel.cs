using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omkParser.Models
{
    class ObjectValueModel
    {
        public List<String> Head { get; set; }
        public List<List<Model>> Body { get; set; }
    }
}
