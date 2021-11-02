using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server_sip_alg.Model
{
    public class Response
    {
        public byte[] RequestHeader { get; set; }

        public byte[] RequestBody { get; set; }
    }
}
