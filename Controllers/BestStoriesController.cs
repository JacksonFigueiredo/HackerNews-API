using Hacker_News_API.Business;
using Hacker_News_API.DTOs;
using Hacker_News_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hacker_News_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BestStoriesController : ControllerBase
    {
        private IMemoryCache _cache;
        private const int cacheExpirationMin = 4;
        private const string HackerNewsBestStoriesURL = "https://hacker-news.firebaseio.com/v0/beststories.json";
        private const string HackerNewsStoryDetailsURL = "https://hacker-news.firebaseio.com/v0/item/";

        public BestStoriesController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        /// <summary>
        /// Gets the list of story IDs from the HackerNews API
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<HackerNewStoryDTO>> Get()
        {
            string respString;
            List<int> stories;
            List<HackerNewStoryDTO> bestStorysInDetails;

            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationMin));

            if (!_cache.TryGetValue("bestStories", out bestStorysInDetails))
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(HackerNewsBestStoriesURL);

                    if (response.IsSuccessStatusCode)
                    {
                        respString = await response.Content.ReadAsStringAsync();
                        stories = JsonConvert.DeserializeObject<List<int>>(respString);
                    }
                    else
                    {
                        return new List<HackerNewStoryDTO>();
                    }
                }

                if (stories.Count > 0)
                {
                    bestStorysInDetails = new List<HackerNewStoryDTO>();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        foreach (var StoryId in stories)
                        {
                            HttpResponseMessage response = await httpClient.GetAsync(HackerNewsStoryDetailsURL + StoryId + ".json");
                            if (response.IsSuccessStatusCode)
                            {
                                var respData = await response.Content.ReadAsStringAsync();
                                var story = JsonConvert.DeserializeObject<HackerNewsStory>(respData);
                                var hackerNewsStory = new HackerNewStoryDTO();

                                hackerNewsStory.title = story.title;
                                hackerNewsStory.url = story.url;
                                hackerNewsStory.by = story.by;
                                hackerNewsStory.time = GenericMethods.UnixTimeStampToDateTime(story.time);
                                hackerNewsStory.score = story.score;
                                //hackerNewsStory.kids = story.kids.Count;

                                bestStorysInDetails.Add(hackerNewsStory);
                            }
                            else
                            {
                                return new List<HackerNewStoryDTO>();
                            }
                        }
                    }

                    _cache.Set("bestStories", bestStorysInDetails, cacheOptions);
                }
                else
                {
                    return new List<HackerNewStoryDTO>();
                }
            }
            return bestStorysInDetails.Take(20).OrderByDescending(o => o.score).ToList();
        }
    }
}

