[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$TablesRoot,
    [string]$CommonCharsPath = '',
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $projectRoot 'src\PassiveWubi.Core\Data\practice-data.json'
}

$definitionPath = Join-Path $projectRoot 'src\PassiveWubi.Core\Data\WubiDefinitions.cs'
$definitionSource = Get-Content -LiteralPath $definitionPath -Raw
$versionSources = @{
    V86 = [regex]::Match($definitionSource, 'Build86\(\).*?(?=private static IReadOnlyList<WubiKeyDefinition> Build98)', 'Singleline').Value
    V98 = [regex]::Match($definitionSource, 'Build98\(\).*?(?=private static IReadOnlyList<WubiKeyDefinition> BuildNewCentury)', 'Singleline').Value
    NewCentury = [regex]::Match($definitionSource, 'BuildNewCentury\(\).*?(?=private static WubiKeyDefinition Key)', 'Singleline').Value
}
$tableFolders = @{ V86 = '86'; V98 = '98'; NewCentury = '06' }

$simpleCodes = @{
    G = '一'; F = '地'; D = '在'; S = '要'; A = '工'
    H = '上'; J = '是'; K = '中'; L = '国'; M = '同'
    T = '和'; R = '的'; E = '有'; W = '人'; Q = '我'
    Y = '主'; U = '产'; I = '不'; O = '为'; P = '这'
    N = '民'; B = '了'; V = '发'; C = '以'; X = '经'
}

$keyNames = @{
    G = '王'; F = '土'; D = '大'; S = '木'; A = '工'
    H = '目'; J = '日'; K = '口'; L = '田'; M = '山'
    T = '禾'; R = '白'; E = '月'; W = '人'; Q = '金'
    Y = '言'; U = '立'; I = '水'; O = '火'; P = '之'
    N = '已'; B = '子'; V = '女'; C = '又'; X = '纟'
}

$mnemonics = @{
    V86 = [ordered]@{
        G = '王旁青头戋五一'; F = '土士二干十寸雨'; D = '大犬三羊古石厂'; S = '木丁西'; A = '工戈草头右框七'
        H = '目具上止卜虎皮'; J = '日早两竖与虫依'; K = '口与川，字根稀'; L = '田甲方框四车力'; M = '山由贝，下框几'
        T = '禾竹一撇双人立，反文条头共三一'; R = '白手看头三二斤'; E = '月彡乃用家衣底'; W = '人八登祭三四里'; Q = '金勺缺点无尾鱼，犬旁留叉儿一点夕，氏无七'
        Y = '言文方广在四一，高头一捺谁人去'; U = '立辛两点六门疒'; I = '水旁兴头小倒立'; O = '火业头，四点米'; P = '之宝盖，摘礻衤'
        N = '已半巳满不出己，左框折尸心和羽'; B = '子耳了也框向上'; V = '女刀九臼山朝西'; C = '又巴马，经有上，勇字头'; X = '慈母无心弓和匕，幼无力'
    }
    V98 = [ordered]@{
        G = '王旁青头五夫一'; F = '土干十寸未甘雨'; D = '大犬戊其古石厂'; S = '木丁西'; A = '工戈草头右框七'
        H = '目上卜止虎头具'; J = '日早两竖与虫依'; K = '口中两川三个竖'; L = '田甲方框四车里'; M = '山由贝骨下框集'
        T = '禾竹反文双人立'; R = '白斤气丘叉手提'; E = '月用力豸毛衣臼'; W = '人八登头单人几'; Q = '金夕鸟儿犭边鱼'
        Y = '言文方点谁人去'; U = '立辛六羊病门里'; I = '水族三点鳖头小'; O = '火业广鹿四点米'; P = '之字宝盖补礻衤'
        N = '已类左框心尸羽'; B = '子耳了也乃框皮'; V = '女刀九艮山西倒'; C = '又巴牛厶马失蹄'; X = '幺母贯头弓和匕'
    }
    NewCentury = [ordered]@{
        G = '王旁青头五一提'; F = '土士二干十寸雨'; D = '大三肆头古石厂'; S = '木丁西边要无女'; A = '工戈草头右框七'
        H = '目止具头卜虎皮'; J = '日曰两竖与虫依'; K = '口中两川三个竖'; L = '田框四车甲单底'; M = '山由贝骨下框里'
        T = '禾竹牛旁卧人立'; R = '白斤气头叉手提'; E = '月舟衣力豕豸臼'; W = '人八登祭风头几'; Q = '金夕犭儿包头鱼'
        Y = '言文方点在四一'; U = '立带两点病门里'; I = '水边一族三点小'; O = '火变三态广二米'; P = '之字宝盖补示衣'
        N = '已类左框心尸羽'; B = '子耳了也乃齿底'; V = '女刀九巡录无水'; C = '又巴甬矣马失蹄'; X = '幺母绞丝弓三匕'
    }
}

$identification = '仁仅位什付作件休何们仙但住你他低佳使供俊信倍借值停偏'
$difficult = '凹凸舞藏鼠鼎赢繁陶囊餐健幽卵州鹿垂黎微承拜翠勤貌谁赛赢攀'
$hanRegex = [regex]::new('^[\p{IsCJKUnifiedIdeographs}\u3400-\u4DBF]+$', [Text.RegularExpressions.RegexOptions]::Compiled)

function Test-Han([string]$Text) {
    return $Text.Length -gt 0 -and $hanRegex.IsMatch($Text)
}

