using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSIS_5
{
    class GetItem
    {
        public string Type { get; set; }
        public string Path { get; set; }

        public GetItem(string type, string path)
        {
            this.Path = path;
            this.Type = type;
        }
    }
}
