using Microsoft.EntityFrameworkCore;
using ReceptHemsida.Data;
using ReceptHemsida.Models;
using ReceptHemsida.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class FavoriteServiceTests
{
    // Skapar en in-memory databas för att kunna testa utan riktig databas
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikt databasnamn för varje test
            .Options;

        return new ApplicationDbContext(options);
    }

    // Testar att hämta favorit-recept när användaren har favoriter
    [Fact]
    public async Task GetFavoriteRecipesAsync_ShouldReturnFavoriteRecipes_WhenUserHasFavorites()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user1";
        var recipe = new Recipe
        {
            Id = "recipe1",
            Title = "Chocolate Cake",
            Description = "A delicious chocolate cake.",
            Difficulty = "Medium",
            UserId = userId // UserId måste sättas eftersom det är ett krav
        };

        var favorite = new Favorite
        {
            UserId = userId,
            RecipeId = recipe.Id,
            Recipe = recipe
        };

        context.Favorites.Add(favorite);
        context.Recipes.Add(recipe); // Lägg till receptet i databasen
        await context.SaveChangesAsync();

        var result = await favoriteService.GetFavoriteRecipesAsync(userId);

        Assert.NotNull(result); // Resultatet får inte vara null
        Assert.Single(result); // Ska finnas ett favorit-recept
        Assert.Equal("Chocolate Cake", result[0].Title); // Kontrollera titeln
    }

    // Testar att hämta favoriter när användaren inte har några
    [Fact]
    public async Task GetFavoriteRecipesAsync_ShouldReturnEmptyList_WhenUserHasNoFavorites()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user2";
        var result = await favoriteService.GetFavoriteRecipesAsync(userId);

        Assert.NotNull(result);
        Assert.Empty(result); // Ska vara tomt
    }

    // Testar att lägga till en favorit
    [Fact]
    public async Task AddFavoriteAsync_ShouldAddFavorite_WhenValidFavorite()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user1";
        var recipe = new Recipe { Id = "recipe1", Title = "Chocolate Cake" };
        var favorite = new Favorite { UserId = userId, RecipeId = recipe.Id, Recipe = recipe };

        await favoriteService.AddFavoriteAsync(favorite);

        var result = await context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipe.Id);
        Assert.NotNull(result); // Kontrollera att den lades till
        Assert.Equal("user1", result.UserId);
        Assert.Equal("recipe1", result.RecipeId);
    }

    // Testar att ta bort en favorit
    [Fact]
    public async Task RemoveFavoriteAsync_ShouldRemoveFavorite_WhenFavoriteExists()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user1";
        var recipe = new Recipe
        {
            Id = "recipe1",
            Title = "Chocolate Cake",
            Description = "A delicious chocolate cake.",
            Difficulty = "Medium",
            UserId = userId
        };

        var favorite = new Favorite
        {
            UserId = userId,
            RecipeId = recipe.Id,
            Recipe = recipe
        };

        context.Favorites.Add(favorite);
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();

        await favoriteService.RemoveFavoriteAsync(userId, recipe.Id);

        var result = await context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipe.Id);
        Assert.Null(result); // Favoriten ska vara borttagen
    }

    // Testar om ett recept är favoritmarkerat
    [Fact]
    public async Task IsRecipeFavoritedAsync_ShouldReturnTrue_WhenRecipeIsFavorited()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user1";
        var recipe = new Recipe
        {
            Id = "recipe1",
            Title = "Chocolate Cake",
            Description = "A delicious chocolate cake.",
            Difficulty = "Medium",
            UserId = userId
        };

        var favorite = new Favorite
        {
            UserId = userId,
            RecipeId = recipe.Id,
            Recipe = recipe
        };

        context.Favorites.Add(favorite);
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();

        var result = await favoriteService.IsRecipeFavoritedAsync(userId, recipe.Id);

        Assert.True(result); // Ska returnera true
    }

    // Testar om ett recept inte är favoritmarkerat
    [Fact]
    public async Task IsRecipeFavoritedAsync_ShouldReturnFalse_WhenRecipeIsNotFavorited()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var userId = "user2";
        var recipeId = "recipe1";

        var result = await favoriteService.IsRecipeFavoritedAsync(userId, recipeId);

        Assert.False(result); // Ska returnera false
    }

    // Testar att hämta en användares favoritrecept
    [Fact]
    public async Task GetUserFavoritesAsync_ShouldReturnUserFavorites()
    {
        using var context = CreateInMemoryContext();
        var favoriteService = new FavoriteService(context);

        var user = new ApplicationUser
        {
            Id = "user1",
            UserName = "TestUser",
            Email = "testuser@example.com",  // Krävs i modellen
            FirstName = "FirstName",  // Om dessa är krav i modellen
            LastName = "LastName"
        };

        var recipe1 = new Recipe
        {
            Id = "recipe1",
            Title = "Chocolate Cake",
            Description = "A delicious chocolate cake.",
            Difficulty = "Medium",
            UserId = user.Id
        };

        var recipe2 = new Recipe
        {
            Id = "recipe2",
            Title = "Vanilla Cake",
            Description = "A delicious vanilla cake.",
            Difficulty = "Medium",
            UserId = user.Id
        };

        var favorite1 = new Favorite { UserId = user.Id, RecipeId = recipe1.Id, Recipe = recipe1 };
        var favorite2 = new Favorite { UserId = user.Id, RecipeId = recipe2.Id, Recipe = recipe2 };

        context.Users.Add(user);
        context.Recipes.AddRange(recipe1, recipe2);
        context.Favorites.AddRange(favorite1, favorite2);
        await context.SaveChangesAsync();

        var result = await favoriteService.GetUserFavoritesAsync(user);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Ska returnera två favoriter
        Assert.Contains(result, r => r.Title == "Chocolate Cake");
        Assert.Contains(result, r => r.Title == "Vanilla Cake");
    }
}
