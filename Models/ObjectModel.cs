using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using omkParser.Models;

namespace omkParser
{
    class ObjectModel
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public ObjectValueModel FieldValue { get; set; }

    }
}
