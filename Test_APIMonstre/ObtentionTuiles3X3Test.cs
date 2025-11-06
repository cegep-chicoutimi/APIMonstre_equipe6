using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace Test_APIMonstre
{
    public class ObtentionTuiles3X3Test : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _registeredEmail = "test_Obtention3X3@test.com";
        private string _logOutUser = "logoutUser@test.com";
        private string _password = "password123";
        

        public ObtentionTuiles3X3Test(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTuiles_AtMapEdge_ReturnsOnlyAvailableTiles()
        {
            var posX = 49;
            var posY = 49;

            await Task.Delay(2000); // Attend que la carte soit prête

            var coords = GetCoordsAround(posX, posY);
            ExplorerDto exploreDto = CreateDto(coords);

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();
            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Échec de l'exploration : {responseContent}");

            var exploreResult = await exploreResponse.Content.ReadFromJsonAsync<TuileAvecInfosDto[]>();
            Assert.Equal(3, exploreResult.Length);
        }

        [Fact]
        public async Task GetTuiles_WithAuthenticatedUser_Returns3x3Grid()
        {
            var posX = 25;
            var posY = 25;

            await Task.Delay(2000); // Attend que la carte soit prête

            var coords = GetCoordsAround(posX, posY);
            ExplorerDto exploreDto = CreateDto(coords);

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();
            var exploreResult = await exploreResponse.Content.ReadFromJsonAsync<TuileAvecInfosDto[]>();
            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Échec de l'exploration : {responseContent}");
            Assert.Equal(8, exploreResult.Length);
        }

        [Fact]
        public async Task GetTuiles_WithAuthenticatedUser_IncludesMonsterData()
        {
            var posX = 23;
            var posY = 23;

            await Task.Delay(2000); // Attend que la carte soit prête

            var coords = GetCoordsAround(posX, posY);
            ExplorerDto exploreDto = CreateDto(coords);

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();
            var exploreResult = await exploreResponse.Content.ReadFromJsonAsync<TuileAvecInfosDto[]>();
            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Échec de l'exploration : {responseContent}");
            var tuileWithMonstre = exploreResult.FirstOrDefault(m => m.PositionX == 23 && m.PositionY == 22);
            Assert.Equal(681, tuileWithMonstre.Monstre.MonstreId);
            Assert.Equal("aegislash-shield", tuileWithMonstre.Monstre.Nom);
        }

        private ExplorerDto CreateDto(int[][] coords)
        {
            return new ExplorerDto
            {
                Coords = coords,
                Email = _registeredEmail
            };
        }

        [Fact]
        public async Task GetTuiles_WithoutAuthentication_ReturnsUnauthorized()
        {
            var posX = 23;
            var posY = 23;

            await Task.Delay(2000); // Attend que la carte soit prête

            var coords = GetCoordsAround(posX, posY);

            var exploreDto = new ExplorerDto
            {
                Coords = coords,
                Email = $"testplayer_{Guid.NewGuid()}@test.com"
            };

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();

            // Vérifie que le statut HTTP est 404 Not Found
            Assert.Equal(System.Net.HttpStatusCode.NotFound, exploreResponse.StatusCode);

            // Vérifie que le message de retour contient bien ton message
            Assert.Contains("Utilisateur non trouvé", responseContent);
        }

        [Fact]
        public async Task GetTuiles_WithDisconnectedUser_ReturnsUnauthorized()
        {
            var posX = 23;
            var posY = 23;

            await Task.Delay(2000); // Attend que la carte soit prête

            var registerDto = new RegisterRequestDto
            {
                Email = _logOutUser,
                Password = _password,
                Pseudo = "TestLoggedOut"
            };

            await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                registerDto
            );
            await _client.PostAsJsonAsync(
                "/api/Utilisateurs/logout",
                registerDto
            );

            var coords = GetCoordsAround(posX, posY);

            var exploreDto = new ExplorerDto
            {
                Coords = coords,
                Email = _logOutUser
            };

            var exploreResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/explorer", exploreDto
            );

            var responseContent = await exploreResponse.Content.ReadAsStringAsync();

            // Vérifie que le statut HTTP est 404 Not Found
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, exploreResponse.StatusCode);

            // Vérifie que le message de retour contient bien ton message
            Assert.Contains("Utilisateur non connecté", responseContent);
        }

        private int[][] GetCoordsAround(int posX, int posY)
        {
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

            return coords;
        }
    }
}
