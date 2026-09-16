# Master Data Importer

Web 公開済みの Google スプレッドシートから CSV を取得し、
`MasterDataCsvImporter` を介して ScriptableObject へ取り込む Editor 拡張です。

## 設定

各プロジェクトで `Assets/ProjectSettings/AltoMasterDataImporter.json` を作成します。
`spreadsheetId` には Web 公開したスプレッドシートの URL の
`https://docs.google.com/spreadsheets/d/e/` から次のスラッシュまでの部分を指定します。

```json
{
  "spreadsheetId": "...",
  "csvPostfix": "MasterTable",
  "csvOutputDirectory": "Assets/Workflow/MasterDataCsv",
  "dataTableOutputDirectory": "Assets/Resources/MasterData",
  "dataNamespace": "YourGame",
  "sheets": [
    {
      "name": "Items",
      "gid": "0"
    },
    ...
  ]
}
```

`name` と `csvPostfix` を連結した名前は、次の3項目に共通して使われます。

- CSV: `{csvOutputDirectory}/{name}{csvPostfix}.csv`
- ScriptableObject: `{dataTableOutputDirectory}/{name}{csvPostfix}.asset`
- 型: `{dataNamespace}.{name}{csvPostfix}`

取り込み先の ScriptableObject は、事前に作成しておく必要があります。

## インポーターの導入方法

AltoLib 自体は Unity メニューを追加しません。このインポーターを使う場合は、
各プロジェクトの Editor コードで、任意のメニューパスからウィンドウを開きます。

```csharp
using AltoLib.Editor;
using UnityEditor;

public static class MasterDataImporterMenu
{
    [MenuItem("Workflow/Master Data Importer")]
    static void ShowWindow()
    {
        MasterDataImporterWindow.Open();
    }
}
```

インポーターは中間ファイルとして CSV を保存し、その後 CSV から ScriptableObject に取り込みます。
CSV の内容が変更されていない場合も、ScriptableObject を再取り込みします。

セル内のコンマと改行は、CSV では `<comma>` と `<br>` に変換して保存されます。
ScriptableObject へ取り込む際に、それぞれ元のコンマと改行へ復元されます。

## 利用例 : データの準備とインポート

例として、以下のような内容の「Sample」というシートを取り込みたいとします：

|id|value|text|
|--|--|--|
|1001|100|hoge|
|1002|200|FUGA|
|1003|303|piYo|

マスタのスキーマファイル、および ScriptableObject 用のクラスとして以下を実装します：

```csharp
using AltoLib;
using UnityEngine;

namespace YourGame
{
    [System.Serializable]
    public class SampleMaster : IMasterDataSchema
    {
        public int id;
        public int value;
        public string text;

        public int PrimaryId => id;
        public string PrimaryKey => null;
    }

    [CreateAssetMenu(
        fileName = "SampleMasterTable",
        menuName = "Master Data/Sample"
    )]
    public class SampleMasterTable : MasterDataTable<SampleMaster>
    {
    }
}
```

- 上記 ScriptableObject アセット（データの取り込み先）を事前に作成しておきます
- json 設定の `sheets` に、`{name: "Sample", gid: "..."}` を追記します
- これで、インポーター上で Sample シートを取り込めるようになります

## 利用例 : コード上からのデータの取得方法

データの取得用クラス（Repository レイヤー）として以下を実装します：

```csharp
using AltoLib;

namespace YourGame
{
    public class SampleRepo : MasterDataRepo<SampleMasterTable, SampleMaster>
    {
        // 必要に応じて取得処理を実装。
        // デフォルトで All(), GetById(), GetByKey() が用意されています
    }
}
```

各マスタの Repository には、何らかのアクセス手段を用意します。
例として以下のようなロケータを実装した場合、

```csharp
using AltoLib;

namespace YourGame
{
    public class MasterDataLocator
    {
        public SampleRepo Sample = new();
        ...

        public void Init()
        {
            var repos = new IMasterDataRepo[]
            {
                Sample,
                ...
            };
            foreach (var repo in repos)
            {
                repo.LoadData();
                repo.PreprocessData();
            }
        }
    }

    public class Game
    {
        public static MasterDataLocator Master { get; private set; }

        public static void Init()
        {
            Master = new();
            Master.Init();
        }
    }
}
```

以下のようにデータを取得できます：

```csharp
SampleMaster data = Game.Master.Sample.GetById(1001);
```

## 標準 Import の仕様

`MasterDataTable<TSchema>.Import()` は、1 行目をヘッダ、それ以降をデータ行として扱います。
ヘッダ名と同名の public インスタンスフィールドをスキーマから検索し、セルの文字列を
フィールド型へ変換して設定します。ヘッダ名とフィールド名は大文字・小文字を区別します。
対応するフィールドがない列はスキップされます。

※ 事前に `MasterDataTable<TSchema>.VerboseLogMode` を
`true` にしておくと、見つからなかったフィールド名をログ出力します。

先頭のセルが `#` で始まる行と、すべてのセルが空の行は取り込みません。
ヘッダ名が `#` で始まる列も無視します。データ行のセル数がヘッダより少ない場合はエラーになります。

### 対応するフィールド型

標準 Import は、以下の型に対応しています。

- `int`
- `long`
- `string`
- `float`
- `double`
- `bool`
- Enum
- 上記を要素型とする `List<T>`

`bool` は `0` を `false`、それ以外の文字列を `true` として扱います。
空のセルは、`string` では空文字列、それ以外の単一値では `0` として変換します。

### Enum の変換

Enum フィールドには、CSV 上の文字列と同名の Enum 識別子を指定します。

```csharp
public enum CardCategory
{
    None,
    LevelUp,
    Skill,
}

public CardCategory category;
```

【シート例】
|category|
|--|
|LevelUp|

識別子は大文字・小文字を区別します。文字列ではなく数値でも指定できますが、
Enumに定義されていない識別子や数値はエラーになります。
空のセルは `0` として扱うため、値 `0` が定義されていない Enum ではエラーになります。

### List の変換

`List<T>` の要素は、スプレッドシートのセル内でコンマ区切りにします。

```csharp
public List<int> values;
public List<CardCategory> categories;
```

【シート例】
|values|categories|
|--|--|
|1,2,3|LevelUp,Skill|

ダウンロードした CSV ではセル内のコンマが一度 `<comma>` に置き換えられ、
ScriptableObject への取り込み時にコンマへ復元された後、List の区切りとして解釈されます。

- セル全体が空の場合は、空の List になります
- 数値や Enum など、`string` 以外の要素は前後の空白を除去してから変換します
- `1,,3` のように途中に空の要素がある場合はエラーになります
- ネストしたList、配列、`HashSet<T>` には対応していません
- `List<string>` の各要素にコンマを含めることはできません
- 未対応の要素型はエラーになります

### 独自変換

標準 Import で扱えない型や、プロジェクト固有の表現を使う場合だけ、
`CustomImport()` を override します。セル内の `<comma>` と `<br>` は、
`CustomImport()`が呼ばれる前にコンマと改行へ復元されています。

```csharp
protected override bool CustomImport(SampleMaster dataRecord, string key, string value)
{
    if (key != nameof(SampleMaster.text)) { return false; }

    dataRecord.text = value.Trim();
    return true;
}
```

変換した場合は `true` を返します。`false` を返したフィールドには標準変換が試行されます。

`Import()` では、すべてのレコードを変換できてから ScriptableObject の `records` を更新します。
途中で変換エラーが発生した場合、以前に正常に取り込まれた `records` は維持されます。
