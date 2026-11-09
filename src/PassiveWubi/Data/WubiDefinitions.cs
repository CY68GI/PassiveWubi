using PassiveWubi.Models;

namespace PassiveWubi.Data;

public static class WubiDefinitions
{
    private const string Zone1 = "#EF725E";
    private const string Zone2 = "#E7AE54";
    private const string Zone3 = "#6BC49A";
    private const string Zone4 = "#65AFE1";
    private const string Zone5 = "#AD8AE5";
    private const string LearnKey = "#D7DFEC";

    private static readonly IReadOnlyDictionary<WubiVersion, IReadOnlyList<WubiKeyDefinition>> Layouts =
        new Dictionary<WubiVersion, IReadOnlyList<WubiKeyDefinition>>
        {
            [WubiVersion.V86] = Build86(),
            [WubiVersion.V98] = Build98(),
            [WubiVersion.NewCentury] = BuildNewCentury()
        };

    public static IReadOnlyList<WubiKeyDefinition> GetLayout(WubiVersion version) =>
        Layouts.TryGetValue(version, out var layout) ? layout : Layouts[WubiVersion.V86];

    public static string GetVersionName(WubiVersion version) => version switch
    {
        WubiVersion.V86 => "86 版",
        WubiVersion.V98 => "98 版",
        WubiVersion.NewCentury => "新世纪版",
        _ => "86 版"
    };

    public static string GetVersionDescription(WubiVersion version) => version switch
    {
        WubiVersion.V86 => "经典稳定，学习资料最丰富",
        WubiVersion.V98 => "字根分布更均衡，重码更少",
        WubiVersion.NewCentury => "规范字形，兼顾生僻与繁体字",
        _ => string.Empty
    };

    private static IReadOnlyList<WubiKeyDefinition> Build86() =>
    [
        Key("Q", "我", "金勺缺点无尾鱼\n犬旁留叉儿一点夕\n氏无七", Zone1),
        Key("W", "人", "人八登祭三四里", Zone1),
        Key("E", "有", "月彡乃用家衣底", Zone1),
        Key("R", "的", "白手看头三二斤", Zone1),
        Key("T", "和", "禾竹一撇双人立\n反文条头共三一", Zone1),
        Key("Y", "主", "言文方广在四一\n高头一捺谁人去", Zone2),
        Key("U", "产", "立辛两点六门疒", Zone2),
        Key("I", "不", "水旁兴头小倒立", Zone2),
        Key("O", "为", "火业头\n四点米", Zone2),
        Key("P", "这", "之宝盖\n摘礻衤", Zone2),
        Key("A", "工", "工戈草头右框七", Zone3),
        Key("S", "要", "木丁西", Zone3),
        Key("D", "在", "大犬三羊古石厂", Zone3),
        Key("F", "地", "土士二干十寸雨", Zone3),
        Key("G", "一", "王旁青头戋五一", Zone3),
        Key("H", "上", "目具上止卜虎皮", Zone4),
        Key("J", "是", "日早两竖与虫依", Zone4),
        Key("K", "中", "口与川，字根稀", Zone4),
        Key("L", "国", "田甲方框四车力", Zone4),
        Key("Z", "", "万能学习键\n用于反查编码", LearnKey),
        Key("X", "经", "慈母无心弓和匕\n幼无力", Zone5),
        Key("C", "以", "又巴马\n经有上\n勇字头", Zone5),
        Key("V", "发", "女刀九臼山朝西", Zone5),
        Key("B", "了", "子耳了也框向上", Zone5),
        Key("N", "民", "已半巳满不出己\n左框折尸心和羽", Zone5),
        Key("M", "同", "山由贝\n下框几", Zone5)
    ];

