using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Competition
    {
        public string ProductName { get; set; }
        public IList<string> Companies { get; set; }
        public IList<string> Prices { get; set; }
        public Competition()
        {
            Companies = new List<string>();
            Prices = new List<string>();
        }
    }
}
