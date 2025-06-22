using AutoMapper;
using ExternalProvider.Abstract;
using ExternalProvider.Models.Domain;
using ExternalProvider.Models.Dto;
using Newtonsoft.Json;
using ExternalProvider.Models.Config;
using Microsoft.Extensions.Options;

namespace ExternalProvider.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly ExternalApiSettings _settings;
        public ExternalUserService(IHttpClientFactory httpClientFactory, IMapper mapper, 
                                    IOptions<ExternalApiSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _settings = options.Value;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            HttpClient client = _httpClientFactory.CreateClient("UserClient");

            client.BaseAddress = new Uri(_settings.BaseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);

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

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            int currentPage = 1;
            bool hasMoreUsers = true;

            HttpClient client = _httpClientFactory.CreateClient("UserClient");

            client.BaseAddress = new Uri(_settings.BaseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);

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

            return users;
        }        
    }
}
