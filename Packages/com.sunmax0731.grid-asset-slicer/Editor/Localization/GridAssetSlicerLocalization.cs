using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sunmax.GridAssetSlicer.Editor.Localization
{
    public static class GridAssetSlicerLocalization
    {
        private static readonly IReadOnlyDictionary<string, string> Japanese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["language"] = "表示言語",
            ["language.auto"] = "自動",
            ["language.japanese"] = "日本語",
            ["language.english"] = "英語",
            ["sourceImage"] = "元画像",
            ["preview"] = "プレビュー",
            ["export"] = "書き出し...",
            ["saveSession"] = "セッション保存",
            ["loadSession"] = "セッション読込",
            ["gridSettings"] = "グリッド設定",
            ["rows"] = "行数",
            ["columns"] = "列数",
            ["marginLeft"] = "左余白",
            ["marginTop"] = "上余白",
            ["marginRight"] = "右余白",
            ["marginBottom"] = "下余白",
            ["gutterX"] = "横方向の隙間",
            ["gutterY"] = "縦方向の隙間",
            ["cellWidth"] = "セル幅",
            ["cellHeight"] = "セル高さ",
            ["output"] = "出力",
            ["outputFolder"] = "出力フォルダ",
            ["outputPrefix"] = "出力接頭辞",
            ["startIndex"] = "開始番号",
            ["serialDigits"] = "連番桁数",
            ["conflictMode"] = "衝突時の動作",
            ["qualityChecks"] = "品質チェック",
            ["gridBounds"] = "グリッド範囲",
            ["readableSource"] = "読込可能な元画像",
            ["outputSettings"] = "出力設定",
            ["includedCells"] = "対象セル",
            ["display"] = "表示",
            ["parameterHelp"] = "パラメータヘルプ",
            ["displayHelp"] = "左に設定、中央にグリッド、右にインスペクタ、下に品質レポートを表示します。",
            ["previewTitle"] = "プレビュー ({0})",
            ["noSourceImage"] = "元画像なし",
            ["selectSource"] = "プレビューするPNGテクスチャを選択してください。",
            ["cellInspector"] = "セルインスペクタ",
            ["selectCell"] = "プレビュー内のセルを選択してください。",
            ["index"] = "番号",
            ["coordinate"] = "座標",
            ["include"] = "含める",
            ["bounds"] = "範囲",
            ["outputFile"] = "出力ファイル",
            ["sessionInfo"] = "セッション情報",
            ["source"] = "元画像",
            ["detectedGrid"] = "検出グリッド",
            ["totalCells"] = "総セル数",
            ["included"] = "対象",
            ["excluded"] = "除外",
            ["qualityReport"] = "品質チェックレポート",
            ["cell"] = "セル",
            ["status"] = "状態",
            ["details"] = "詳細",
            ["pass"] = "成功",
            ["fail"] = "失敗",
            ["disabled"] = "無効",
            ["exported"] = "出力済み",
            ["skipped"] = "スキップ",
            ["error"] = "エラー",
            ["quality.disabled"] = "この品質チェックはOFFです。",
            ["quality.grid.pass"] = "グリッドは元画像の範囲内です。",
            ["quality.readable.pass"] = "元画像のピクセルを読み込めます。",
            ["quality.readable.fail"] = "元画像が未選択、または読み込めません。書き出しに失敗する可能性があります。",
            ["quality.output.pass"] = "出力設定は有効です。",
            ["quality.included.pass"] = "{0} 個のセルが出力対象です。",
            ["quality.included.fail"] = "出力対象のセルがありません。",
            ["help.sourceImage"] = "Source Image はプレビューと書き出しに使うテクスチャアセットです。Language はツールの表示言語を切り替えます。",
            ["help.rows"] = "元画像内の縦方向のセル数です。",
            ["help.columns"] = "元画像内の横方向のセル数です。",
            ["help.marginLeft"] = "画像左端からグリッド開始位置まで除外するピクセル数です。",
            ["help.marginTop"] = "画像上端からグリッド開始位置まで除外するピクセル数です。",
            ["help.marginRight"] = "グリッド終了位置から画像右端まで除外するピクセル数です。",
            ["help.marginBottom"] = "グリッド終了位置から画像下端まで除外するピクセル数です。",
            ["help.gutterX"] = "隣り合うセル間の横方向の隙間をピクセルで指定します。",
            ["help.gutterY"] = "隣り合うセル間の縦方向の隙間をピクセルで指定します。",
            ["help.cellWidth"] = "セル幅を明示指定します。OFF にすると、画像サイズ、余白、隙間、列数から自動計算します。",
            ["help.cellHeight"] = "セル高さを明示指定します。OFF にすると、画像サイズ、余白、隙間、行数から自動計算します。",
            ["help.outputFolder"] = "生成した PNG の保存先です。Unity プロジェクトからの相対パスで扱います。",
            ["help.outputPrefix"] = "生成ファイル名の連番前に付ける文字列です。",
            ["help.startIndex"] = "書き出しファイル名に使う最初の連番です。",
            ["help.serialDigits"] = "連番の最小桁数です。3 の場合は 001 のようにゼロ埋めします。",
            ["help.conflictMode"] = "同名ファイルが既にある場合に、上書き、スキップ、複製名作成のどれで処理するかを指定します。",
            ["help.gridBounds"] = "計算されたグリッドが元画像の範囲内に収まっているかを確認します。",
            ["help.readableSource"] = "PNG 書き出しのため、Unity が元画像のピクセルを読み取れるかを確認します。",
            ["help.outputSettings"] = "出力フォルダとファイル名設定から有効な出力パスを作れるかを確認します。",
            ["help.includedCells"] = "書き出し対象として含まれているセルが 1 つ以上あるかを確認します。",
            ["help.parameterHelp"] = "操作やプレビューを止めずに、各入力項目の説明をインライン表示します。"
        };

        public static GridAssetSlicerDisplayLanguage ResolveLanguage(GridAssetSlicerLanguageMode mode)
        {
            return mode switch
            {
                GridAssetSlicerLanguageMode.Japanese => GridAssetSlicerDisplayLanguage.Japanese,
                GridAssetSlicerLanguageMode.English => GridAssetSlicerDisplayLanguage.English,
                _ => Application.systemLanguage == SystemLanguage.Japanese
                    ? GridAssetSlicerDisplayLanguage.Japanese
                    : GridAssetSlicerDisplayLanguage.English
            };
        }

        public static string Get(GridAssetSlicerDisplayLanguage language, string key, string englishText)
        {
            if (language == GridAssetSlicerDisplayLanguage.Japanese
                && Japanese.TryGetValue(key, out var translated))
            {
                return translated;
            }

            return englishText;
        }

        public static string Format(GridAssetSlicerDisplayLanguage language, string key, string englishFormat, params object[] args)
        {
            return string.Format(Get(language, key, englishFormat), args);
        }

        public static string GetLanguageModeLabel(GridAssetSlicerDisplayLanguage language, GridAssetSlicerLanguageMode mode)
        {
            return mode switch
            {
                GridAssetSlicerLanguageMode.Auto => Get(language, "language.auto", "Auto"),
                GridAssetSlicerLanguageMode.Japanese => Get(language, "language.japanese", "Japanese"),
                GridAssetSlicerLanguageMode.English => Get(language, "language.english", "English"),
                _ => mode.ToString()
            };
        }
    }
}
