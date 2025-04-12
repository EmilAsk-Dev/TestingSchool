namespace E2ETesting.Steps;
using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

// Bindar klassen till SpecFlow så att stegen kan användas i feature-filer
[Binding]
public class RecipeSteps
{
    private IPlaywright _playwright; // Hanterar Playwright-instansen
    private IBrowser _browser;       // Webbläsarinstansen
    private IBrowserContext _context; // Kontext för browser-session (cookies m.m.)
    private IPage _page;             // Den aktuella sidan som testas
    private string _createdRecipeTitle; // Används för att spara titeln på receptet
    private static bool _isLoggedIn = false; // Håller koll på inloggningsstatus
    private static string _currentEmail = string.Empty; // Sparar aktuell e-post

    // Körs innan varje scenario – startar Playwright, öppnar webbläsare och skapar ny sida
    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 300 });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
        _createdRecipeTitle = "Test Recipe " + DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    // Körs efter varje scenario – stänger webbläsaren och släpper resurser
    [AfterScenario]
    public async Task Teardown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    // Navigerar till inloggningssidan
    [Given("I am on the login page")]
    public async Task GivenIAmOnTheLoginPage()
    {
        await _page.GotoAsync("https://localhost:7060/Login");
    }

    // Fyller i e-postfältet
    [When(@"I enter ""(.*)"" as the email")]
    public async Task WhenIEnterAsTheEmail(string email)
    {
        _currentEmail = email;
        await _page.FillAsync("input[id$='UsernameOrEmail']", email);
    }

    // Fyller i lösenordsfältet
    [When(@"I enter ""(.*)"" as the password")]
    public async Task WhenIEnterAsThePassword(string password)
    {
        await _page.FillAsync("input[id$='Password']", password);
    }

    // Bockar i "Remember Me"-rutan om den finns
    [When(@"I check ""(.*)""")]
    public async Task WhenICheckRememberMe(string label)
    {
        if (label == "Remember Me")
        {
            await _page.CheckAsync("input[id='RememberMe']");
        }
    }

    // Skickar inloggningsformuläret
    [When("I submit the login form")]
    public async Task WhenISubmitTheLoginForm()
    {
        await _page.ClickAsync("button.submit");
    }

    // Bekräftar att man hamnat på startsidan och att inget felmeddelande visas
    [Then("I should be redirected to the homepage")]
    public async Task ThenIShouldBeRedirectedToTheHomepage()
    {
        await _page.WaitForURLAsync("https://localhost:7060/");
        var errorVisible = await _page.IsVisibleAsync("p[style='color: red;']");
        Assert.False(errorVisible, "Felmeddelande ska inte synas efter lyckad inloggning");
        _isLoggedIn = true;
    }

    // Bekräftar att felmeddelande visas efter felaktig inloggning
    [Then("I should see an error message")]
    public async Task ThenIShouldSeeAnErrorMessage()
    {
        var errorVisible = await _page.IsVisibleAsync("p[style='color: red;']");
        Assert.True(errorVisible, "Felmeddelande ska synas efter misslyckad inloggning");
    }

    // Navigerar till sidan för att lägga till nytt recept
    [Then("I navigate to add recipe page")]
    public async Task ThenINavigateToAddRecipePage()
    {
        await _page.GotoAsync("https://localhost:7060/addrecipe");
        await _page.WaitForSelectorAsync("form.recipe-form");
    }

    // Fyller i formuläret för att skapa ett nytt recept
    [When("I fill in the recipe form")]
    public async Task WhenIFillInTheRecipeForm()
    {
        try
        {
            // Fyller i grundläggande information om receptet
            await _page.FillAsync("input[id$='Recipe_Title']", _createdRecipeTitle);
            await _page.FillAsync("textarea[id$='Recipe_Description']", "This is an automated test recipe description.");
            await _page.SelectOptionAsync("select[id$='Recipe_Category']", "Dinner");
            await _page.FillAsync("input[id$='Recipe_CookTime']", "30");
            await _page.SelectOptionAsync("select[id$='Recipe_Difficulty']", "Easy");
            await _page.FillAsync("input[id$='TagsInput']", "test, automation, e2e");

            // Lägger till ingredienser
            await _page.FillAsync("input[id='RecipeIngredients_0__Quantity']", "200");
            await _page.FillAsync("input[id='RecipeIngredients_0__Unit']", "g");
            await _page.FillAsync("input[id='RecipeIngredients_0__IngredientName']", "Flour");

            await _page.FillAsync("input[id='RecipeIngredients_1__Quantity']", "100");
            await _page.FillAsync("input[id='RecipeIngredients_1__Unit']", "ml");
            await _page.FillAsync("input[id='RecipeIngredients_1__IngredientName']", "Milk");

            await _page.FillAsync("input[id='RecipeIngredients_2__Quantity']", "100");
            await _page.FillAsync("input[id='RecipeIngredients_2__Unit']", "ml");
            await _page.FillAsync("input[id='RecipeIngredients_2__IngredientName']", "Something");

            // Lägger till instruktioner
            await _page.FillAsync("textarea[id='Instructions_0__InstructionText']", "Mix flour and milk in a bowl.");
            await _page.FillAsync("textarea[id='Instructions_1__InstructionText']", "Cook for 10 minutes.");
            await _page.FillAsync("textarea[id='Instructions_2__InstructionText']", "Serve and enjoy!");

            // Verifierar att titeln verkligen fylldes i
            var filledTitle = await _page.InputValueAsync("input[id$='Recipe_Title']");
            if (string.IsNullOrWhiteSpace(filledTitle))
            {
                throw new Exception("Titeln fylldes inte i korrekt.");
            }

            Console.WriteLine("Alla fält i receptformuläret fylldes i korrekt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Misslyckades med att fylla i receptformuläret: {ex.Message}");
            throw;
        }
    }
}
