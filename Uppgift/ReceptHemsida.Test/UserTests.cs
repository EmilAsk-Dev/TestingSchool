// Importerar nödvändiga bibliotek och namespaces
using Xunit;
using Microsoft.EntityFrameworkCore;
using ReceptHemsida.Data;
using ReceptHemsida.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class UserServiceTests
{
    // Skapar en in-memory databas för att simulera databasåtkomst under tester
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Använder ett unikt namn för varje test
            .Options;

        return new ApplicationDbContext(options);
    }

    // Skapar en mockad UserManager eftersom vi inte använder riktiga användare eller inloggning i testerna
    private UserManager<ApplicationUser> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new UserManager<ApplicationUser>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }

    // Testar att en användare kan hämtas med ID när användaren existerar
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var user = new ApplicationUser { Id = "user1", UserName = "TestUser", Email = "test@example.com", FirstName = "FirstName", LastName = "LastName" };
        context.Users.Add(user); // Lägger till användare i databasen
        await context.SaveChangesAsync();

        var result = await userService.GetUserByIdAsync("user1");

        Assert.NotNull(result); // Säkerställer att en användare hittades
        Assert.Equal("TestUser", result.UserName); // Verifierar att rätt användare hämtades
    }

    // Testar att null returneras när användaren inte finns
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var result = await userService.GetUserByIdAsync("nonexistent");

        Assert.Null(result); // Ingen användare ska hittas
    }

    // Testar att sökfunktionen hittar användare som matchar ett sökkriterium
    [Fact]
    public async Task SearchUsersAsync_ShouldReturnMatchingUsers()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "Alice", Email = "alice@example.com", FirstName="FirstName", LastName="LastName" },
            new ApplicationUser { Id = "2", UserName = "Bob", Email = "bob@example.com" , FirstName="FirstName", LastName="LastName"},
            new ApplicationUser { Id = "3", UserName = "Charlie", Email = "charlie@example.com",  FirstName="FirstName", LastName="LastName" }
        };

        context.Users.AddRange(users); // Lägger till flera användare i databasen
        await context.SaveChangesAsync();

        var result = await userService.SearchUsersAsync("bob");

        Assert.Single(result); // Endast en användare bör matcha
        Assert.Equal("Bob", result.First().UserName); // Verifierar att det är rätt användare
    }

    // Testar att en användare kan hittas med användarnamn
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnCorrectUser()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var user = new ApplicationUser { Id = "1", UserName = "johnny", Email = "johnny@example.com", FirstName = "FirstName", LastName = "LastName" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await userService.GetUserByUsernameAsync("johnny");

        Assert.NotNull(result); // Användaren bör hittas
        Assert.Equal("johnny", result.UserName); // Verifierar att det är rätt användare
    }

    // Testar att null returneras om ingen användare hittas med angivet användarnamn
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_IfNoMatch()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var result = await userService.GetUserByUsernameAsync("not_found");

        Assert.Null(result); // Ingen användare bör hittas
    }
}