    private static IReadOnlyList<WubiKeyDefinition> Build98() =>
    [
        Key("Q", "我", "金夕鸟儿犭边鱼", Zone1),
        Key("W", "人", "人八登头单人几", Zone1),
        Key("E", "有", "月用力豸毛衣臼", Zone1),
        Key("R", "的", "白斤气丘叉手提", Zone1),
        Key("T", "和", "禾竹反文双人立", Zone1),
        Key("Y", "主", "言文方点谁人去", Zone2),
        Key("U", "产", "立辛六羊病门里", Zone2),
        Key("I", "不", "水族三点鳖头小", Zone2),
        Key("O", "为", "火业广鹿四点米", Zone2),
        Key("P", "这", "之字宝盖补礻衤", Zone2),
        Key("A", "工", "工戈草头右框七", Zone3),
        Key("S", "要", "木丁西", Zone3),
        Key("D", "在", "大犬戊其古石厂", Zone3),
        Key("F", "地", "土干十寸未甘雨", Zone3),
        Key("G", "一", "王旁青头五夫一", Zone3),
        Key("H", "上", "目上卜止虎头具", Zone4),
        Key("J", "是", "日早两竖与虫依", Zone4),
        Key("K", "中", "口中两川三个竖", Zone4),
        Key("L", "国", "田甲方框四车里", Zone4),
        Key("Z", "", "万能学习键\n用于反查编码", LearnKey),
        Key("X", "经", "幺母贯头弓和匕", Zone5),
        Key("C", "以", "又巴牛厶马失蹄", Zone5),
        Key("V", "发", "女刀九艮山西倒", Zone5),
        Key("B", "了", "子耳了也乃框皮", Zone5),
        Key("N", "民", "已类左框心尸羽", Zone5),
        Key("M", "同", "山由贝骨下框集", Zone5)
    ];

    private static IReadOnlyList<WubiKeyDefinition> BuildNewCentury() =>
    [
        Key("Q", "我", "金勺缺点无尾鱼\n犬旁留叉儿一点夕\n氏无七", Zone1),
        Key("W", "人", "人八登祭三四里", Zone1),
        Key("E", "有", "月彡乃用家衣底", Zone1),
        Key("R", "的", "白手看头三二斤", Zone1),
        Key("T", "和", "禾竹一撇双人立\n反文条头共三一", Zone1),
        Key("Y", "主", "言文方广在四一\n高头一捺谁人去", Zone2),
        Key("U", "产", "立辛两点六门疒", Zone2),
        Key("I", "不", "水旁兴头小倒立", Zone2),
        Key("O", "为", "火业头\n四点米", Zone2),
        Key("P", "这", "之宝盖\n摘礻衤", Zone2),
        Key("A", "工", "工戈草头右框七", Zone3),
        Key("S", "要", "木丁西", Zone3),
        Key("D", "在", "大犬三羊古石厂", Zone3),
        Key("F", "地", "土士二干十寸雨", Zone3),
        Key("G", "一", "王旁青头五一提", Zone3),
        Key("H", "上", "目具上止卜虎皮", Zone4),
        Key("J", "是", "日早两竖与虫依", Zone4),
        Key("K", "中", "口与川，字根稀", Zone4),
        Key("L", "国", "田甲方框四车力", Zone4),
        Key("Z", "", "万能学习键\n用于反查编码", LearnKey),
        Key("X", "经", "慈母无心弓和匕\n幼无力", Zone5),
        Key("C", "以", "又巴马\n经有上\n勇字头", Zone5),
        Key("V", "发", "女刀九臼山朝西", Zone5),
        Key("B", "了", "子耳了也框向上", Zone5),
        Key("N", "民", "已半巳满不出己\n左框折尸心和羽", Zone5),
        Key("M", "同", "山由贝\n下框几", Zone5)
    ];

    private static WubiKeyDefinition Key(
        string key,
        string simpleCode,
        string rootMnemonic,
        string accentColor) =>
        new()
        {
            Key = key,
            SimpleCode = simpleCode,
            RootMnemonic = rootMnemonic,
            AccentColor = accentColor,
            Zone = key == "Z" ? "学习键" : GetZoneName(key, accentColor)
        };

    private static string GetZoneName(string key, string color) => color switch
    {
        Zone1 => "撇区",
        Zone2 => "捺区",
        Zone3 => "横区",
        Zone4 => "竖区",
        Zone5 => "折区",
        _ => key == "Z" ? "学习键" : string.Empty
    };
}
