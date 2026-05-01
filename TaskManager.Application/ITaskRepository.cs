using TaskManager.Domain;

namespace TaskManager.Application;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllByUserAsync(Guid userId);
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task CreateAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(Guid id);
}
