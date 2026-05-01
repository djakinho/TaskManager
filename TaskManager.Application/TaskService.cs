using TaskManager.Domain;

namespace TaskManager.Application;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public Task CreateAsync(TaskItem task, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskItem>> GetAllAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TaskItem task, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, Guid userId)
    {
        throw new NotImplementedException();
    }
}
