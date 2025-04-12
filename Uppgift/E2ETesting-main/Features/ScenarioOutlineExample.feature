Feature: Login and Recipe Management
  As a user
  I want to log in and manage recipes
  So that I can contribute recipes to the website

  Scenario: Login and add a new recipe
    Given I am on the login page
    When I enter "Emil2" as the email
    And I enter "Password1!" as the password
    And I check "Remember Me"
    And I submit the login form
    Then I should be redirected to the homepage
    Then I navigate to add recipe page
    When I fill in the recipe form
    
    