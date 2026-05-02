using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskManager.Application;
using TaskManager.Domain;

namespace TaskManager.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:Default"]
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT Id, Name, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email",
            connection);
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200) { Value = email });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT Id, Name, Email, PasswordHash, CreatedAt FROM Users WHERE Id = @Id",
            connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task CreateAsync(User user)
    {
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }

        if (user.CreatedAt == default)
        {
            user.CreatedAt = DateTime.UtcNow;
        }

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "INSERT INTO Users (Id, Name, Email, PasswordHash, CreatedAt) " +
            "VALUES (@Id, @Name, @Email, @PasswordHash, @CreatedAt)",
            connection);

        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = user.Id });
        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = user.Name });
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200) { Value = user.Email });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 500) { Value = user.PasswordHash });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = user.CreatedAt });

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }
}
