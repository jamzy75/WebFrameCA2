using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebFrameCA2;
using System.Net.Http;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private Mock<RestClient> _mockRestClient;

        [TestInitialize]
        public void Setup()
        {
            // Initialize the mock RestClient before each test
            _mockRestClient = new Mock<RestClient>();
        }

        [TestMethod]
        public async Task GetWeatherAsync_ValidLocation_ReturnsWeatherResponse()
        {
            // Arrange
            string location = "Dundalk";
            var fakeJsonResponse = JsonConvert.SerializeObject(new WeatherResponse
            {
                ResolvedAddress = "Dundalk, Ireland",
                CurrentConditions = new CurrentConditions
                {
                    Temp = 15,
                    Conditions = "Clear"
                }
            });

            var mockResponse = new RestResponse
            {
                Content = fakeJsonResponse,
                StatusCode = System.Net.HttpStatusCode.OK
            };

            _mockRestClient
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), Method.Get, default))
                .ReturnsAsync(mockResponse);

            var weatherService = new WeatherService(_mockRestClient.Object);

            // Act
            var result = await weatherService.GetWeatherAsync(location);

            // Assert
            Assert.IsNotNull(result, "Expected non-null WeatherResponse");
            Assert.AreEqual("Dundalk, Ireland", result.ResolvedAddress);
            Assert.AreEqual(15, result.CurrentConditions.Temp);
            Assert.AreEqual("Clear", result.CurrentConditions.Conditions);
        }

        [TestMethod]
        public async Task GetWeatherAsync_InvalidLocation_ReturnsNull()
        {
            // Arrange
            string location = "InvalidLocation";
            var mockResponse = new RestResponse
            {
                Content = null,
                StatusCode = System.Net.HttpStatusCode.NotFound
            };

            _mockRestClient
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), Method.Get, default))
                .ReturnsAsync(mockResponse);

            var weatherService = new WeatherService(_mockRestClient.Object);

            // Act
            var result = await weatherService.GetWeatherAsync(location);

            // Assert
            Assert.IsNull(result, "Expected null WeatherResponse for invalid location");
        }

        [TestMethod]
        public async Task GetWeatherAsync_ApiError_ThrowsException()
        {
            // Arrange
            string location = "Dundalk";
            _mockRestClient
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), Method.Get, default))
                .ThrowsAsync(new HttpRequestException("API Error"));

            var weatherService = new WeatherService(_mockRestClient.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => weatherService.GetWeatherAsync(location));
        }
    }
}
