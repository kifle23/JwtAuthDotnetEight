using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using JwtAuthDotnetEight;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Headers;

namespace JwtAuthDotnetEight.Tests.Integration
{
    public class TasksApiTests
    {
        private WebApplicationFactory<Program> _factory = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [TearDown]
        public void Teardown()
        {
            _factory.Dispose();
        }

    private static async System.Threading.Tasks.Task<HttpClient> AuthedClient(WebApplicationFactory<Program> factory)
        {
            var client = factory.CreateClient();
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "password" });
            loginRes.EnsureSuccessStatusCode();
            var login = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.That(login, Is.Not.Null);
            Assert.That(login!.token, Is.Not.Null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.token!);
            return client;
        }

        [Test]
        public async Task Create_Update_Delete_Works()
        {
            var client = await AuthedClient(_factory);

            // Create
            var createRes = await client.PostAsJsonAsync("/api/tasks", new { title = "int test", description = "d", priority = "LOW" });
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<TaskDto>();
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.id, Is.GreaterThan(0));

            // Update
            var updateRes = await client.PutAsJsonAsync($"/api/tasks/{created.id}", new { status = "IN_PROGRESS" });
            updateRes.EnsureSuccessStatusCode();
            var updated = await updateRes.Content.ReadFromJsonAsync<TaskDto>();
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.status, Is.EqualTo("IN_PROGRESS"));

            // Delete
            var delRes = await client.DeleteAsync($"/api/tasks/{created.id}");
            Assert.That((int)delRes.StatusCode, Is.EqualTo(204));
        }

        private record LoginResponse(string? token);
        private record TaskDto(int id, string title, string? description, string status, string? priority, int? assigneeId, DateTime? updatedAt);
    }
}
