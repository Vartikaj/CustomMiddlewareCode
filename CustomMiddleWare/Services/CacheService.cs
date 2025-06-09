using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json;

namespace CustomMiddleWare.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }   

        public async Task Set<T>(string key, T value, int slidingValue, int absoluteValue) where T : class
        {
            DistributedCacheEntryOptions oDistributedCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(absoluteValue),
                SlidingExpiration = TimeSpan.FromMinutes(slidingValue)
            };

            var response = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            await _cache.SetStringAsync(key, response, oDistributedCacheEntryOptions);
        }


        public async Task<T> Get<T> (IDistributedCache _distributedCache, string key)
        {
            var json = await _distributedCache.GetStringAsync(key);
            return json == null ? default : JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
