using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Moq;
using TaskManager.Application;
using TaskManager.Domain;
using DomainTaskStatus = TaskManager.Domain.TaskStatus;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();

    private TaskService CreateService() => new(_taskRepositoryMock.Object);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_ShouldThrowValidationException_WhenTitleIsEmpty(string title)
    {
        var service = CreateService();
        var task = new TaskItem
        {
            Title = title,
            Description = "Description",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        await FluentActions.Invoking(() => service.CreateAsync(task, Guid.NewGuid()))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenDueDateIsInThePast()
    {
        var service = CreateService();
        var task = new TaskItem
        {
            Title = "Valid title",
            Description = "Description",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        await FluentActions.Invoking(() => service.CreateAsync(task, Guid.NewGuid()))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepository_WhenInputIsValid()
    {
        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Title = "Valid title",
            Description = "Description",
            Status = DomainTaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        await service.CreateAsync(task, userId);

        _taskRepositoryMock.Verify(x => x.CreateAsync(It.Is<TaskItem>(t => t.Title == task.Title && t.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyTasksForGivenUserId()
    {
        var userId = Guid.NewGuid();
        var expectedTasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Description = "A", Status = DomainTaskStatus.Todo, DueDate = DateTime.UtcNow.AddDays(2), UserId = userId },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Description = "B", Status = DomainTaskStatus.Done, DueDate = DateTime.UtcNow.AddDays(3), UserId = userId }
        };

        _taskRepositoryMock
            .Setup(x => x.GetAllByUserAsync(userId))
            .ReturnsAsync(expectedTasks);

        var service = CreateService();

        var result = await service.GetAllAsync(userId);

        result.Should().BeEquivalentTo(expectedTasks);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenTaskBelongsToDifferentUser()
    {
        var userId = Guid.NewGuid();
        var taskOwnerId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Owned task",
            Description = "Owned by another user",
            Status = DomainTaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(2),
            UserId = taskOwnerId
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var service = CreateService();

        await FluentActions.Invoking(() => service.DeleteAsync(task.Id, userId))
            .Should().ThrowAsync<NotFoundException>();
    }
}
