namespace PassiveWubi.Core.Practice;

public sealed class PracticeSession
{
    private readonly List<PracticeItem> _items;
    private int _index;
    private DateTimeOffset _startedAt = DateTimeOffset.Now;

    public PracticeSession(IEnumerable<PracticeItem> items, bool shuffle = false)
    {
        _items = items
            .Where(item => !string.IsNullOrWhiteSpace(item.Prompt)
                           && !string.IsNullOrWhiteSpace(item.Code))
            .ToList();
        if (shuffle)
        {
            Shuffle();
        }
    }

    public PracticeItem? Current =>
        _items.Count == 0 ? null : _items[Math.Clamp(_index, 0, _items.Count - 1)];

    public int Position => _items.Count == 0 ? 0 : Math.Min(_index + 1, _items.Count);
    public int Total => _items.Count;
    public int CorrectCount { get; private set; }
    public int AttemptCount { get; private set; }
    public TimeSpan Elapsed => DateTimeOffset.Now - _startedAt;
    public double Accuracy => AttemptCount == 0 ? 0 : CorrectCount * 100.0 / AttemptCount;

    public PracticeFeedback Submit(string? answer)
    {
        if (Current is null)
        {
            return new PracticeFeedback(false, string.Empty, string.Empty);
        }

        AttemptCount++;
        var correct = WubiAnswerEvaluator.IsCorrect(Current.Code, answer);
        if (correct)
        {
            CorrectCount++;
        }

        return new PracticeFeedback(correct, Current.Code, WubiAnswerEvaluator.Normalize(answer));
    }

    public bool MoveNext()
    {
        if (_items.Count == 0)
        {
            return false;
        }

        _index++;
        if (_index < _items.Count)
        {
            return true;
        }

        _index = 0;
        Shuffle();
        return false;
    }

    public void Restart()
    {
        _index = 0;
        CorrectCount = 0;
        AttemptCount = 0;
        _startedAt = DateTimeOffset.Now;
        Shuffle();
    }

    private void Shuffle()
    {
        for (var i = _items.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (_items[i], _items[j]) = (_items[j], _items[i]);
        }
    }
}

public sealed record PracticeFeedback(
    bool IsCorrect,
    string CorrectCode,
    string SubmittedCode);
