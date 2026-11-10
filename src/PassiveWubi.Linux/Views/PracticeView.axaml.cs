using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PassiveWubi.Core;
using PassiveWubi.Core.Practice;

namespace PassiveWubi.Linux.Views;

public partial class PracticeView : UserControl
{
    private readonly DispatcherTimer _speedTimer;
    private TimeSpan _speedRemaining;
    private DateTimeOffset _speedStartedAt;
    private string _speedTarget = string.Empty;
    private bool _speedRunning;
    private bool _initializing = true;
    private bool _practiceAnswered;
    private PracticeSession? _practiceSession;
    private string _practiceKind = "character";
    private WubiVersion _practiceVersion;
    private bool _practiceLoaded;

    public PracticeView()
    {
        InitializeComponent();
        PopulateSpeedTexts();
        PopulateCharacterCategories();
        _speedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _speedTimer.Tick += (_, _) => SpeedTimer_Tick();
        _initializing = false;
    }

    private App Host => (App)Application.Current!;

    public void RefreshForVersion()
    {
        var version = Host.Settings.Version;
        VersionLabelText.Text = $"当前题库：{WubiDefinitions.GetVersionName(version)}";
        if (!_practiceLoaded || version != _practiceVersion)
        {
            _practiceVersion = version;
            _practiceLoaded = true;
            StartPracticeSession();
        }
    }

    private void PopulateSpeedTexts()
    {
        foreach (var item in SpeedTestData.Texts)
        {
            SpeedTextCombo.Items.Add(new ComboBoxItem { Content = item.Title, Tag = item.Id });
        }

        SpeedTextCombo.Items.Add(new ComboBoxItem { Content = "自定义文本", Tag = "custom" });
        SpeedTextCombo.SelectedIndex = 0;
    }

    private void PopulateCharacterCategories()
    {
        SubcategoryCombo.Items.Clear();
        AddSub("成字字根", CharacterExercise.FormingRoot);
        AddSub("键名字", CharacterExercise.KeyName);
        AddSub("常用字", CharacterExercise.CommonCharacter);
        AddSub("一级简码", CharacterExercise.Level1);
        AddSub("二级简码", CharacterExercise.Level2);
        AddSub("三级简码", CharacterExercise.Level3);
        AddSub("识别码", CharacterExercise.Identification);
        AddSub("难拆字", CharacterExercise.Difficult);
        SubcategoryCombo.SelectedIndex = 2;
    }

    private void PopulatePhraseCategories()
    {
        SubcategoryCombo.Items.Clear();
        AddSub("二字词组", PhraseExercise.TwoCharacters);
        AddSub("三字词组", PhraseExercise.ThreeCharacters);
        AddSub("四字词组", PhraseExercise.FourCharacters);
        AddSub("混合词组", PhraseExercise.Mixed);
        AddSub("混合乱序", PhraseExercise.MixedShuffled);
        SubcategoryCombo.SelectedIndex = 0;
    }

    private void AddSub(string text, object tag) =>
        SubcategoryCombo.Items.Add(new ComboBoxItem { Content = text, Tag = tag });

