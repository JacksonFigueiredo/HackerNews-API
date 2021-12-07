using HackerNewsAPI.Business;
using HackerNewsAPI.DTOs;
using HackerNewsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackerNewsAPI.Controllers
{
    [ApiController]
    [Route("best20")]
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
        public async Task<List<HackerNewsStoryDTO>> Get()
        {
            try
            {
                string respString;
                List<int> stories;
                List<HackerNewsStoryDTO> bestStorysInDetails;

                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationMin)); // Set Cache.

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
                            return new List<HackerNewsStoryDTO>();
                        }
                    }

                    if (stories.Count > 0)
                    {
                        bestStorysInDetails = new List<HackerNewsStoryDTO>();

                        using (HttpClient httpClient = new HttpClient())
                        {
                            foreach (var StoryId in stories)
                            {
                                HttpResponseMessage response = await httpClient.GetAsync(HackerNewsStoryDetailsURL + StoryId + ".json"); 
                                if (response.IsSuccessStatusCode)
                                {
                                    var respData = await response.Content.ReadAsStringAsync();

                                    var story = JsonConvert.DeserializeObject<HackerNewsStory>(respData);

                                    var hackerNewsStory = new HackerNewsStoryDTO();

                                    hackerNewsStory.title = story.title;
                                    hackerNewsStory.uri = story.url;
                                    hackerNewsStory.PostedBy = story.by;
                                    hackerNewsStory.time = GenericMethods.UnixTimeStampToDateTime(story.time);
                                    hackerNewsStory.score = story.score;
                                    hackerNewsStory.CommentsCount = story.kids.Count();

                                    // using DTO Above to populate the stories.

                                    bestStorysInDetails.Add(hackerNewsStory);
                                }
                                else
                                {
                                    return new List<HackerNewsStoryDTO>();
                                }
                            }
                        }

                        _cache.Set("bestStories", bestStorysInDetails, cacheOptions); // insert the best stories inside a cache, to now stay asking always to the endpoint
                    }
                    else
                    {
                        return new List<HackerNewsStoryDTO>();
                    }
                }
                return bestStorysInDetails.Take(20).OrderByDescending(o => o.score).ToList(); // take only 20 stories, then sorting by their score in descending order
            }
            catch (Exception expt)
            {
                throw expt.InnerException; // Exception Handling - At least throwing
            }
        }
    }
}

