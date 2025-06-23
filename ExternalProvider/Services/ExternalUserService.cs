using AutoMapper;
using ExternalProvider.Abstract;
using ExternalProvider.Models.Domain;
using ExternalProvider.Models.Dto;
using Newtonsoft.Json;
using ExternalProvider.Models.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace ExternalProvider.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly ExternalApiSettings _settings;
        private readonly IMemoryCache _cache;
        public ExternalUserService(IHttpClientFactory httpClientFactory, 
                                    IMapper mapper, 
                                    IOptions<ExternalApiSettings> options,
                                    IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _settings = options.Value;
            _cache = cache;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            string cacheKey = $"User_{id}";

            if (_cache.TryGetValue(cacheKey, out User cachedUser))
            {
                return cachedUser;
            }

            HttpClient client = _httpClientFactory.CreateClient("UserClient");

            var response = await client.GetAsync($"users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserResponseDto>(content);

            User user = new User();
            if (result == null)
                return user;
                        
            var userData = result.Data;
            user = _mapper.Map<User>(userData);

            _cache.Set(cacheKey, user, TimeSpan.FromSeconds(_settings.CacheDurationInSeconds));

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            string cacheKey = "AllUsers";

            if (_cache.TryGetValue(cacheKey, out List<User> cachedUsers))
            {
                return cachedUsers;
            }

            var users = new List<User>();
            int currentPage = 1;
            bool hasMoreUsers = true;

            HttpClient client = _httpClientFactory.CreateClient("UserClient");

            while (hasMoreUsers)
            {
                var response = await client.GetAsync($"users?page={currentPage}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UsersPaginatedListResponseDto>(content);

                if (result == null || result.Data == null || result.Data.Count == 0)
                    break;
                
                var pageUsersDto = result.Data;
                var pageUsers = _mapper.Map<List<User>>(pageUsersDto);

                users.AddRange(pageUsers);

                currentPage++;
                hasMoreUsers = currentPage <= result.Total_Pages;
            }

            _cache.Set(cacheKey, users, TimeSpan.FromSeconds(_settings.CacheDurationInSeconds));

            return users;
        }        
    }
}
