using System.ComponentModel.DataAnnotations;
using TaskManager.Domain;
using DomainTaskStatus = TaskManager.Domain.TaskStatus;

namespace TaskManager.Application;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task CreateAsync(TaskItem task, Guid userId)
    {
        if (task is null)
        {
            throw new ValidationException("Task is required.");
        }

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new ValidationException("Title is required.");
        }

        if (task.Title.Length > 200)
        {
            throw new ValidationException("Title must be 200 characters or fewer.");
        }

        if (!Enum.IsDefined(typeof(DomainTaskStatus), task.Status))
        {
            throw new ValidationException("Status is invalid.");
        }

        if (task.DueDate <= DateTime.UtcNow)
        {
            throw new ValidationException("DueDate must be in the future.");
        }

        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;

        await _taskRepository.CreateAsync(task);
    }

    public Task<IEnumerable<TaskItem>> GetAllAsync(Guid userId)
    {
        return _taskRepository.GetAllByUserAsync(userId);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null || task.UserId != userId)
        {
            return null;
        }

        return task;
    }

    public async Task UpdateAsync(TaskItem task, Guid userId)
    {
        if (task is null)
        {
            throw new ValidationException("Task is required.");
        }

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new ValidationException("Title is required.");
        }

        if (task.Title.Length > 200)
        {
            throw new ValidationException("Title must be 200 characters or fewer.");
        }

        if (!Enum.IsDefined(typeof(DomainTaskStatus), task.Status))
        {
            throw new ValidationException("Status is invalid.");
        }

        if (task.DueDate <= DateTime.UtcNow)
        {
            throw new ValidationException("DueDate must be in the future.");
        }

        var existingTask = await _taskRepository.GetByIdAsync(task.Id);
        if (existingTask is null || existingTask.UserId != userId)
        {
            throw new NotFoundException("Task not found.");
        }

        task.UserId = userId;
        await _taskRepository.UpdateAsync(task);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask is null || existingTask.UserId != userId)
        {
            throw new NotFoundException("Task not found.");
        }

        await _taskRepository.DeleteAsync(id);
    }
}
