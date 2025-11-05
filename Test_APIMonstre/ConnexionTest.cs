using APIMonstre.Models.Dto;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Test_APIMonstre
{
    public class ConnexionTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _registeredEmail;
        private string _password;
        private bool _isSetupDone = false;
        private readonly object _lock = new();

        public ConnexionTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            if (!_isSetupDone)
            {
                lock (_lock)
                {
                    if (!_isSetupDone)
                    {
                        SetupAsync().GetAwaiter().GetResult();
                        _isSetupDone = true;
                    }
                }
            }
        }

        private async Task SetupAsync()
        {
            await Task.Delay(2000); // Donne un peu de temps à l'appli pour démarrer

            _registeredEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            _password = "password123";
            var testPseudo = "TestHero";

            var registerDto = new RegisterRequestDto
            {
                Email = _registeredEmail,
                Password = _password,
                Pseudo = testPseudo
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                registerDto
            );

            var content = await registerResponse.Content.ReadAsStringAsync();

            Assert.True(registerResponse.IsSuccessStatusCode,
                $"L'enregistrement a échoué : {content}");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_ReturnsOk()
        {
            LoginRequestDto requestDto = new()
            {
                Email = _registeredEmail,
                Password = _password
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );

            Assert.True(loginResponse.IsSuccessStatusCode,
                $"Registration failed: {await loginResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_SetsEstConnecteToTrue()
        {
            LoginRequestDto requestDto = new()
            {
                Email = _registeredEmail,
                Password = _password
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );

            Assert.True(loginResponse.IsSuccessStatusCode,
                $"Registration failed: {await loginResponse.Content.ReadAsStringAsync()}");

            var response = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.True(response.EstConnecte, "L'utilisateur n'est pas marqué comme connecté après la connexion.");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_AllowsSubsequentAuthenticatedRequests() 
        {
            LoginRequestDto requestDto = new()
            {
                Email = _registeredEmail,
                Password = _password
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );

            Assert.True(loginResponse.IsSuccessStatusCode,
                $"Registration failed: {await loginResponse.Content.ReadAsStringAsync()}");

            var posX = 49;
            var posY = 49;

            int[][] coords =
            [
                [posX-1, posY+1],
                [posX, posY+1],
                [posX+1,posY+1],
                [posX+1,posY],
                [posX + 1, posY-1],
                [posX, posY - 1],
                [posX - 1, posY - 1],
                [posX - 1, posY]
            ];
            ExplorerDto exploreDto = new ExplorerDto
            {
                Coords = coords,
                Email = _registeredEmail
            };

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();
            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Échec de l'exploration : {responseContent}");
        }

        [Fact]
        public async Task Connexion_WithInvalidEmail_ReturnsUnauthorized()
        {
            LoginRequestDto requestDto = new()
            {
                Email = "test@test",
                Password = _password
            };
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );
            Assert.False(loginResponse.IsSuccessStatusCode,
                "La connexion a réussi avec des informations d'identification invalides.");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Connexion_WithInvalidPassword_ReturnsUnauthorized()
        {
            LoginRequestDto requestDto = new()
            {
                Email = _registeredEmail,
                Password = _password + "WRONG"
            };
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );
            Assert.False(loginResponse.IsSuccessStatusCode,
                "La connexion a réussi avec des informations d'identification invalides.");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Connexion_WithNonexistentUser_ReturnsUnauthorized()
        {
            LoginRequestDto requestDto = new()
            {
                Email = $"testplayer_{Guid.NewGuid()}@test.com",
                Password = _password
            };
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );
            Assert.False(loginResponse.IsSuccessStatusCode,
                "La connexion a réussi avec des informations d'identification invalides.");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Connexion_WithEmptyCredentials_ReturnsBadRequest() 
        {
            LoginRequestDto requestDto = new()
            {
                Email = "",
                Password = ""
            };
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                requestDto
            );
            Assert.False(loginResponse.IsSuccessStatusCode,
                "La connexion a réussi avec des informations d'identification invalides.");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, loginResponse.StatusCode);
        }
    }
}
