using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHot.Core.POCO
{
    class TopRepositoriesList
    {
        public string Criteria { get; set; }
        public string CreatedAt { get; set; }
        public int Weeks { get; set; }
        public List<TopRepository> Items { get; set; }
    }
}
