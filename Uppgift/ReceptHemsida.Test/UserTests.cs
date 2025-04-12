// Importerar n�dv�ndiga bibliotek och namespaces
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
    // Skapar en in-memory databas f�r att simulera databas�tkomst under tester
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Anv�nder ett unikt namn f�r varje test
            .Options;

        return new ApplicationDbContext(options);
    }

    // Skapar en mockad UserManager eftersom vi inte anv�nder riktiga anv�ndare eller inloggning i testerna
    private UserManager<ApplicationUser> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new UserManager<ApplicationUser>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }

    // Testar att en anv�ndare kan h�mtas med ID n�r anv�ndaren existerar
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var user = new ApplicationUser { Id = "user1", UserName = "TestUser", Email = "test@example.com", FirstName = "FirstName", LastName = "LastName" };
        context.Users.Add(user); // L�gger till anv�ndare i databasen
        await context.SaveChangesAsync();

        var result = await userService.GetUserByIdAsync("user1");

        Assert.NotNull(result); // S�kerst�ller att en anv�ndare hittades
        Assert.Equal("TestUser", result.UserName); // Verifierar att r�tt anv�ndare h�mtades
    }

    // Testar att null returneras n�r anv�ndaren inte finns
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var result = await userService.GetUserByIdAsync("nonexistent");

        Assert.Null(result); // Ingen anv�ndare ska hittas
    }

    // Testar att s�kfunktionen hittar anv�ndare som matchar ett s�kkriterium
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

        context.Users.AddRange(users); // L�gger till flera anv�ndare i databasen
        await context.SaveChangesAsync();

        var result = await userService.SearchUsersAsync("bob");

        Assert.Single(result); // Endast en anv�ndare b�r matcha
        Assert.Equal("Bob", result.First().UserName); // Verifierar att det �r r�tt anv�ndare
    }

    // Testar att en anv�ndare kan hittas med anv�ndarnamn
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

        Assert.NotNull(result); // Anv�ndaren b�r hittas
        Assert.Equal("johnny", result.UserName); // Verifierar att det �r r�tt anv�ndare
    }

    // Testar att null returneras om ingen anv�ndare hittas med angivet anv�ndarnamn
    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_IfNoMatch()
    {
        using var context = CreateInMemoryContext();
        var userManager = GetMockUserManager();
        var userService = new UserService(context, userManager);

        var result = await userService.GetUserByUsernameAsync("not_found");

        Assert.Null(result); // Ingen anv�ndare b�r hittas
    }
}
