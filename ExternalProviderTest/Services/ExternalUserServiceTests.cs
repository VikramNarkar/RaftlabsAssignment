using ExternalProvider.Models.Domain;
using ExternalProvider.Models.Dto;
using Moq.Protected;
using Moq;
using Newtonsoft.Json;
using System.Net;
using AutoMapper;
using ExternalProvider.Services;
using Microsoft.Extensions.Options;
using ExternalProvider.Models.Config;
using Microsoft.Extensions.Caching.Memory;



namespace ExternalProviderTest.Services
{
    public class ExternalUserServiceTests
    {
        private ExternalUserService CreateService(HttpClient client, IMapper? mapperOverride = null)
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("UserClient")).Returns(client);

            var mapper = mapperOverride ?? new Mock<IMapper>().Object;

            var options = Options.Create(new ExternalApiSettings
            {
                BaseUrl = "https://reqres.in/api/",
                ApiKey = "reqres-free-v1"
            });

     
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            return new ExternalUserService(httpClientFactoryMock.Object, mapper, options, memoryCache);
        }


        [Fact]
        public async Task GetUserByIdAsync_ReturnsMappedUser_WhenApiSuccess()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 1, Email = "david.skaggs@example.com", First_Name = "David", Last_Name = "Skaggs", Avatar = "avatar1.jpg"
            };

            var apiResponse = new UserResponseDto
            {
                Data = userDto
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(apiResponse))
                });

            var mockHttpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://reqres.in/api/")
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("UserClient")).Returns(mockHttpClient);

            var mapperMock = new Mock<IMapper>();

            mapperMock
                .Setup(m => m.Map<User>(It.IsAny<UserDto>()))
                .Returns(new User
                {
                    Id = 1,
                    Email = "david.skaggs@example.com",
                    FirstName = "David",
                    LastName = "Skaggs",
                    Avatar = "avatar1.jpg"
                });


            var service = CreateService(mockHttpClient, mapperMock.Object);


            // Act
            var user = await service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user.Id);
            Assert.Equal("david.skaggs@example.com", user.Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsMappedUsers_WhenApiSuccess()
        {
            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Email = "david.skaggs@example.com", First_Name = "David", Last_Name = "Skaggs", Avatar = "avatar1.jpg" },
                new UserDto { Id = 2, Email = "adam.fondoble@example.com", First_Name = "Adam", Last_Name = "Fondoble", Avatar = "avatar2.jpg" }
            };

            var apiResponse = new UsersPaginatedListResponseDto
            {
                Page = 1,
                Per_Page = 6,
                Total = 2,
                Total_Pages = 1,
                Data = userDtos,
                Support = new SupportDto { Text = "Support text", Url = "http://support.url" }
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>
                ("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(apiResponse))
                });

            var mockHttpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new System.Uri("https://reqres.in/api/")
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            httpClientFactoryMock.Setup(f => f.CreateClient("UserClient")).Returns(mockHttpClient);

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<User>>(It.IsAny<List<UserDto>>()))
                .Returns(new List<User>
                {
                    new User { Id = 1, Email = "david.skaggs@example.com", FirstName = "David", LastName = "Skaggs", Avatar = "avatar1.jpg" },
                    new User { Id = 2, Email = "adam.fondoble@example.com", FirstName = "Adam", LastName = "Fondoble", Avatar = "avatar2.jpg" }
                });

            var service = CreateService(mockHttpClient, mapperMock.Object);

            // Act
            var users = await service.GetAllUsersAsync();

            // Assert
            Assert.NotNull(users);
            Assert.Equal(2, users.Count());
            Assert.Equal("david.skaggs@example.com", users.First().Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_ThrowsException_WhenApiFails()
        {
            // Arrange
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var mockHttpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://reqres.in/api/")
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            httpClientFactoryMock.Setup(f => f.CreateClient("UserClient")).Returns(mockHttpClient);

            var mapperMock = new Mock<IMapper>();

            var service = CreateService(mockHttpClient, mapperMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAllUsersAsync());
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmptyList_WhenApiReturnsEmptyData()
        {
            var apiResponse = new UsersPaginatedListResponseDto
            {
                Page = 1,
                Per_Page = 6,
                Total = 0,
                Total_Pages = 1,
                Data = new List<UserDto>(),
                Support = new SupportDto { Text = "Support", Url = "https://support.com" }
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(apiResponse))
                });

            var mockHttpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://reqres.in/api/")
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("UserClient")).Returns(mockHttpClient);

            var mapperMock = new Mock<IMapper>();

            var service = CreateService(mockHttpClient, mapperMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

    }
}