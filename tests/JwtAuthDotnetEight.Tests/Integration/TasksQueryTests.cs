using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http.Json;
using JwtAuthDotnetEight;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Headers;

namespace JwtAuthDotnetEight.Tests.Integration
{
    public class TasksQueryTests
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.token!);
            return client;
        }

        [Test]
        public async Task Get_With_Status_And_Assignee_Filters()
        {
            var client = await AuthedClient(_factory);

            // Create two tasks with different assignees and statuses
            var t1 = await (await client.PostAsJsonAsync("/api/tasks", new { title = "q1", priority = "LOW" })).Content.ReadFromJsonAsync<TaskDto>();
            var t2 = await (await client.PostAsJsonAsync("/api/tasks", new { title = "q2", priority = "LOW" })).Content.ReadFromJsonAsync<TaskDto>();

            // Assign t2 to user 1 and move to IN_PROGRESS
            var upd2 = await (await client.PutAsJsonAsync($"/api/tasks/{t2!.id}", new { assigneeId = 1, status = "IN_PROGRESS" })).Content.ReadFromJsonAsync<TaskDto>();
            Assert.That(upd2!.assigneeId, Is.EqualTo(1));
            Assert.That(upd2.status, Is.EqualTo("IN_PROGRESS"));

            // Filter by status
            var sOnly = await client.GetFromJsonAsync<TaskDto[]>("/api/tasks?status=IN_PROGRESS");
            Assert.That(sOnly, Is.Not.Null);
            Assert.That(sOnly!.Any(t => t.id == upd2.id), Is.True);
            Assert.That((sOnly ?? Array.Empty<TaskDto>()).Any(t => t.id == t1!.id), Is.False);

            // Filter by assignee
            var aOnly = await client.GetFromJsonAsync<TaskDto[]>("/api/tasks?assignee=1");
            Assert.That(aOnly, Is.Not.Null);
            Assert.That(aOnly!.Any(t => t.id == upd2.id), Is.True);
            Assert.That((aOnly ?? Array.Empty<TaskDto>()).Any(t => t.id == t1!.id), Is.False);

            // Cleanup
            await client.DeleteAsync($"/api/tasks/{t1!.id}");
            await client.DeleteAsync($"/api/tasks/{t2!.id}");
        }

        [Test]
        public async Task Get_With_Search_Filter()
        {
            var client = await AuthedClient(_factory);

            // Create tasks with distinct terms in title/description
            var a = await (await client.PostAsJsonAsync("/api/tasks", new { title = "Alpha Task", description = "first term apple", priority = "LOW" })).Content.ReadFromJsonAsync<TaskDto>();
            var b = await (await client.PostAsJsonAsync("/api/tasks", new { title = "Beta Work", description = "contains banana", priority = "LOW" })).Content.ReadFromJsonAsync<TaskDto>();
            var c = await (await client.PostAsJsonAsync("/api/tasks", new { title = "Gamma", description = "no fruit here", priority = "LOW" })).Content.ReadFromJsonAsync<TaskDto>();

            // Search by title fragment
            var r1 = await client.GetFromJsonAsync<TaskDto[]>("/api/tasks?search=Alpha");
            Assert.That(r1!.Any(t => t.id == a!.id), Is.True);
            Assert.That(r1!.Any(t => t.id == b!.id || t.id == c!.id), Is.False);

            // Search by description fragment
            var r2 = await client.GetFromJsonAsync<TaskDto[]>("/api/tasks?search=banana");
            Assert.That(r2!.Any(t => t.id == b!.id), Is.True);
            Assert.That(r2!.Any(t => t.id == a!.id || t.id == c!.id), Is.False);

            // Cleanup
            await client.DeleteAsync($"/api/tasks/{a!.id}");
            await client.DeleteAsync($"/api/tasks/{b!.id}");
            await client.DeleteAsync($"/api/tasks/{c!.id}");
        }

        private record LoginResponse(string? token);
        private record TaskDto(int id, string title, string? description, string status, string? priority, int? assigneeId, DateTime? updatedAt);
    }
}
