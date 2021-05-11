using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace KSIS_5
{
    interface IHttpComand
    {
        void Comand(HttpListenerRequest request, ref HttpListenerResponse response);
    }
}
