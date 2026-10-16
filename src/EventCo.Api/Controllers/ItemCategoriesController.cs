using EventCo.Api.Contracts.Events;
using EventCo.Domain.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCo.Api.Controllers;

// Seule source de la liste des catégories pour le frontend (libellés et ordre d'affichage) : il n'en garde
// aucune copie. Les libellés sont de la présentation, d'où leur place ici plutôt que dans le Domain.
[Authorize]
[ApiController]
[Route("api/item-categories")]
public sealed class ItemCategoriesController : ControllerBase
{
    private static readonly ItemCategoryResponse[] Categories = Enum.GetValues<EventItemCategory>()
        .Select(category => new ItemCategoryResponse(category.ToString(), Label(category)))
        .ToArray();

    [HttpGet]
    public IActionResult GetAll() => Ok(Categories);

    private static string Label(EventItemCategory category) => category switch
    {
        EventItemCategory.Aperitif => "Apéro",
        EventItemCategory.MainCourses => "Entrées et plats",
        EventItemCategory.CheeseAndBread => "Fromage et pain",
        EventItemCategory.Desserts => "Desserts",
        EventItemCategory.Drinks => "Boissons",
        EventItemCategory.Equipment => "Matériel et déco",
        EventItemCategory.Other => "Autre",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
    };
}
