using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;

namespace Test_APIMonstre
{
    //using Microsoft.VisualStudio.TestPlatform.TestHost;
    //no clue si ca marche

    public class SimpleGameFlowTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public SimpleGameFlowTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task CompleteGameFlow_RegisterLoginAndMove_Works()
        {
            // Wait for initialization services to complete
            await Task.Delay(2000); // Give services time to initialize map

            // Generate unique email for this test run
            var testEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            var testPassword = "password123";
            var testPseudo = "TestHero";

            // ================================================================
            // STEP 1: REGISTER
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
            // STEP 2: LOGIN
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

            Assert.True(loginResponse.IsSuccessStatusCode,
                $"Login failed: {await loginResponse.Content.ReadAsStringAsync()}");


            // ================================================================
            // STEP 3: GET CHARACTER INFO
            // ================================================================



            LoginResponseDto response = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(response.Personnage);

            // Store starting position
            int startX = response.Personnage.PositionX;
            int startY = response.Personnage.PositionY;
            int startHP = response.Personnage.PointsVie;
            int idPersonnage = response.Personnage.IdPersonnage;

            // ================================================================
            // STEP 4: EXPLORE TILE TO THE RIGHT
            // ================================================================
            // Les coordonnées refletent le comportement côté Web

            int targetX = startX;
            int targetY = startY + 1;
            string direction = "right";

            var exploreResponse = await _client.PostAsJsonAsync(
                $"/api/Personnages/{idPersonnage}/{direction}",
                testEmail
            );

            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Explore failed: {await exploreResponse.Content.ReadAsStringAsync()}");

            var tileInfo = await exploreResponse.Content.ReadFromJsonAsync<TuileAvecInfosDto>();
            Assert.NotNull(tileInfo);

            bool hasMonster = tileInfo.Monstre != null;


            // ================================================================
            // STEP 5: MOVE TO THE RIGHT
            // ================================================================

            var moveResponse = await _client.PostAsJsonAsync(
                $"/api/Personnages/Deplacement/{targetX}/{targetY}",
                testEmail
            );

            Assert.True(moveResponse.IsSuccessStatusCode,
                $"Move failed: {await moveResponse.Content.ReadAsStringAsync()}");

            var moveResult = await moveResponse.Content.ReadFromJsonAsync<PersonnageInfosCombatDto>();
            Assert.NotNull(moveResult);

            // ================================================================
            // STEP 6: VERIFY RESULTS
            // ================================================================

            Assert.True(moveResult.Experience != null, "Character info should be returned with every movement");
            Assert.True(moveResult.LevelUp != null, "Character info should be returned with every movement");
            Assert.True(moveResult. != null, "Character info should be returned with every movement");
            Assert.True(moveResult.Experience != null, "Character info should be returned with every movement");
            Assert.True(moveResult.Experience != null, "Character info should be returned with every movement");


            if (hasMonster)
            {
                // There was a monster - check combat results
                if (moveResult.monstreVaincu)
                {
                    // Verify we gained XP
                    Assert.True(moveResult.xpGained > 0, "Should have gained XP from defeating monster");

                    // Verify we moved to the tile (only if we won)
                    Assert.Equal(targetX, moveResult.finalX);
                    Assert.Equal(targetY, moveResult.finalY);

                }
                else if (moveResult.personnageDead)
                {
                    // Verify we didn't gain XP
                    Assert.Equal(0, moveResult.xpGained);

                    // Verify HP is full
                    Assert.Equal(moveResult.personnage.PointsVieMax, moveResult.personnage.PointsVieActuels);

                    // Verify we're not at the target (we teleported home)
                    Assert.True(moveResult.finalX != targetX || moveResult.finalY != targetY,
                        "Should have been teleported away from combat location");
                }
                else
                {
                    Assert.Equal(startX, moveResult.finalX);
                    Assert.Equal(startY, moveResult.finalY);
                    Assert.Equal(0, moveResult.xpGained);
                    Assert.True(moveResult.levelUp == null, "Should not level up if monster not defeated");
                    Assert.False(moveResult.personnageDead, "Character should not be dead if monster not defeated");
                    Assert.False(moveResult.monstreVaincu, "Monster should not be defeated if character not dead");
                }
            }
            else
            {
                // Verify we moved
                Assert.Equal(targetX, moveResult.finalX);
                Assert.Equal(targetY, moveResult.finalY);

                // Verify no combat occurred
                Assert.False(moveResult.monstreVaincu);
                Assert.False(moveResult.personnageDead);
                Assert.Equal(0, moveResult.xpGained);

                // Verify HP didn't change
                Assert.Equal(startHP, moveResult.personnage.PointsVieActuels);
            }
        }
}