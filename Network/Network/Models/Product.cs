using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Models
{
    public class Product
    {
        public string? Url { get; set; }
        public string? Image { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public object Link { get; internal set; }
    }

}
