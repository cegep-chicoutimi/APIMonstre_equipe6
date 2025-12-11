using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Test_APIMonstre
{
    public class AuthTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public AuthTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Inscription_WithValidDataTests()
        {
            var testEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            var testPassword = "password123";
            var testPseudo = "TestPseudo";

            RegisterRequestDto requestDto = new()
            {
                Email = testEmail,
                Password = testPassword,
                Pseudo = testPseudo
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                requestDto
            );

            Assert.True(registerResponse.IsSuccessStatusCode,
                $"Registration failed: {await registerResponse.Content.ReadAsStringAsync()}");

            var response = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        }
    }
}
