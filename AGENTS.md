# エージェントガイドライン - DnZip

このドキュメントは、DnZipプロジェクトで作業するAIエージェント向けの重要情報をまとめたものです。DnZipは、暗号化と再帰的なアーカイブをサポートし、Windows標準のアーカイバの代替として設計された .NETベースのCLIツールです。

## ビルド、テスト、およびリンターコマンド

プロジェクトは標準の .NET CLIを使用します。メインプロジェクトは `src/DnZip/DnZip.csproj` にあります。

### ビルド

プロジェクトをビルドする場合：

```bash
dotnet build src/DnZip/DnZip.csproj
```

### 実行

開発中に引数を指定して実行する場合：

```bash
dotnet run --project src/DnZip/DnZip.csproj -- <archiveFilePath> <sourceDirectoryPath> [オプション]
```

利用可能なオプション（`Compress` メソッドのパラメーターに対応）：

- `--recurse` または `-r`: ディレクトリ構造を再帰的にアーカイブします。
- `--encrypt` または `-e`: アーカイブファイルを暗号化します（パスワードの入力が求められます）。

例：

```bash
dotnet run --project src/DnZip/DnZip.csproj -- output.zip ./data --recurse
```

### テスト

現在、このリポジトリにテストはありません。テストを追加する場合は、xUnitを使用し、`tests/` ディレクトリに配置してください。
すべてのテストを実行する場合：

```bash
dotnet test
```

特定のテストを実行する場合：

```bash
dotnet test --filter "FullyQualifiedName=MyNamespace.MyTestClass.MyTestMethod"
```

### リンター / フォーマット

コードスタイルを維持するために、組み込みのdotnetフォーマッタを使用してください：

```bash
dotnet format src/DnZip/DnZip.csproj
```

---

## コードスタイルと規約

### 全般

- **ターゲットフレームワーク**: .NET 9
- **インデント**: スペース2つ（タブ禁止）。これはこのプロジェクトの厳格な要件です。
- **改行コード**: LF（Unixスタイル）を推奨します。
- **中括弧**: クラス、メソッド、制御構造においてAllmanスタイル（新しい行に中括弧を置く）を使用します。

### 命名規則

- **クラス/インターフェイス/メソッド**: `PascalCase` (例: `Program`, `CreateArchive`)。
- **パラメーター/ローカル変数**: `camelCase` (例: `archiveFilePath`, `zip`)。
- **プライベートフィールド**: `_camelCase` (例: `_myField`)。
- **名前空間**: `PascalCase` で、ディレクトリ構造に合わせます (例: `DnZip`)。

### ファイル構造とインポート

- インポート (`using`) はファイルの先頭に置きます。
- 順序: `System` 名前空間を最初、次にサードパーティライブラリ、最後に内部名前空間の順にします。各グループ内はアルファベット順にソートします。
- 名前空間の宣言はブロックスコープスタイルを使用します：
  
  ```csharp
  namespace DnZip
  {
    public class Program { ... }
  }
  ```

### 型と機能

- 最新のC#機能 (C# 13+) を使用します。
- 右辺から型が明らかな場合は `var` を使用します (例: `var sourceDirectory = new DirectoryInfo(path);`)。
- I/Oバウンドな操作には `async/await` を優先します。
- 基本的な文字列検証には `string.IsNullOrEmpty()` を使用します。

### エラー処理

- CLIは成功時に `0`、失敗時に `1` を返すべきです。
- `Main` やコマンドメソッドでのトップレベルの例外はキャッチし、コンソールにログ出力してください。
- 「ファイルが見つからない」などの想定されるユーザーエラーに対して例外をスローするのは避け、明確なメッセージを表示して非ゼロの終了コードを返してください。

---

## 技術スタックとライブラリ

### ZIP操作: DotNetZip (Ionic.Zip)

- ZIP操作には `DotNetZip` を使用します。
- **エンコーディング**: Windowsなどのさまざまな展開ツールとの互換性を確保するため、`ZipFile` を初期化する際は常に `Encoding.GetEncoding("Shift_JIS")` を使用してください。
- **圧縮**: デフォルトで `CompressionLevel` は `BestCompression` に設定します。

### CLIフレームワーク: ConsoleAppFramework

- コマンドはメソッドとして定義されます (例: `Compress`)。
- これらのメソッドのパラメーターは、自動的にCLIの引数/オプションになります。
- エントリポイントは `ConsoleApp.RunAsync(args, Compress)` です。

### 対話型プロンプト: Sharprompt

- パスワードなどの機密情報の入力に使用します。
- 機密データには `Prompt.Password("...")` を使用してください。

---

## ディレクトリ構造

- `src/DnZip/`: プロジェクトのソースコード。
- `src/DnZip/Program.cs`: メインのエントリポイントと圧縮ロジック。
- `src/DnZip.slnx`: XMLベースのソリューションファイル。
- `src/DnZip/bin/` & `src/DnZip/obj/`: ビルド生成物 (git ignore対象）。

---

## エージェント向けのルールとベストプラクティス

1. **インデントの維持**: スペース2つのインデントスタイルを変更しないでください。
2. **エンコーディングプロバイダー**: `Shift_JIS` を使用する前に、必ず `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` を呼び出す必要があります。
3. **相対パス**: ZIPアーカイブにファイルを追加する際は、`Path.GetRelativePath` を使用してアーカイブ内での正しいエントリパスを決定してください。
4. **HACKコメント**: 実装済みの機能でない限り、`HACK` コメントを削除しないでください。
   - `HACK: 複数ファイル指定に対応`
   - `HACK: --no-dir-entries(-D) に対応`
5. **コミュニケーション**: ユーザーとの対話は日本語で行ってください。
6. **Git操作**: ユーザーの明示的な指示がない限り、勝手に `git commit` や `git push` を行わないでください。
7. **アトミックな変更**: 変更は焦点を絞り、アトミックに保ってください。各変更をビルドで検証してください。
8. **ドキュメント**: CLI引数を変更した場合は、新しい構文やオプションを反映するように `README.md` を更新してください。

---

## 検証チェックリスト

- [ ] コードがスペース2つのインデントにしたがっている。
- [ ] `dotnet build` でビルドが成功する。
- [ ] 新しいコンパイラ警告が導入されていない。
- [ ] 終了コードが正しく処理されている（成功時0、エラー時1）。
- [ ] パスワードプロンプトが安全である（`Sharprompt`を使用）。
