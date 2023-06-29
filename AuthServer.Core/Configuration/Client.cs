using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Configuration
{
    public class Client
    {
        public string Id { get; set; }
        public string Secret { get; set; }
        /// <summary>
        /// Bu clientın erişebileceği apiler.
        /// www.myapi1.com, www.myapi2.com etc.
        /// </summary>
        public List<string> Audiences { get; set; }
    }
}
