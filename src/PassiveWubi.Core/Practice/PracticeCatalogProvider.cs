using System.Reflection;
using System.Text.Json;

namespace PassiveWubi.Core.Practice;

public static class PracticeCatalogProvider
{
    private static readonly Lazy<PracticeCatalog> Catalog = new(Load);

    public static PracticeEdition GetEdition(WubiVersion version) =>
        Catalog.Value.Editions.TryGetValue(version, out var edition)
            ? edition
            : Catalog.Value.Editions[WubiVersion.V86];

    public static IReadOnlyList<PracticeItem> GetCharacterExercises(
        WubiVersion version,
        CharacterExercise exercise)
    {
        var edition = GetEdition(version);
        return exercise switch
        {
            CharacterExercise.FormingRoot => edition.FormingRoots,
            CharacterExercise.KeyName => edition.KeyNames,
            CharacterExercise.CommonCharacter => edition.CommonChars,
            CharacterExercise.Level1 => edition.Level1,
            CharacterExercise.Level2 => edition.Level2,
            CharacterExercise.Level3 => edition.Level3,
            CharacterExercise.Identification => edition.Identification,
            CharacterExercise.Difficult => edition.Difficult,
            _ => edition.CommonChars
        };
    }

    public static IReadOnlyList<PracticeItem> GetPhraseExercises(
        WubiVersion version,
        PhraseExercise exercise)
    {
        var edition = GetEdition(version);
        var items = exercise switch
        {
            PhraseExercise.TwoCharacters => edition.Words2,
            PhraseExercise.ThreeCharacters => edition.Words3,
            PhraseExercise.FourCharacters => edition.Words4,
            PhraseExercise.Mixed => edition.Words2
                .Concat(edition.Words3)
                .Concat(edition.Words4)
                .ToList(),
            PhraseExercise.MixedShuffled => edition.Words2
                .Concat(edition.Words3)
                .Concat(edition.Words4)
                .OrderBy(_ => Random.Shared.Next())
                .ToList(),
            _ => edition.Words2
        };
        return items;
    }

    public static IReadOnlyList<PracticeItem> GetRootExercises(WubiVersion version) =>
        GetEdition(version).RootChars;

    public static IReadOnlyList<MnemonicPracticeItem> GetMnemonics(WubiVersion version) =>
        GetEdition(version).Mnemonics;

    private static PracticeCatalog Load()
    {
        var assembly = typeof(PracticeCatalogProvider).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith("practice-data.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Embedded practice data was not found.");
        using var document = JsonDocument.Parse(stream);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var catalog = new PracticeCatalog();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!Enum.TryParse<WubiVersion>(property.Name, out var version))
            {
                continue;
            }

            var edition = property.Value.Deserialize<PracticeEdition>(options)
                ?? throw new InvalidOperationException($"Practice data for {version} is invalid.");
            catalog.Editions[version] = edition;
        }

        return catalog.Editions.Count == 3
            ? catalog
            : throw new InvalidOperationException("Practice data must contain all three Wubi editions.");
    }
}
