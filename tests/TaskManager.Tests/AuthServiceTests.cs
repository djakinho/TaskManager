using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Application;
using TaskManager.Domain;

namespace TaskManager.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();

    private AuthService CreateService() => new(_userRepositoryMock.Object, _configurationMock.Object);

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenUserIsNull()
    {
        var service = CreateService();

        await FluentActions.Invoking(() => service.RegisterAsync(null!, "password123"))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("User is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenEmailIsInvalid(string? email)
    {
        var service = CreateService();
        var user = new User { Email = email!, Name = "Test User" };

        await FluentActions.Invoking(() => service.RegisterAsync(user, "password123"))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Email is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenPasswordIsInvalid(string? password)
    {
        var service = CreateService();
        var user = new User { Email = "test@test.com", Name = "Test User" };

        await FluentActions.Invoking(() => service.RegisterAsync(user, password!))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenEmailAlreadyInUse()
    {
        var user = new User { Email = "duplicate@test.com", Name = "Test User" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(new User());

        var service = CreateService();

        await FluentActions.Invoking(() => service.RegisterAsync(user, "validPassword123"))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCallRepository_WhenInputIsValid()
    {
        var user = new User { Email = "newuser@test.com", Name = "New User" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        await service.RegisterAsync(user, "validPassword123");

        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == user.Email)), Times.Once);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("email@test.com", "")]
    public async Task LoginAsync_ShouldThrowValidationException_WhenFieldsAreEmpty(string email, string password)
    {
        var service = CreateService();

        await FluentActions.Invoking(() => service.LoginAsync(email, password))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Email and password are required.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowValidationException_WhenCredentialsAreWrong()
    {
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        await FluentActions.Invoking(() => service.LoginAsync("wrong@test.com", "anyPassword"))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var password = "validPassword123";
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Email = "valid@test.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test User"
        };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _configurationMock.Setup(x => x["Jwt:Secret"]).Returns("this-is-a-very-long-secret-key-32-chars-min");

        var service = CreateService();

        var token = await service.LoginAsync(user.Email, password);

        token.Should().NotBeNullOrWhiteSpace();
    }
}