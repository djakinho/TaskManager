using System.ComponentModel.DataAnnotations;
using TaskManager.Domain;
using DomainTaskStatus = TaskManager.Domain.TaskStatus;

namespace TaskManager.WebApi.Dtos;

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public DomainTaskStatus Status { get; set; }

    [Required]
    public DateTime DueDate { get; set; }
}