function Get-Dictionary([string]$Path) {
    $entries = foreach ($line in [IO.File]::ReadLines($Path)) {
        if ($line -match '^\s*#' -or $line -match '^\.\.\.$' -or $line -match '^---$') {
            continue
        }

        $parts = $line -split "`t"
        if ($parts.Count -lt 3 -or $parts[0] -notmatch '^[^\t]+$' -or $parts[1] -notmatch '^[a-z]+$') {
            continue
        }

        $weight = 0.0
        [void][double]::TryParse($parts[2], [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$weight)
        if ($weight -le 0 -or -not (Test-Han $parts[0])) {
            continue
        }

        [pscustomobject]@{
            Text = $parts[0]
            Code = $parts[1]
            Weight = $weight
        }
    }

    return @($entries)
}

function Select-Practice([object[]]$Items, [string]$Category, [string]$CodePattern, [int]$Count) {
    return @($Items |
        Where-Object { $_.Code -match $CodePattern } |
        Group-Object Text |
        ForEach-Object { $_.Group | Sort-Object Weight -Descending | Select-Object -First 1 } |
        Sort-Object Weight -Descending |
        Select-Object -First $Count |
        ForEach-Object {
            [ordered]@{
                prompt = $_.Text
                code = $_.Code
                category = $Category
            }
        })
}

$result = [ordered]@{}
foreach ($version in 'V86', 'V98', 'NewCentury') {
    Write-Host "Processing $version dictionary..."
    $allEntries = Get-Dictionary (Join-Path $TablesRoot "$($tableFolders[$version])\wubi.dict.yaml")
    Write-Host "  Parsed $($allEntries.Count) entries."
    $singleEntries = @($allEntries | Where-Object { $_.Text.Length -eq 1 })
    $byChar = @{}
    foreach ($item in ($singleEntries | Group-Object Text)) {
        $byChar[$item.Name] = @($item.Group | Sort-Object Weight -Descending | Select-Object -First 1)[0]
    }

    $rootDefinitions = $versionSources[$version]
    $rootItems = @{}
    $formingRoots = @{}
    foreach ($match in [regex]::Matches($rootDefinitions, 'Key\("([A-Z])",\s*"[^"]+",\s*"[^"]*",\s*"([^"]*)"')) {
        $key = $match.Groups[1].Value
        if ($key -eq 'Z') { continue }
        foreach ($root in ($match.Groups[2].Value -split ' ' | Where-Object { $_ })) {
            if ($byChar.ContainsKey($root)) {
                $rootItems[$root] = [ordered]@{
                    prompt = $root
                    code = $key.ToLowerInvariant()
                    category = $key
                }
                if ($root -ne $keyNames[$key]) {
                    $formingRoots[$root] = [ordered]@{
                        prompt = $root
                        code = $byChar[$root].Code
                        category = '成字字根'
                    }
                }
            }
        }
    }

    $keyNameItems = @($keyNames.GetEnumerator() | ForEach-Object {
        [ordered]@{
            prompt = $_.Value
            code = ($_.Key.ToString() * 4)
            category = $_.Key
        }
    })
    $level1Items = @($simpleCodes.GetEnumerator() | ForEach-Object {
        [ordered]@{
            prompt = $_.Value
            code = $_.Key.ToString().ToLowerInvariant()
            category = $_.Key
        }
    })
    $identificationItems = @($identification.ToCharArray() | Where-Object { $byChar.ContainsKey([string]$_) } | ForEach-Object {
        $entry = $byChar[[string]$_]
        [ordered]@{ prompt = $entry.Text; code = $entry.Code; category = '识别码' }
    })
    $difficultItems = @($difficult.ToCharArray() | Where-Object { $byChar.ContainsKey([string]$_) } | ForEach-Object {
        $entry = $byChar[[string]$_]
        [ordered]@{ prompt = $entry.Text; code = $entry.Code; category = '难拆字' }
    })
    $mnemonicItems = @($mnemonics[$version].GetEnumerator() | ForEach-Object {
        [ordered]@{ key = $_.Key; text = $_.Value }
    })
    $commonPractice = if (-not [string]::IsNullOrWhiteSpace($CommonCharsPath) -and (Test-Path -LiteralPath $CommonCharsPath)) {
        $orderedCommon = ((([IO.File]::ReadAllText($CommonCharsPath)) -replace '\s+', '').ToCharArray() |
            Select-Object -First 500 |
            ForEach-Object { [string]$_ } |
            Select-Object -Unique)
        @($orderedCommon | Where-Object { $byChar.ContainsKey($_) } | Select-Object -First 260 | ForEach-Object {
            [ordered]@{
                prompt = $_
                code = $byChar[$_].Code
                category = '常用字'
            }
        })
    }
    else {
        Select-Practice $singleEntries '常用字' '^[a-z]{4}$' 160
    }

    $result[$version] = [ordered]@{
        keyNames = $keyNameItems
        rootChars = @($rootItems.Values | Select-Object -First 100)
        formingRoots = @($formingRoots.Values | Select-Object -First 100)
        commonChars = $commonPractice
        level1 = $level1Items
        level2 = Select-Practice $singleEntries '二级简码' '^[a-z]{2}$' 100
        level3 = Select-Practice $singleEntries '三级简码' '^[a-z]{3}$' 100
        identification = $identificationItems
        difficult = $difficultItems
        mnemonics = $mnemonicItems
        words2 = Select-Practice @($allEntries | Where-Object { $_.Text.Length -eq 2 }) '二字词组' '^[a-z]{4}$' 60
        words3 = Select-Practice @($allEntries | Where-Object { $_.Text.Length -eq 3 }) '三字词组' '^[a-z]{4}$' 60
        words4 = Select-Practice @($allEntries | Where-Object { $_.Text.Length -eq 4 }) '四字词组' '^[a-z]{4}$' 60
    }
    Write-Host "  Practice categories prepared."
}

$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$json = $result | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($OutputPath, $json, [Text.UTF8Encoding]::new($false))
Write-Host "Practice data generated: $OutputPath"
