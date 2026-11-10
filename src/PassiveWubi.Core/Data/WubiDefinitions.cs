using PassiveWubi.Core;

namespace PassiveWubi.Core;

// Root characters are kept separately from mnemonic text. The keyboard renders
// the key name, level-one code and the root set for the selected Wubi edition.
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
        Key("Q", "金", "我", "钅 儿 尢 勹 犭 乂 巟 鱼 歺 夕 然 ⺈ 卬 乐", Zone1),
        Key("W", "人", "人", "亻 八 癶 祭 共", Zone1),
        Key("E", "月", "有", "舟 用 乃 彡 爫 豸 豕 衣 氏 臼", Zone1),
        Key("R", "白", "的", "手 扌 斤 龵 缶 ⺁ 拜 彦 乒", Zone1),
        Key("T", "禾", "和", "竹 ⺮ 丿 彳 攵 夂 𠂉", Zone1),
        Key("Y", "言", "主", "讠 文 方 广 亠 丶 ㇏ 隹 京 高", Zone2),
        Key("U", "立", "产", "辛 六 门 疒 ⺀ 冫 丷 北 丬 䒑", Zone2),
        Key("I", "水", "不", "氵 小 兴 光 ⺌ ⺍ 兆 承 束", Zone2),
        Key("O", "火", "为", "灬 米 业 兼 亦", Zone2),
        Key("P", "之", "这", "辶 廴 冖 宀 礻 衤", Zone2),
        Key("A", "工", "工", "戈 艹 卄 廿 龷 匚 七 牙 东 弌 弋", Zone3),
        Key("S", "木", "要", "丁 西 覀", Zone3),
        Key("D", "大", "在", "犬 三 羊 古 石 厂 丆 尤 龶 其", Zone3),
        Key("F", "土", "地", "士 二 干 十 寸 雨 ⻗ 革 丑", Zone3),
        Key("G", "王", "一", "一 五 戋 青 龶", Zone3),
        Key("H", "目", "上", "止 卜 虍 皮 具 ⺊ 丨 亅", Zone4),
        Key("J", "日", "是", "曰 早 虫 刂 临 归 ⺜ 円", Zone4),
        Key("K", "口", "中", "川 丿 丨 三", Zone4),
        Key("L", "田", "国", "甲 囗 四 皿 罒 车 力 曾 柬 卌", Zone4),
        Key("Z", "学习键", "", "万 能 学 习 反 查 编 码", LearnKey),
        Key("X", "纟", "经", "丝 幺 母 弓 匕 龙 顷", Zone5),
        Key("C", "又", "以", "巴 马 厶 龴 甬", Zone5),
        Key("V", "女", "发", "刀 九 臼 彐 巛 艮 山", Zone5),
        Key("B", "子", "了", "孑 耳 阝 也 乃 廴 皮", Zone5),
        Key("N", "已", "民", "己 巳 心 忄 尸 羽 乙 乛 ⺄ 乚", Zone5),
        Key("M", "山", "同", "由 贝 冂 几 骨 冖", Zone5)
    ];

    private static IReadOnlyList<WubiKeyDefinition> Build98() =>
    [
        Key("Q", "金", "我", "钅 夕 犭 儿 鱼 鸟 包 ⺈ 勹", Zone1),
        Key("W", "人", "人", "亻 八 癶 祭 几 风", Zone1),
        Key("E", "月", "有", "舟 用 力 豸 毛 衣 豕 臼 𧘇 爫", Zone1),
        Key("R", "白", "的", "斤 气 丘 叉 手 扌 龵", Zone1),
        Key("T", "禾", "和", "竹 ⺮ 攵 彳 丿 𠂉 牛 立", Zone1),
        Key("Y", "言", "主", "讠 文 方 亠 丶 ㇏ 高", Zone2),
        Key("U", "立", "产", "辛 六 羊 疒 门 丷 䒑", Zone2),
        Key("I", "水", "不", "氵 小 兴 光 ⺌ ⺍ 承", Zone2),
        Key("O", "火", "为", "业 广 鹿 灬 米", Zone2),
        Key("P", "之", "这", "辶 廴 宀 冖 礻 衤", Zone2),
        Key("A", "工", "工", "戈 艹 卄 廿 龷 匚 七 牙 东", Zone3),
        Key("S", "木", "要", "丁 西 覀", Zone3),
        Key("D", "大", "在", "犬 戊 其 古 石 厂 丆", Zone3),
        Key("F", "土", "地", "干 十 寸 未 甘 雨 二 革", Zone3),
        Key("G", "王", "一", "青 一 五 夫", Zone3),
        Key("H", "目", "上", "上 卜 止 虍 具 皮", Zone4),
        Key("J", "日", "是", "早 刂 虫 丨", Zone4),
        Key("K", "口", "中", "川 丨 丿", Zone4),
        Key("L", "田", "国", "甲 囗 四 车 力 罒", Zone4),
        Key("Z", "学习键", "", "万 能 学 习 反 查 编 码", LearnKey),
        Key("X", "纟", "经", "幺 母 丝 弓 匕", Zone5),
        Key("C", "又", "以", "巴 牛 厶 马 龴", Zone5),
        Key("V", "女", "发", "刀 九 艮 山 彐", Zone5),
        Key("B", "子", "了", "孑 耳 阝 也 乃 皮 廴", Zone5),
        Key("N", "已", "民", "己 巳 尸 心 忄 羽 乙 乛 ⺄", Zone5),
        Key("M", "山", "同", "由 贝 骨 冂 几 冖", Zone5)
    ];

    private static IReadOnlyList<WubiKeyDefinition> BuildNewCentury() =>
    [
        Key("Q", "金", "我", "钅 夕 犭 儿 包 鱼 ⺈", Zone1),
        Key("W", "人", "人", "亻 八 癶 祭 风 几", Zone1),
        Key("E", "月", "有", "舟 衣 力 豕 豸 臼 𧘇 爫", Zone1),
        Key("R", "白", "的", "斤 气 叉 手 扌 龵", Zone1),
        Key("T", "禾", "和", "竹 牛 牜 卧 人 立", Zone1),
        Key("Y", "言", "主", "讠 文 方 丶 四 一", Zone2),
        Key("U", "立", "产", "辛 丷 疒 门 里", Zone2),
        Key("I", "水", "不", "氵 小 ⺌ ⺍ 灬", Zone2),
        Key("O", "火", "为", "灬 广 二 米 业", Zone2),
        Key("P", "之", "这", "辶 宀 冖 礻 衤", Zone2),
        Key("A", "工", "工", "戈 艹 卄 廿 龷 匚 七", Zone3),
        Key("S", "木", "要", "丁 西 覀 要 无 女", Zone3),
        Key("D", "大", "在", "三 镸 古 石 厂 丆", Zone3),
        Key("F", "土", "地", "士 二 干 十 寸 雨 ⻗", Zone3),
        Key("G", "王", "一", "一 五 青 龶", Zone3),
        Key("H", "目", "上", "止 具 卜 虍 皮", Zone4),
        Key("J", "日", "是", "曰 刂 虫 丨", Zone4),
        Key("K", "口", "中", "川 丨 丿", Zone4),
        Key("L", "田", "国", "囗 四 车 甲 十 罒", Zone4),
        Key("Z", "学习键", "", "万 能 学 习 反 查 编 码", LearnKey),
        Key("X", "纟", "经", "幺 母 丝 弓 匕 巛", Zone5),
        Key("C", "又", "以", "巴 甬 厶 矣 马 龴", Zone5),
        Key("V", "女", "发", "刀 九 巛 录 彐 无 水", Zone5),
        Key("B", "子", "了", "孑 耳 阝 也 乃 齿 廴", Zone5),
        Key("N", "已", "民", "己 巳 彐 心 忄 尸 羽", Zone5),
        Key("M", "山", "同", "由 贝 骨 冂 几 里", Zone5)
    ];

    private static WubiKeyDefinition Key(
        string key,
        string keyName,
        string simpleCode,
        string roots,
        string accentColor) =>
        new()
        {
            Key = key,
            KeyName = keyName,
            SimpleCode = simpleCode,
            Roots = roots.Split(' ', StringSplitOptions.RemoveEmptyEntries),
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
