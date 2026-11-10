using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PassiveWubi.Core.Practice;

namespace PassiveWubi.Views;

public partial class PracticeView : UserControl
{
    private readonly DispatcherTimer _speedTimer;
    private TimeSpan _speedRemaining;
    private DateTimeOffset _speedStartedAt;
    private string _speedTarget = string.Empty;
    private bool _speedRunning;
    private bool _isInitializing;
    private bool _practiceAnswered;
    private PracticeSession? _practiceSession;
    private string _practiceKind = "character";
    private WubiVersion _practiceVersion;
    private bool _practiceLoaded;

    public PracticeView()
    {
        InitializeComponent();
        _isInitializing = true;
        PopulateSpeedTexts();
        PopulateCharacterCategories();
        _speedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _speedTimer.Tick += SpeedTimer_Tick;
        Loaded += PracticeView_Loaded;
        Unloaded += PracticeView_Unloaded;
        _isInitializing = false;
    }

    private App Host => (App)Application.Current;

    public void ActivatePractice()
    {
        RefreshVersion();
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => SpeedInput.Focus()));
    }

    private void PracticeView_Loaded(object sender, RoutedEventArgs e)
    {
        Host.Settings.PropertyChanged += Settings_PropertyChanged;
        RefreshVersion();
    }

    private void PracticeView_Unloaded(object sender, RoutedEventArgs e)
    {
        Host.Settings.PropertyChanged -= Settings_PropertyChanged;
        _speedTimer.Stop();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Models.AppSettings.Version))
        {
            RefreshVersion();
        }
    }

    private void PopulateSpeedTexts()
    {
        foreach (var text in SpeedTestData.Texts)
        {
            SpeedTextCombo.Items.Add(new ComboBoxItem
            {
                Content = text.Title,
                Tag = text.Id
            });
        }

        SpeedTextCombo.Items.Add(new ComboBoxItem
        {
            Content = "自定义文本",
            Tag = "custom"
        });
        SpeedTextCombo.SelectedIndex = 0;
    }

    private void PopulateCharacterCategories()
    {
        SubcategoryCombo.Items.Clear();
        AddSubcategory("成字字根", CharacterExercise.FormingRoot);
        AddSubcategory("键名字", CharacterExercise.KeyName);
        AddSubcategory("常用字", CharacterExercise.CommonCharacter);
        AddSubcategory("一级简码", CharacterExercise.Level1);
        AddSubcategory("二级简码", CharacterExercise.Level2);
        AddSubcategory("三级简码", CharacterExercise.Level3);
        AddSubcategory("识别码", CharacterExercise.Identification);
        AddSubcategory("难拆字", CharacterExercise.Difficult);
        SubcategoryCombo.SelectedIndex = 2;
    }

    private void PopulatePhraseCategories()
    {
        SubcategoryCombo.Items.Clear();
        AddSubcategory("二字词组", PhraseExercise.TwoCharacters);
        AddSubcategory("三字词组", PhraseExercise.ThreeCharacters);
        AddSubcategory("四字词组", PhraseExercise.FourCharacters);
        AddSubcategory("混合词组", PhraseExercise.Mixed);
        AddSubcategory("混合乱序", PhraseExercise.MixedShuffled);
        SubcategoryCombo.SelectedIndex = 0;
    }

    private void AddSubcategory(string title, object tag) =>
        SubcategoryCombo.Items.Add(new ComboBoxItem { Content = title, Tag = tag });

    private void RefreshVersion()
    {
        var version = Host.Settings.Version;
        VersionLabelText.Text = $"当前题库：{WubiDefinitions.GetVersionName(version)}";
        if (!_practiceLoaded || _practiceVersion != version)
        {
            _practiceVersion = version;
            _practiceLoaded = true;
            StartPracticeSession();
        }
    }

    private void SpeedText_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || CustomTextInput is null)
        {
            return;
        }

        CustomTextInput.Visibility =
            (SpeedTextCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "custom"
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void SpeedStart_Click(object sender, RoutedEventArgs e)
    {
        var target = ResolveSpeedText();
        if (string.IsNullOrWhiteSpace(target))
        {
            MessageBox.Show(
                Window.GetWindow(this),
                "请选择文章或输入自定义测试文字。",
                "速度测试",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        _speedTarget = target;
        _speedRemaining = TimeSpan.FromMinutes(GetSelectedMinutes());
        _speedStartedAt = DateTimeOffset.Now;
        _speedRunning = true;
        SpeedTargetText.Text = _speedTarget;
        SpeedInput.Text = string.Empty;
        SpeedInput.IsReadOnly = false;
        SpeedInput.Focus();
        SpeedStartButton.Content = "重新开始";
        SpeedTimerText.Text = FormatRemaining(_speedRemaining);
        UpdateSpeedStats();
        _speedTimer.Start();
    }

    private void SpeedReset_Click(object sender, RoutedEventArgs e)
    {
        _speedTimer.Stop();
        _speedRunning = false;
        _speedTarget = string.Empty;
        SpeedInput.Text = string.Empty;
        SpeedInput.IsReadOnly = true;
        SpeedTargetText.Text = "准备好后点击“开始测试”。";
        SpeedTimerText.Text = $"{GetSelectedMinutes():00}:00";
        SpeedCpmText.Text = "0 字/分钟";
        SpeedAccuracyText.Text = "0%";
        SpeedCountText.Text = "0 字";
        SpeedStartButton.Content = "开始测试";
    }

    private void SpeedInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_speedRunning)
        {
            return;
        }

        UpdateSpeedStats();
        if (SpeedInput.Text.Length >= _speedTarget.Length)
        {
            FinishSpeedTest("测试文本已全部输入");
        }
    }

    private void SpeedInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            e.Handled = true;
            SpeedInput.SelectedText = "    ";
        }
    }

    private void SpeedTimer_Tick(object? sender, EventArgs e)
    {
        if (!_speedRunning)
        {
            return;
        }

        _speedRemaining = TimeSpan.FromMinutes(GetSelectedMinutes()) -
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
        if (!_speedRunning && _speedTarget.Length == 0)
        {
            return;
        }

        var elapsed = DateTimeOffset.Now - _speedStartedAt;
        var result = SpeedTestCalculator.Calculate(_speedTarget, SpeedInput.Text, elapsed);
        SpeedCpmText.Text = $"{result.CharactersPerMinute:F0} 字/分钟";
        SpeedAccuracyText.Text = $"{result.Accuracy:F1}%";
        SpeedCountText.Text = $"{result.TypedCharacters} 字";
    }

    private void FinishSpeedTest(string reason)
    {
        if (!_speedRunning)
        {
            return;
        }

        _speedRunning = false;
        _speedTimer.Stop();
        SpeedInput.IsReadOnly = true;
        SpeedTimerText.Text = reason;
        UpdateSpeedStats();
    }

    private string ResolveSpeedText()
    {
        var selected = (SpeedTextCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (selected == "custom")
        {
            return CustomTextInput.Text.Trim();
        }

        return SpeedTestData.Texts
            .FirstOrDefault(item => item.Id == selected)
            ?.Text ?? string.Empty;
    }

    private int GetSelectedMinutes()
    {
        return int.TryParse(SpeedMinutesInput.Text, out var minutes)
            ? Math.Clamp(minutes, 1, 50)
            : 3;
    }

    private static string FormatRemaining(TimeSpan remaining) =>
        $"{(int)Math.Max(0, remaining.TotalMinutes):00}:{Math.Max(0, remaining.Seconds):00}";

    private void PracticeKind_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || SubcategoryCombo is null)
        {
            return;
        }

        _practiceKind = (PracticeKindCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString()
                        ?? "character";
        SubcategoryLabel.Visibility = _practiceKind is "character" or "phrase"
            ? Visibility.Visible
            : Visibility.Collapsed;
        SubcategoryCombo.Visibility = SubcategoryLabel.Visibility;

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

    private void Subcategory_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitializing)
        {
            StartPracticeSession();
        }
    }

    private void StartPracticeSession()
    {
        if (PracticePromptText is null)
        {
            return;
        }

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
        ShowCurrentPracticeQuestion();
    }

    private IEnumerable<PracticeItem> GetCharacterItems()
    {
        var selected = (SubcategoryCombo.SelectedItem as ComboBoxItem)?.Tag;
        var category = selected is CharacterExercise exercise
            ? exercise
            : CharacterExercise.CommonCharacter;
        return PracticeCatalogProvider.GetCharacterExercises(_practiceVersion, category);
    }

    private IEnumerable<PracticeItem> GetPhraseItems()
    {
        var selected = (SubcategoryCombo.SelectedItem as ComboBoxItem)?.Tag;
        var category = selected is PhraseExercise exercise
            ? exercise
            : PhraseExercise.TwoCharacters;
        return PracticeCatalogProvider.GetPhraseExercises(_practiceVersion, category);
    }

    private void ShowCurrentPracticeQuestion()
    {
        _practiceAnswered = false;
        PracticeAnswerInput.IsEnabled = true;
        PracticeAnswerInput.Clear();
        PracticeAnswerInput.Focus();
        PracticeFeedbackText.Foreground = (Brush)FindResource("InkBrush");
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
        PracticeProgressText.Text =
            $"{_practiceSession.Position} / {_practiceSession.Total}";
        PracticeAccuracyText.Text = $"{_practiceSession.Accuracy:F0}%";
    }

    private void PracticeSubmit_Click(object sender, RoutedEventArgs e) => SubmitPracticeAnswer();

    private void PracticeNext_Click(object sender, RoutedEventArgs e)
    {
        if (_practiceSession is null)
        {
            return;
        }

        var continued = _practiceSession.MoveNext();
        if (!continued)
        {
            PracticeProgressText.Text = "本轮完成，题目已重新排列";
        }

        ShowCurrentPracticeQuestion();
    }

    private void PracticeAnswer_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            SubmitPracticeAnswer();
        }
    }

    private void SubmitPracticeAnswer()
    {
        if (_practiceSession?.Current is null || _practiceAnswered)
        {
            return;
        }

        var submitted = PracticeAnswerInput.Text.Trim();
        if (submitted.Length == 0)
        {
            PracticeAnswerInput.Focus();
            return;
        }

        var feedback = _practiceSession.Submit(submitted);
        _practiceAnswered = true;
        PracticeAnswerInput.IsEnabled = false;
        PracticeAccuracyText.Text = $"{_practiceSession.Accuracy:F0}%";
        PracticeFeedbackText.Foreground = feedback.IsCorrect
            ? new SolidColorBrush(Color.FromRgb(71, 183, 132))
            : new SolidColorBrush(Color.FromRgb(239, 114, 94));
        PracticeFeedbackText.Text = feedback.IsCorrect
            ? $"回答正确：{feedback.CorrectCode}"
            : $"回答错误。你输入的是“{feedback.SubmittedCode}”，正确编码是“{feedback.CorrectCode}”";
        PracticeBreakdownText.Text =
            $"编码对应键位：{WubiAnswerEvaluator.DescribeCode(_practiceVersion, feedback.CorrectCode)}";
    }
}
