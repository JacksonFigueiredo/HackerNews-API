using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hacker_News_API.Models
{
    public class HackerNewsStory
    {
        public string by { get; set; }
        public int descendants { get; set; }
        public int id { get; set; }
        public List<int> kids { get; set; }
        public int score { get; set; }
        public long time { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }
}
