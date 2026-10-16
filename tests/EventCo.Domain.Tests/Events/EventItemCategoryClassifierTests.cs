using EventCo.Domain.Events;

namespace EventCo.Domain.Tests.Events;

public class EventItemCategoryClassifierTests
{
    [Theory]
    [InlineData("Chips", EventItemCategory.Aperitif)]
    [InlineData("Gâteau apéro", EventItemCategory.Aperitif)]
    [InlineData("Plateau de charcuterie", EventItemCategory.Aperitif)]
    [InlineData("Quiche lorraine", EventItemCategory.MainCourses)]
    [InlineData("Tarte salée aux poireaux", EventItemCategory.MainCourses)]
    [InlineData("Pommes de terre", EventItemCategory.MainCourses)]
    [InlineData("Plateau de fromages", EventItemCategory.CheeseAndBread)]
    [InlineData("2 baguettes", EventItemCategory.CheeseAndBread)]
    [InlineData("Bûche au chocolat", EventItemCategory.Desserts)]
    [InlineData("Salade de fruits", EventItemCategory.Desserts)]
    [InlineData("Pain au chocolat", EventItemCategory.Desserts)]
    [InlineData("GÂTEAUX", EventItemCategory.Desserts)]
    [InlineData("Vin chaud", EventItemCategory.Drinks)]
    [InlineData("2 bouteilles de rosé", EventItemCategory.Drinks)]
    [InlineData("Jus d'orange", EventItemCategory.Drinks)]
    [InlineData("Guirlandes", EventItemCategory.Equipment)]
    [InlineData("Verres à vin", EventItemCategory.Equipment)]
    [InlineData("Œufs", EventItemCategory.MainCourses)]
    [InlineData("Réserver la salle", EventItemCategory.Other)]
    [InlineData("Le truc de mamie", EventItemCategory.Other)]
    public void Classify_Title_ReturnsExpectedCategory(string title, EventItemCategory expected)
    {
        Assert.Equal(expected, EventItemCategoryClassifier.Classify(title));
    }
}
