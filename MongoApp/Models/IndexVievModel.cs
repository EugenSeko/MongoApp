using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoApp.Models
{
    public class IndexViewModel
    {
        public FilterViewModel Filter { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
