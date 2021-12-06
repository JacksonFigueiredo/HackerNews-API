using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsAPI.DTOs
{
    public class HackerNewsStoryDTO
    {
        public string PostedBy { get; set; }
        public int CommentsCount { get; set; }
        public int score { get; set; }
        public DateTime time { get; set; }
        public string title { get; set; }
        public string uri { get; set; }
    }
}
