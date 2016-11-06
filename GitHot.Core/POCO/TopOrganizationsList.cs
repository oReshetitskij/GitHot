using System.Collections.Generic;

namespace GitHot.Core.POCO
{
    public class TopOrganizationsList
    {
        public string Criteria { get; set; }
        public string CreatedAt { get; set; }
        public int Weeks { get; set; }
        public List<TopOrganization> Items { get; set; }
    }
}
