using PassiveWubi.Core;
using PassiveWubi.Core.Practice;

foreach (var version in Enum.GetValues<WubiVersion>())
{
    var edition = PracticeCatalogProvider.GetEdition(version);
    Require(edition.KeyNames.Count == 25, $"{version}: key-name count");
    Require(edition.Mnemonics.Count == 25, $"{version}: mnemonic count");
    Require(edition.FormingRoots.Count > 20, $"{version}: forming roots");
    Require(edition.CommonChars.Count >= 200, $"{version}: common characters");
    Require(edition.Level1.Count == 25, $"{version}: level-one codes");
    Require(edition.Level2.Count > 50, $"{version}: level-two codes");
    Require(edition.Level3.Count > 50, $"{version}: level-three codes");
    Require(edition.Identification.Count >= 20, $"{version}: identification codes");
    Require(edition.Difficult.Count >= 20, $"{version}: difficult characters");
    Require(edition.Words2.Count >= 40, $"{version}: two-character phrases");
    Require(edition.Words3.Count >= 40, $"{version}: three-character phrases");
    Require(edition.Words4.Count >= 40, $"{version}: four-character phrases");

    foreach (var exercise in Enum.GetValues<CharacterExercise>())
    {
        var items = PracticeCatalogProvider.GetCharacterExercises(version, exercise);
        Require(items.Count > 0, $"{version}: {exercise} has questions");
        Require(items.All(item => !string.IsNullOrWhiteSpace(item.Prompt)
                                  && !string.IsNullOrWhiteSpace(item.Code)),
            $"{version}: {exercise} answers");
    }

    Require(edition.RootChars.All(item =>
            item.Code.Length == 1 && item.Code[0] is >= 'a' and <= 'z'),
        $"{version}: root questions must use key letters");
}

var sample = PracticeCatalogProvider.GetEdition(WubiVersion.V86).Level1[0];
var session = new PracticeSession([sample]);
Require(session.Submit(sample.Code.ToUpperInvariant()).IsCorrect, "answer normalization");
Require(!session.Submit("zzzz").IsCorrect, "incorrect answer detection");

var speed = SpeedTestCalculator.Calculate(
    "肌忆五笔",
    "肌忆五笔",
    TimeSpan.FromMinutes(1));
Require(Math.Abs(speed.Accuracy - 100) < 0.01, "speed accuracy");
Require(speed.CorrectCharacters == 4, "speed character count");

Console.WriteLine("PassiveWubi.Core practice smoke tests passed.");

static void Require(bool condition, string name)
{
    if (!condition)
    {
        throw new InvalidOperationException($"Smoke test failed: {name}");
    }
}
