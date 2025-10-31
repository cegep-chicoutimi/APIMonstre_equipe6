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
            int personnageExp = response.Personnage.Experience;
            // ================================================================
            // STEP 4: EXPLORE TILE TO THE RIGHT
            // ================================================================
            // Les coordonnées refletent le comportement côté Web

            int targetX = startX + 1;
            int targetY = startY;
            string direction = "right";

            int[][] coords = new int[3][];
            coords[0] = new int[2];
            coords[1] = new int[2];
            coords[2] = new int[2];
            coords[0][0] = targetX;
            coords[0][1] = targetY - 1;
            coords[1][0] = targetX;
            coords[1][1] = targetY;
            coords[2][0] = targetX;
            coords[2][1] = targetY + 1;

            var exploreResponse = await _client.PostAsJsonAsync(
                $"/api/Tuiles/explorer", coords
            );

            Assert.True(exploreResponse.IsSuccessStatusCode,
                $"Explore failed: {await exploreResponse.Content.ReadAsStringAsync()}");

            var tileInfo = await exploreResponse.Content.ReadFromJsonAsync<TuileAvecInfosDto[]>();
            Assert.NotNull(tileInfo);

            bool hasMonster = tileInfo[1].Monstre != null;


            // ================================================================
            // STEP 5: MOVE TO THE RIGHT
            // ================================================================

            var moveResponse = await _client.GetAsync(
                $"/api/Personnages/{idPersonnage}/{direction}"
            );

            Assert.True(moveResponse.IsSuccessStatusCode,
                $"Move failed: {await moveResponse.Content.ReadAsStringAsync()}");

            var moveResult = await moveResponse.Content.ReadFromJsonAsync<PersonnageInfosCombatDto>();
            Assert.NotNull(moveResult);

            // ================================================================
            // STEP 6: VERIFY RESULTS
            // ================================================================

            Assert.True(moveResult.Experience != null, "Character info should be returned with every movement");
            Assert.True(moveResult.PointsVie != null, "Character info should be returned with every movement");
            Assert.True(moveResult.PositionX != null, "Character info should be returned with every movement");
            Assert.True(moveResult.PositionY != null, "Character info should be returned with every movement");

            bool levelUp = moveResult.LevelUp != null;

            if (levelUp)
            {
                Assert.True(moveResult.LevelUp.Niveau != null, "Character info should be returned with level up");
                Assert.True(moveResult.LevelUp.Defense != null, "Character info should be returned with level up");
                Assert.True(moveResult.LevelUp.Force != null, "Character info should be returned with level up");
                Assert.True(moveResult.LevelUp.PointsVieMax != null, "Character info should be returned with level up");
                Assert.True(moveResult.LevelUp.SeuilsExperienceProchainNiveau != null, "Character info should be returned with level up");
            }

            if (hasMonster)
            {
                // There was a monster - check combat results
                if (moveResult.Victoire)
                {
                    // Verify we gained XP
                    Assert.True(moveResult.Experience > 0, "Should have gained XP from defeating monster");

                    // Verify we moved to the tile (only if we won)
                    Assert.Equal(targetX, moveResult.PositionX);
                    Assert.Equal(targetY, moveResult.PositionY);

                }
                else if (moveResult.Defaite)
                {
                    // Verify we didn't gain XP
                    Assert.Equal(0, moveResult.Experience);

                    // Verify HP is full
                    Assert.Equal(moveResult.PointsVieMax, moveResult.PointsVie);

                    // Verify we're not at the target (we teleported home)
                    Assert.True(moveResult.PositionX != targetX || moveResult.PositionY != targetY,
                        "Should have been teleported away from combat location");
                }
                else
                {
                    Assert.Equal(startX, moveResult.PositionX);
                    Assert.Equal(startY, moveResult.PositionY);
                    Assert.Equal(personnageExp, moveResult.Experience);
                    Assert.True(moveResult.LevelUp == null, "Should not level up if monster not defeated");
                    Assert.False(moveResult.Defaite, "Character should not be dead if monster not defeated");
                    Assert.False(moveResult.Victoire, "Monster should not be defeated if character not dead");
                }
            }
            else
            {
                // Verify we moved
                Assert.Equal(targetX, moveResult.PositionX);
                Assert.Equal(targetY, moveResult.PositionY);

                // Verify no combat occurred
                Assert.False(moveResult.Defaite);
                Assert.False(moveResult.Victoire);
                Assert.Equal(personnageExp, moveResult.Experience);

                // Verify HP didn't change
                Assert.Equal(startHP, moveResult.PointsVie);
            }
        }
    }
}