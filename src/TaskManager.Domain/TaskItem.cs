namespace TaskManager.Domain;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}
