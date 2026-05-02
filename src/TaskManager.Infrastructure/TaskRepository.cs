using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskManager.Application;
using TaskManager.Domain;
using DomainTaskStatus = TaskManager.Domain.TaskStatus;

namespace TaskManager.Infrastructure;

public class TaskRepository : ITaskRepository
{
    private readonly string _connectionString;

    public TaskRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:Default"]
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    public async Task<IEnumerable<TaskItem>> GetAllByUserAsync(Guid userId)
    {
        var tasks = new List<TaskItem>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt FROM Tasks WHERE UserId = @UserId",
            connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }

        return tasks;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt FROM Tasks WHERE Id = @Id",
            connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapTask(reader);
    }

    public async Task CreateAsync(TaskItem task)
    {
        if (task.Id == Guid.Empty)
        {
            task.Id = Guid.NewGuid();
        }

        if (task.CreatedAt == default)
        {
            task.CreatedAt = DateTime.UtcNow;
        }

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "INSERT INTO Tasks (Id, Title, Description, Status, DueDate, UserId, CreatedAt) " +
            "VALUES (@Id, @Title, @Description, @Status, @DueDate, @UserId, @CreatedAt)",
            connection);

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = task.Id });
        command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = task.Title });
        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 1000) { Value = (object?)task.Description ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = task.Status.ToString() });
        command.Parameters.Add(new SqlParameter("@DueDate", SqlDbType.DateTime2) { Value = task.DueDate });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = task.UserId });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = task.CreatedAt });

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "UPDATE Tasks SET Title = @Title, Description = @Description, Status = @Status, DueDate = @DueDate WHERE Id = @Id",
            connection);

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = task.Id });
        command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = task.Title });
        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 1000) { Value = (object?)task.Description ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = task.Status.ToString() });
        command.Parameters.Add(new SqlParameter("@DueDate", SqlDbType.DateTime2) { Value = task.DueDate });

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("DELETE FROM Tasks WHERE Id = @Id", connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static TaskItem MapTask(SqlDataReader reader)
    {
        return new TaskItem
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Status = Enum.Parse<DomainTaskStatus>(reader.GetString(3)),
            DueDate = reader.GetDateTime(4),
            UserId = reader.GetGuid(5),
            CreatedAt = reader.GetDateTime(6)
        };
    }
}
