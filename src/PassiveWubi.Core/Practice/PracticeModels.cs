namespace PassiveWubi.Core.Practice;

public sealed class PracticeItem
{
    public string Prompt { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public sealed class MnemonicPracticeItem
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class PracticeEdition
{
    public List<PracticeItem> KeyNames { get; set; } = [];
    public List<PracticeItem> RootChars { get; set; } = [];
    public List<PracticeItem> FormingRoots { get; set; } = [];
    public List<PracticeItem> CommonChars { get; set; } = [];
    public List<PracticeItem> Level1 { get; set; } = [];
    public List<PracticeItem> Level2 { get; set; } = [];
    public List<PracticeItem> Level3 { get; set; } = [];
    public List<PracticeItem> Identification { get; set; } = [];
    public List<PracticeItem> Difficult { get; set; } = [];
    public List<MnemonicPracticeItem> Mnemonics { get; set; } = [];
    public List<PracticeItem> Words2 { get; set; } = [];
    public List<PracticeItem> Words3 { get; set; } = [];
    public List<PracticeItem> Words4 { get; set; } = [];
}

public sealed class PracticeCatalog
{
    public Dictionary<WubiVersion, PracticeEdition> Editions { get; set; } = [];
}

public enum CharacterExercise
{
    FormingRoot,
    KeyName,
    CommonCharacter,
    Level1,
    Level2,
    Level3,
    Identification,
    Difficult
}

public enum PhraseExercise
{
    TwoCharacters,
    ThreeCharacters,
    FourCharacters,
    Mixed,
    MixedShuffled
}
