using JwtAuthDotnetEight;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using System.Collections.Generic;

namespace JwtAuthDotnetEight.Tests.Integration
{
    public class TasksHubTests
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

        [Test]
        public async Task Hub_Receives_Created_And_Updated()
        {
            var client = _factory.CreateClient();
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "password" });
            loginRes.EnsureSuccessStatusCode();
            var login = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.That(login?.token, Is.Not.Null);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.token!);

            var preCheck = await client.GetAsync("/api/tasks");
            Assert.That((int)preCheck.StatusCode, Is.EqualTo(200), $"Pre hub GET /api/tasks failed: {(int)preCheck.StatusCode} {await preCheck.Content.ReadAsStringAsync()}");

            var serverUri = client.BaseAddress!.ToString().TrimEnd('/');
            var hubUrl = serverUri + "/hub/tasks";
            var hubUrlWithToken = hubUrl + $"?access_token={login!.token!}";

            var receivedCreated = new System.Threading.Tasks.TaskCompletionSource<object>();
            var receivedUpdated = new System.Threading.Tasks.TaskCompletionSource<object>();

            var handler = _factory.Server.CreateHandler();
                var connection = new HubConnectionBuilder()
                    .WithUrl(hubUrlWithToken, options => {
                        options.AccessTokenProvider = () => Task.FromResult(login!.token!)!;
                        options.Headers.Add("Authorization", $"Bearer {login!.token!}");
                        options.Transports = HttpTransportType.LongPolling;
                        options.HttpMessageHandlerFactory = _ => handler;
                    })
                    .WithAutomaticReconnect()
                    .Build();

            connection.On<object>("task:created", _ => receivedCreated.TrySetResult(new object()));
            connection.On<object>("task:updated", _ => receivedUpdated.TrySetResult(new object()));

            await connection.StartAsync();
            await Task.Delay(100);

            var postCheck = await client.GetAsync("/api/tasks");
            Assert.That((int)postCheck.StatusCode, Is.EqualTo(200), $"Post hub GET /api/tasks failed: {(int)postCheck.StatusCode} {await postCheck.Content.ReadAsStringAsync()}");

            // Trigger create
            var createRes = await client.PostAsJsonAsync("/api/tasks", new { title = "hub test", description = "d", priority = "LOW" });
            Assert.That((int)createRes.StatusCode, Is.EqualTo(200), $"POST /api/tasks failed: {(int)createRes.StatusCode} {await createRes.Content.ReadAsStringAsync()}");

            // Trigger update
            var created = await createRes.Content.ReadFromJsonAsync<TaskDto>();
            var updateRes = await client.PutAsJsonAsync($"/api/tasks/{created!.id}", new { status = "IN_PROGRESS" });
            Assert.That((int)updateRes.StatusCode, Is.EqualTo(200), $"PUT /api/tasks/{{id}} failed: {(int)updateRes.StatusCode} {await updateRes.Content.ReadAsStringAsync()}");

            // Wait for events
            var createdOk = await Task.WhenAny(receivedCreated.Task, Task.Delay(TimeSpan.FromSeconds(5))) == receivedCreated.Task;
            var updatedOk = await Task.WhenAny(receivedUpdated.Task, Task.Delay(TimeSpan.FromSeconds(5))) == receivedUpdated.Task;

            Assert.That(createdOk, Is.True, "Did not receive task:created event");
            Assert.That(updatedOk, Is.True, "Did not receive task:updated event");

            await connection.StopAsync();
        }

        [Test]
        public async Task Hub_Receives_Deleted()
        {
            var client = _factory.CreateClient();
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "password" });
            loginRes.EnsureSuccessStatusCode();
            var login = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.That(login?.token, Is.Not.Null);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.token!);

            var preCheck = await client.GetAsync("/api/tasks");
            Assert.That((int)preCheck.StatusCode, Is.EqualTo(200), $"Pre hub GET /api/tasks failed: {(int)preCheck.StatusCode} {await preCheck.Content.ReadAsStringAsync()}");

            var serverUri = client.BaseAddress!.ToString().TrimEnd('/');
            var hubUrl = serverUri + "/hub/tasks";
            var hubUrlWithToken = hubUrl + $"?access_token={login!.token!}";

            var receivedDeleted = new TaskCompletionSource<int>();

            var handler = _factory.Server.CreateHandler();
                var connection = new HubConnectionBuilder()
                    .WithUrl(hubUrlWithToken, options => {
                        options.AccessTokenProvider = () => Task.FromResult(login!.token!)!;
                        options.Headers.Add("Authorization", $"Bearer {login!.token!}");
                        options.Transports = HttpTransportType.LongPolling;
                        options.HttpMessageHandlerFactory = _ => handler;
                    })
                    .WithAutomaticReconnect()
                    .Build();

            connection.On<System.Text.Json.JsonElement>("task:deleted", payload =>
            {
                try
                {
                    if (payload.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        payload.TryGetProperty("id", out var idEl) &&
                        idEl.TryGetInt32(out var id))
                    {
                        receivedDeleted.TrySetResult(id);
                    }
                }
                catch { }
            });

            connection.On<object>("task:deleted", payload =>
            {
                try
                {
                    switch (payload)
                    {
                        case System.Text.Json.JsonElement el when el.ValueKind == System.Text.Json.JsonValueKind.Object:
                            if (el.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var id))
                                receivedDeleted.TrySetResult(id);
                            break;
                        case IDictionary<string, object> dict:
                            if (dict.TryGetValue("id", out var idVal))
                            {
                                if (idVal is int i) receivedDeleted.TrySetResult(i);
                                else if (idVal is long l) receivedDeleted.TrySetResult((int)l);
                                else if (idVal is string s && int.TryParse(s, out var si)) receivedDeleted.TrySetResult(si);
                            }
                            break;
                    }
                }
                catch { }
            });

            await connection.StartAsync();

            var postCheck = await client.GetAsync("/api/tasks");
            Assert.That((int)postCheck.StatusCode, Is.EqualTo(200), $"Post hub GET /api/tasks failed: {(int)postCheck.StatusCode} {await postCheck.Content.ReadAsStringAsync()}");

            // Create then delete
            var createRes = await client.PostAsJsonAsync("/api/tasks", new { title = "hub del", description = "d", priority = "LOW" });
            Assert.That((int)createRes.StatusCode, Is.EqualTo(200), $"POST /api/tasks failed: {(int)createRes.StatusCode} {await createRes.Content.ReadAsStringAsync()}");
            var created = await createRes.Content.ReadFromJsonAsync<TaskDto>();
            await Task.Delay(100); // allow created event to flow
            var delRes = await client.DeleteAsync($"/api/tasks/{created!.id}");
            Assert.That((int)delRes.StatusCode, Is.EqualTo(204));

            var gotDeleted = await Task.WhenAny(receivedDeleted.Task, Task.Delay(TimeSpan.FromSeconds(15))) == receivedDeleted.Task;
            Assert.That(gotDeleted, Is.True, "Did not receive task:deleted event");
            Assert.That(receivedDeleted.Task.Result, Is.EqualTo(created!.id));

            await connection.StopAsync();
        }

        private record LoginResponse(string? token);
        private record TaskDto(int id, string title, string? description, string status, string? priority, int? assigneeId, DateTime? updatedAt);
    }
}
