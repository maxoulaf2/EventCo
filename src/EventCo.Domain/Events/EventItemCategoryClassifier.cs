using System.Globalization;
using System.Text;

namespace EventCo.Domain.Events;

// Déduit la catégorie d'un article de son intitulé, à la lecture (cf. docs/adr/0001-categorie-deduite-a-la-lecture.md) :
// améliorer ce dictionnaire reclasse tous les articles dont la catégorie n'a pas été corrigée à la main.
public static class EventItemCategoryClassifier
{
    private static readonly Dictionary<EventItemCategory, string[]> Keywords = new()
    {
        [EventItemCategory.Aperitif] =
        [
            "apéro", "apéritif", "gâteau apéro", "biscuit apéro", "chips", "cacahuète", "olive", "saucisson", "tapenade",
            "houmous", "guacamole", "tzatziki", "tarama", "rillettes", "blinis", "toast", "crackers", "verrine",
            "amuse-bouche", "crudités", "pistache", "noix de cajou", "feuilleté", "gougère", "mini pizza", "plateau apéro",
            "plateau de charcuterie",
        ],
        [EventItemCategory.MainCourses] =
        [
            "entrée", "plat", "salade", "taboulé", "quiche", "tarte salée", "cake salé", "pizza", "lasagne", "gratin",
            "poulet", "dinde", "chapon", "rôti", "bœuf", "boeuf", "porc", "agneau", "gigot", "viande", "poisson",
            "saumon", "foie gras", "huître", "crevette", "fruits de mer", "soupe", "velouté", "terrine", "pâté",
            "pâtes", "riz", "couscous", "paella", "chili", "curry", "tajine", "raclette", "fondue", "légume",
            "pomme de terre", "frite", "purée", "burger", "sandwich", "wrap", "œuf", "oeuf", "merguez", "saucisse",
            "brochette", "charcuterie", "jambon",
        ],
        [EventItemCategory.CheeseAndBread] =
        [
            "fromage", "camembert", "brie", "comté", "roquefort", "chèvre", "reblochon", "emmental", "gruyère",
            "mozzarella", "feta", "parmesan", "mont d'or", "pain", "baguette", "pain de mie", "pain de campagne",
            "beurre", "plateau de fromages",
        ],
        [EventItemCategory.Desserts] =
        [
            "dessert", "gâteau", "bûche", "tarte", "tiramisu", "mousse au chocolat", "chocolat", "glace", "sorbet",
            "crêpe", "gaufre", "cookie", "biscuit", "macaron", "galette des rois", "fruit", "salade de fruits",
            "clafoutis", "flan", "crème brûlée", "panna cotta", "cheesecake", "brownie", "muffin", "cupcake",
            "madeleine", "financier", "meringue", "pavlova", "profiterole", "éclair", "bonbon", "confiserie",
            "croissant", "pain au chocolat", "viennoiserie", "brioche", "pomme", "fraise", "clémentine",
            "mandarine", "cerise", "pastèque", "yaourt", "compote", "confiture", "pâte à tartiner", "pain d'épices",
        ],
        [EventItemCategory.Drinks] =
        [
            "boisson", "eau", "vin", "rosé", "champagne", "crémant", "prosecco", "bière", "cidre", "jus", "soda",
            "coca", "limonade", "sirop", "thé", "café", "tisane", "whisky", "rhum", "vodka", "gin", "pastis",
            "liqueur", "digestif", "cocktail", "punch", "sangria", "bouteille", "glaçon", "lait", "kir",
        ],
        [EventItemCategory.Equipment] =
        [
            "guirlande", "décoration", "déco", "ballon", "bougie", "nappe", "serviette", "gobelet", "verre",
            "assiette", "couvert", "fourchette", "couteau", "cuillère", "plateau", "glacière", "enceinte",
            "musique", "barbecue", "charbon", "chaise", "table", "tente", "sac poubelle", "essuie-tout", "sopalin",
            "papier alu", "papier aluminium", "film alimentaire", "tire-bouchon", "décapsuleur", "jeu", "guitare",
            "lampe", "lampion", "rallonge", "multiprise", "parasol", "couverture", "plaid", "matelas", "duvet",
            "sac de couchage", "vaisselle", "torchon", "éponge", "liquide vaisselle", "cotillon", "confetti",
        ],
    };

    private static readonly (string[] Words, EventItemCategory Category)[] Entries =
        Keywords.SelectMany(k => k.Value.Select(keyword => (Normalize(keyword), k.Key))).ToArray();

    // Une expression de plusieurs mots l'emporte sur un mot seul (« salade de fruits » avant « salade ») ;
    // à égalité, le premier mot reconnu dans l'intitulé l'emporte, le nom principal venant en tête en français
    // (« vin chaud », « bûche au chocolat »).
    public static EventItemCategory Classify(string title)
    {
        var words = Normalize(title);
        var best = (Length: 0, Position: int.MaxValue, Category: EventItemCategory.Other);

        foreach (var (keywordWords, category) in Entries)
        {
            var position = IndexOf(words, keywordWords);
            if (position < 0)
                continue;

            if (keywordWords.Length > best.Length || (keywordWords.Length == best.Length && position < best.Position))
                best = (keywordWords.Length, position, category);
        }

        return best.Category;
    }

    private static int IndexOf(string[] words, string[] sequence)
    {
        for (var i = 0; i + sequence.Length <= words.Length; i++)
        {
            if (words.AsSpan(i, sequence.Length).SequenceEqual(sequence))
                return i;
        }

        return -1;
    }

    // Minuscules, sans accents ni ligatures, découpé en mots, pluriels simples en -s/-x ramenés au singulier.
    // Appliquée à la fois aux mots-clés et à l'intitulé : un mot-clé déjà terminé par -s (« jus ») reste comparable.
    private static string[] Normalize(string text)
    {
        var decomposed = text.ToLowerInvariant().Replace("œ", "oe").Replace("æ", "ae").Normalize(NormalizationForm.FormD);
        var letters = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            letters.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        return letters.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Length > 3 && (word.EndsWith('s') || word.EndsWith('x')) ? word[..^1] : word)
            .ToArray();
    }
}
