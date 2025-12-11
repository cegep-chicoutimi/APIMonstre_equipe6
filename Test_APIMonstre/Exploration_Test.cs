using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Test_APIMonstre
{
    public class Exploration_Test : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public Exploration_Test(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();            
        }

        [Fact]
        public async Task ExplorerTuile_Tests()
        {
            // Wait for initialization services to complete
            // Give services time to initialize map
            await Task.Delay(2000); 
            string testEmail = $"testplayer_Explore@test.com";
            string testPassword = "password123";
            string testPseudo = "TestExplore";

            // ================================================================
            // LOGIN
            // ================================================================

            var loginDto = new LoginRequestDto
            {
                Email = testEmail,
                Password = testPassword
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/login",
                loginDto
            );

            // verify login succeeded
            Assert.True(loginResponse.IsSuccessStatusCode,
                $"Login failed: {await loginResponse.Content.ReadAsStringAsync()}");

            // ================================================================
            // GET CHARACTER INFO
            // ================================================================

            LoginResponseDto response = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(response.Personnage);

            // Store character info
            var perso = response.Personnage;

            // Test de succès pour l'epxloration des tuiles 
            // ================================================================
            // ExplorerTuile_WithinRange_ReturnsTuileData
            // ================================================================
            int tuileInRangeX = perso.PositionX + 1;
            int tuileInRangeY = perso.PositionY + 1;

            var tuileInRange = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileInRangeX}/{tuileInRangeY}", testEmail
            );

            Assert.True(tuileInRange.IsSuccessStatusCode,
                $"Exploration failed: {await tuileInRange.Content.ReadAsStringAsync()}");

            // ================================================================
            // ExplorerTuile_WithinRange_ReturnsNullMonsterIfEmpty
            // ================================================================

            Assert.Null(tuileInRange.Content.ReadFromJsonAsync<TuileAvecInfosDto>().Result.Monstre);

            // ================================================================
            // ExplorerTuile_WithinRange_ReturnsMonsterIfPresent
            // ================================================================

            int tuileWithMonsterInRangeX = perso.PositionX;
            int tuileWithMonsterInRangeY = perso.PositionY + 1;

            var tuileWithMonster = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileWithMonsterInRangeX}/{tuileWithMonsterInRangeY}", testEmail
            );

            Assert.True(tuileWithMonster.IsSuccessStatusCode,
                $"Exploration failed: {await tuileWithMonster.Content.ReadAsStringAsync()}");

            Assert.NotNull(tuileWithMonster.Content.ReadFromJsonAsync<TuileAvecInfosDto>().Result.Monstre);

            // ================================================================
            // ExplorerTuile_TwoStepsAway_ReturnsSuccess
            // ================================================================

            int tuileTwoStepsAwayX = perso.PositionX + 2;
            int tuileTwoStepsAwayY = perso.PositionY + 2;

            var tuileTwoStepsAway = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileTwoStepsAwayX}/{tuileTwoStepsAwayY}", testEmail
            );

            Assert.True(tuileTwoStepsAway.IsSuccessStatusCode,
                $"Exploration failed: {await tuileTwoStepsAway.Content.ReadAsStringAsync()}");

            // Test d'erreur pour l'epxloration des tuiles
            // ================================================================
            // ExplorerTuile_FiveStepsAway_ReturnsForbidden
            // ================================================================

            int tuileFiveStepsAwayX = perso.PositionX + 5;
            int tuileFiveStepsAwayY = perso.PositionY + 5;

            var tuileFiveStepsAway = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileFiveStepsAwayX}/{tuileFiveStepsAwayY}", testEmail
            );

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, tuileFiveStepsAway.StatusCode);

            Assert.True(tuileFiveStepsAway.StatusCode == System.Net.HttpStatusCode.Forbidden,
                $"Expected Forbidden but got: {await tuileFiveStepsAway.Content.ReadAsStringAsync()}");

            // ================================================================
            // ExplorerTuile_BeyondMapBoundaries_ReturnsForbidden
            // ================================================================

            int tuileBeyondMapX = perso.PositionX + 50;
            int tuileBeyondMapY = perso.PositionY + 50;

            var tuileBeyondMap = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileBeyondMapX}/{tuileBeyondMapY}", testEmail
            );

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, tuileBeyondMap.StatusCode);

            Assert.True(tuileBeyondMap.StatusCode == System.Net.HttpStatusCode.Forbidden,
                $"Expected Forbidden but got: {await tuileBeyondMap.Content.ReadAsStringAsync()}");

            // ================================================================
            // ExplorerTuile_NegativeCoordinates_ReturnsForbidden
            // ================================================================

            int tuileNegativeX = perso.PositionX - 50;
            int tuileNegativeY = perso.PositionY - 50;

            var tuileNegative = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileNegativeX}/{tuileNegativeY}", testEmail
            );

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, tuileNegative.StatusCode);

            Assert.True(tuileNegative.StatusCode == System.Net.HttpStatusCode.Forbidden,
                $"Expected Forbidden but got: {await tuileNegative.Content.ReadAsStringAsync()}");

        }
        [Fact]
        public async Task ExplorerTuile_WithoutAuthentication_ReturnsNotFound()
        {
            // on retourne un not found parce qu'on cherche un utilisateur et on ne le trouve pas 

            int tuileX =  10;
            int tuileY =  10;

            var tuileNegative = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileX}/{tuileY}", ""
            );

            Assert.Equal(System.Net.HttpStatusCode.NotFound, tuileNegative.StatusCode);

            Assert.True(tuileNegative.StatusCode == System.Net.HttpStatusCode.NotFound,
                $"Expected Not Found but got: {await tuileNegative.Content.ReadAsStringAsync()}");
        }
        [Fact]
        public async Task ExplorerTuile_WithDisconnectedUser_ReturnsForbidden()
        {
            // New user registration

            var testEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            var testPassword = "password123";
            var testPseudo = "TestHero";

            // ================================================================
            // REGISTER
            // ================================================================

            var registerDto = new RegisterRequestDto
            {
                Email = testEmail,
                Password = testPassword,
                Pseudo = testPseudo
            };

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                registerDto
            );

            // Verify registration succeeded
            Assert.True(registerResponse.IsSuccessStatusCode,
                $"Registration failed: {await registerResponse.Content.ReadAsStringAsync()}");

            // ================================================================
            // EXPLORER TUILE WITHOUT LOGIN
            // ================================================================

            int tuileX = 10;
            int tuileY = 10;

            var tuileNegative = await _client.PostAsJsonAsync(
                $"/api/Tuiles/{tuileX}/{tuileY}", testEmail
            );

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, tuileNegative.StatusCode);

            Assert.True(tuileNegative.StatusCode == System.Net.HttpStatusCode.Forbidden,
                $"Expected Forbidden but got: {await tuileNegative.Content.ReadAsStringAsync()}");
        }
    }
}
