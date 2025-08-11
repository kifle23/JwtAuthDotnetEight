using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Repositories;
using JwtAuthDotnetEight.Services;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.SignalR;
using JwtAuthDotnetEight.Hubs;

namespace JwtAuthDotnetEight.Tests
{
    public class TaskServiceTests
    {
        [Test]
    public void UpdateAsync_InvalidTransition_Throws()
        {
    var userRepo = new Mock<IUserRepository>();
    var taskRepo = new Mock<ITaskRepository>();
    var hub = new Mock<IHubContext<TaskHub>>();
    var clients = new Mock<IHubClients>();
    var proxy = new Mock<IClientProxy>();
    clients.Setup(c => c.All).Returns(proxy.Object);
    hub.Setup(h => h.Clients).Returns(clients.Object);
    proxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default)).Returns(System.Threading.Tasks.Task.CompletedTask);
    var svc = new TaskService(userRepo.Object, taskRepo.Object, hub.Object);
        var task = new TaskItem { Id = 1, Title = "t", CreatorId = 1, Creator = new User{ Id=1, Username="u", Email="e", PasswordHash="p" }, Status = JwtAuthDotnetEight.Models.TaskStatus.TODO };
        taskRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateAsync(1, null, null, JwtAuthDotnetEight.Models.TaskStatus.DONE, null, null));
        }

        [Test]
        public async Task CreateAsync_CreatesTask()
        {
            var userRepo = new Mock<IUserRepository>();
            var taskRepo = new Mock<ITaskRepository>();
            var hub = new Mock<IHubContext<TaskHub>>();
            var clients = new Mock<IHubClients>();
            var proxy = new Mock<IClientProxy>();
            clients.Setup(c => c.All).Returns(proxy.Object);
            hub.Setup(h => h.Clients).Returns(clients.Object);
            proxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default)).Returns(System.Threading.Tasks.Task.CompletedTask);
            var svc = new TaskService(userRepo.Object, taskRepo.Object, hub.Object);
            var creator = new User { Id = 2, Username = "creator", Email = "c@example.com", PasswordHash = "ph" };
            userRepo.Setup(r => r.GetUserByUsernameAsync("creator")).ReturnsAsync(creator);
            taskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

            var created = await svc.CreateAsync("creator", "title", "desc", JwtAuthDotnetEight.Models.TaskPriority.LOW, null);

            Assert.That(created.Title, Is.EqualTo("title"));
            Assert.That(created.CreatorId, Is.EqualTo(creator.Id));
        }
    }
}