    private void SpeedText_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || CustomTextInput is null)
        {
            return;
        }

        CustomTextInput.IsVisible =
            (SpeedTextCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "custom";
    }

    private void SpeedStart_Click(object? sender, RoutedEventArgs e)
    {
        var target = ResolveSpeedText();
        if (string.IsNullOrWhiteSpace(target))
        {
            SpeedTargetText.Text = "请选择文章或输入自定义测试文字。";
            return;
        }

        _speedTarget = target;
        _speedRemaining = TimeSpan.FromMinutes(GetMinutes());
        _speedStartedAt = DateTimeOffset.Now;
        _speedRunning = true;
        SpeedTargetText.Text = target;
        SpeedInput.Text = string.Empty;
        SpeedInput.IsReadOnly = false;
        SpeedInput.Focus();
        _speedTimer.Start();
    }

    private void SpeedReset_Click(object? sender, RoutedEventArgs e)
    {
        _speedTimer.Stop();
        _speedRunning = false;
        _speedTarget = string.Empty;
        SpeedInput.Text = string.Empty;
        SpeedInput.IsReadOnly = true;
        SpeedTargetText.Text = "准备好后点击“开始测试”。";
        SpeedTimerText.Text = $"{GetMinutes():00}:00";
        SpeedCpmText.Text = "0 字/分钟";
        SpeedAccuracyText.Text = "正确率 0%";
        SpeedCountText.Text = "0 字";
    }

    private void SpeedInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_speedRunning)
        {
            return;
        }

        UpdateSpeedStats();
        if (SpeedInput.Text?.Length >= _speedTarget.Length)
        {
            FinishSpeedTest("测试完成");
        }
    }

    private void SpeedTimer_Tick()
    {
        if (!_speedRunning)
        {
            return;
        }

        _speedRemaining = TimeSpan.FromMinutes(GetMinutes()) -
                          (DateTimeOffset.Now - _speedStartedAt);
        if (_speedRemaining <= TimeSpan.Zero)
        {
            _speedRemaining = TimeSpan.Zero;
            FinishSpeedTest("时间到");
            return;
        }

        SpeedTimerText.Text = FormatRemaining(_speedRemaining);
        UpdateSpeedStats();
    }

    private void UpdateSpeedStats()
    {
        var elapsed = DateTimeOffset.Now - _speedStartedAt;
        var result = SpeedTestCalculator.Calculate(_speedTarget, SpeedInput.Text ?? string.Empty, elapsed);
        SpeedCpmText.Text = $"{result.CharactersPerMinute:F0} 字/分钟";
        SpeedAccuracyText.Text = $"正确率 {result.Accuracy:F1}%";
        SpeedCountText.Text = $"{result.TypedCharacters} 字";
    }

    private void FinishSpeedTest(string message)
    {
        _speedRunning = false;
        _speedTimer.Stop();
        SpeedInput.IsReadOnly = true;
        SpeedTimerText.Text = message;
        UpdateSpeedStats();
    }

    private string ResolveSpeedText()
    {
        var selected = (SpeedTextCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (selected == "custom")
        {
            return CustomTextInput.Text?.Trim() ?? string.Empty;
        }

        return SpeedTestData.Texts.FirstOrDefault(item => item.Id == selected)?.Text ?? string.Empty;
    }

    private int GetMinutes()
    {
        return int.TryParse(SpeedMinutesInput.Text, out var minutes)
            ? Math.Clamp(minutes, 1, 50)
            : 3;
    }

    private static string FormatRemaining(TimeSpan value) =>
        $"{(int)Math.Max(0, value.TotalMinutes):00}:{Math.Max(0, value.Seconds):00}";

    private void PracticeKind_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing || SubcategoryCombo is null)
        {
            return;
        }

        _practiceKind = (PracticeKindCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString()
                        ?? "character";
        SubcategoryLabel.IsVisible = _practiceKind is "character" or "phrase";
        SubcategoryCombo.IsVisible = SubcategoryLabel.IsVisible;
        if (_practiceKind == "phrase")
        {
            PopulatePhraseCategories();
        }
        else if (_practiceKind == "character")
        {
            PopulateCharacterCategories();
        }

        StartPracticeSession();
    }

    private void Subcategory_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            StartPracticeSession();
        }
    }

    private void StartPracticeSession()
    {
        var items = _practiceKind switch
        {
            "mnemonic" => PracticeCatalogProvider.GetMnemonics(_practiceVersion)
                .Select(item => new PracticeItem
                {
                    Prompt = item.Text,
                    Code = item.Key.ToLowerInvariant(),
                    Category = $"口诀 · {item.Key} 键"
                }),
            "root" => PracticeCatalogProvider.GetRootExercises(_practiceVersion),
            "phrase" => GetPhraseItems(),
            _ => GetCharacterItems()
        };

        var phraseCategory = (SubcategoryCombo.SelectedItem as ComboBoxItem)?.Tag;
        _practiceSession = new PracticeSession(
            items,
            _practiceKind == "phrase" && phraseCategory is PhraseExercise.MixedShuffled);
        PracticeCategoryText.Text = _practiceKind switch
        {
            "mnemonic" => "五笔口诀练习",
            "root" => "五笔字根练习",
            "phrase" => "五笔词组练习",
            _ => "五笔拆字练习"
        };
        ShowQuestion();
    }

    private IEnumerable<PracticeItem> GetCharacterItems()
    {
        var selected = (SubcategoryCombo.SelectedItem as ComboBoxItem)?.Tag;
        return PracticeCatalogProvider.GetCharacterExercises(
            _practiceVersion,
            selected is CharacterExercise exercise ? exercise : CharacterExercise.CommonCharacter);
    }

    private IEnumerable<PracticeItem> GetPhraseItems()
    {
        var selected = (SubcategoryCombo.SelectedItem as ComboBoxItem)?.Tag;
        return PracticeCatalogProvider.GetPhraseExercises(
            _practiceVersion,
            selected is PhraseExercise exercise ? exercise : PhraseExercise.TwoCharacters);
    }

    private void ShowQuestion()
    {
        _practiceAnswered = false;
        PracticeAnswerInput.IsEnabled = true;
        PracticeAnswerInput.Text = string.Empty;
        PracticeAnswerInput.Focus();
        PracticeFeedbackText.Foreground = Brushes.LightGray;
        PracticeFeedbackText.Text = "输入编码后按 Enter 或点击“提交”。";
        PracticeBreakdownText.Text = string.Empty;

        if (_practiceSession?.Current is not { } current)
        {
            PracticePromptText.Text = "当前分类暂无题目";
            PracticeHintText.Text = "请选择其他分类。";
            PracticeProgressText.Text = "0 / 0";
            return;
        }

        PracticePromptText.Text = current.Prompt;
        PracticeHintText.Text = _practiceKind == "mnemonic"
            ? "判断题：这句口诀属于哪个键？输入键位字母。"
            : _practiceKind == "root"
                ? "判断题：这个字根位于哪个键？输入键位字母。"
                : _practiceKind == "phrase"
                    ? "输入该词组的五笔编码。"
                    : "输入该字的五笔编码。";
        PracticeProgressText.Text = $"{_practiceSession.Position} / {_practiceSession.Total}";
        PracticeAccuracyText.Text = $"正确率 {_practiceSession.Accuracy:F0}%";
    }

    private void PracticeAnswer_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            SubmitAnswer();
        }
    }

    private void PracticeSubmit_Click(object? sender, RoutedEventArgs e) => SubmitAnswer();

    private void PracticeNext_Click(object? sender, RoutedEventArgs e)
    {
        if (_practiceSession is null)
        {
            return;
        }

        if (!_practiceSession.MoveNext())
        {
            PracticeProgressText.Text = "本轮完成，题目已重新排列";
        }

        ShowQuestion();
    }

    private void SubmitAnswer()
    {
        if (_practiceSession?.Current is null || _practiceAnswered)
        {
            return;
        }

        var answer = PracticeAnswerInput.Text?.Trim() ?? string.Empty;
        if (answer.Length == 0)
        {
            PracticeAnswerInput.Focus();
            return;
        }

        var feedback = _practiceSession.Submit(answer);
        _practiceAnswered = true;
        PracticeAnswerInput.IsEnabled = false;
        PracticeAccuracyText.Text = $"正确率 {_practiceSession.Accuracy:F0}%";
        PracticeFeedbackText.Foreground = feedback.IsCorrect
            ? Brush.Parse("#53C692")
            : Brush.Parse("#EF725E");
        PracticeFeedbackText.Text = feedback.IsCorrect
            ? $"回答正确：{feedback.CorrectCode}"
            : $"回答错误。你输入“{feedback.SubmittedCode}”，正确编码是“{feedback.CorrectCode}”";
        PracticeBreakdownText.Text =
            $"编码对应键位：{WubiAnswerEvaluator.DescribeCode(_practiceVersion, feedback.CorrectCode)}";
    }
}
