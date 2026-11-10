namespace PassiveWubi.Core.Practice;

public sealed record SpeedText(string Id, string Title, string Text);

public static class SpeedTestData
{
    public static IReadOnlyList<SpeedText> Texts { get; } =
    [
        new(
            "ordinary",
            "日常办公",
            "清晨打开电脑，先整理今天要完成的事情。把重要的工作写在前面，再逐项安排时间。遇到问题时不急着反复尝试，先确认目标和路径，再记录结果。稳定的节奏比一时的速度更重要，认真检查也能减少返工。"),
        new(
            "wubi",
            "五笔学习",
            "学习五笔需要熟悉字根在键盘上的位置，也要理解汉字的拆分顺序。先从键名字和基本字根开始，再练习一级简码、二级简码和三级简码。每天用一点时间反复输入常用字，逐渐把看见字形到按下按键的过程变成自然的反应。"),
        new(
            "landscape",
            "山水短文",
            "沿着山路慢慢向前，树木在风里轻轻摇动。溪水从石头旁边流过，远处的云层映着柔和的光。走过一段坡地，视野忽然开阔，田野和村庄安静地铺在眼前。停下来听一会风声，再继续赶路，脚步反而更加轻松。"),
        new(
            "science",
            "科技短文",
            "软件设计既要解决眼前的问题，也要考虑以后维护和扩展的成本。清晰的数据结构可以减少重复代码，稳定的接口能让不同平台保持一致。测试不是为了证明程序没有错误，而是尽早发现风险，让修改和发布都更有把握。")
    ];
}

public sealed record SpeedTestResult(
    int CorrectCharacters,
    int TypedCharacters,
    double Accuracy,
    double CharactersPerMinute,
    TimeSpan Elapsed);

public static class SpeedTestCalculator
{
    public static SpeedTestResult Calculate(
        string target,
        string typed,
        TimeSpan elapsed)
    {
        var length = Math.Min(target.Length, typed.Length);
        var correct = 0;
        for (var i = 0; i < length; i++)
        {
            if (target[i] == typed[i])
            {
                correct++;
            }
        }

        var minutes = Math.Max(elapsed.TotalMinutes, 1.0 / 60.0);
        return new SpeedTestResult(
            correct,
            typed.Length,
            typed.Length == 0 ? 0 : correct * 100.0 / typed.Length,
            correct / minutes,
            elapsed);
    }
}
