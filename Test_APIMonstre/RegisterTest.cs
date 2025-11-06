using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test_APIMonstre
{
    public class RegisterTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public RegisterTest(WebApplicationFactory<Program> factory)
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

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MonstreContext>();

                await dbContext.Utilisateur
                    .Where(u => u.Email == testEmail)
                    .ExecuteDeleteAsync();

                await dbContext.Personnage
                    .Where(p => p.IdUtilisateur == response.IdUtilisateur)
                    .ExecuteDeleteAsync();
            }
        }

        [Fact]
        public async Task Inscription_WithValidData_CreatesCharacterAutomatically()
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

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MonstreContext>();
                var personnage = await dbContext.Personnage
                    .FirstOrDefaultAsync(p => p.IdUtilisateur == response.IdUtilisateur);
                Assert.NotNull(personnage);
                await dbContext.Utilisateur
                    .Where(u => u.Email == testEmail)
                    .ExecuteDeleteAsync();
                await dbContext.Personnage
                    .Where(p => p.IdUtilisateur == response.IdUtilisateur)
                    .ExecuteDeleteAsync();
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Inscription_WithValidData_PlacesCharacterInRandomCity()
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

            Assert.NotNull(response);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MonstreContext>();
                var personnage = await dbContext.Personnage
                    .FirstOrDefaultAsync(p => p.IdUtilisateur == response.IdUtilisateur);
                Assert.NotNull(personnage);
                Assert.True(personnage.PositionX >= 0 && personnage.PositionY >= 0,
                    "Character position should be set to a valid location in a city.");
                Tuile tuile =  dbContext.Tuile.FirstOrDefault(t => t.PositionX == personnage.PositionX && t.PositionY == personnage.PositionY);

                Assert.NotNull(tuile);
                Assert.Equal(TypeTuile.VILLE, (TypeTuile)tuile.Type);
                await dbContext.Utilisateur
                    .Where(u => u.Email == testEmail)
                    .ExecuteDeleteAsync();
                await dbContext.Personnage
                    .Where(p => p.IdUtilisateur == response.IdUtilisateur)
                    .ExecuteDeleteAsync();
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Inscription_WithExistingEmail_ReturnsBadRequest()
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

            var firstRegisterResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                requestDto
            );

            Assert.True(firstRegisterResponse.IsSuccessStatusCode,
                $"First registration failed: {await firstRegisterResponse.Content.ReadAsStringAsync()}");

            var secondRegisterResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                requestDto
            );

            Assert.False(secondRegisterResponse.IsSuccessStatusCode,
                "Second registration with the same email should fail.");

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MonstreContext>();
                await dbContext.Utilisateur
                    .Where(u => u.Email == testEmail)
                    .ExecuteDeleteAsync();
                var registeredUser = await dbContext.Utilisateur
                    .FirstOrDefaultAsync(u => u.Email == testEmail);
                if (registeredUser != null)
                {
                    await dbContext.Personnage
                        .Where(p => p.IdUtilisateur == registeredUser.IdUtilisateur)
                        .ExecuteDeleteAsync();
                }

                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Inscription_EmptyEmail_ReturnsBadRequest()
        {
            var testEmail = "";
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
            Assert.False(registerResponse.IsSuccessStatusCode,
                "Registration with invalid email format should fail.");
        }

        [Fact]
        public async Task Inscription_WithEmptyPassword_ReturnsBadRequest()
        {
            var testEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            var testPassword = "";
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
            Assert.False(registerResponse.IsSuccessStatusCode,
                "Registration with empty password should fail.");
        }

        [Fact]
        public async Task Inscription_WithEmptyPseudo_ReturnsBadRequest()
        {
            var testEmail = $"testplayer_{Guid.NewGuid()}@test.com";
            var testPassword = "password123";
            var testPseudo = "";
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
            Assert.False(registerResponse.IsSuccessStatusCode,
                "Registration with empty pseudo should fail.");
        }
    }
}
