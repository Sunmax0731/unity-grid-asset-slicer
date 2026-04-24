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
            ["previewWindow"] = "別窓プレビュー",
            ["detachedPreviewTitle"] = "グリッドプレビュー",
            ["openPreviewWindow"] = "プレビューウィンドウを開く",
            ["detachedPreviewReconnect"] = "Tools > Grid Asset Slicer > Open を開き直して、プレビューを再接続してください。",
            ["detachedHelpReconnect"] = "Tools > Grid Asset Slicer > Open を開き直して、ヘルプを再接続してください。",
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
            ["help"] = "ヘルプ",
            ["parameterHelp"] = "パラメータヘルプ",
            ["openHelpWindow"] = "ヘルプウィンドウを開く",
            ["displayHelp"] = "メインウィンドウには設定とインスペクタを表示し、グリッドプレビューは別ウィンドウで確認します。",
            ["workspace"] = "ワークスペース",
            ["previewTitle"] = "プレビュー ({0})",
            ["detachedPreviewHelp"] = "プレビューは別の可変サイズウィンドウに表示します。設定を編集しながらセルを確認できます。",
            ["noSourceImage"] = "元画像なし",
            ["selectSource"] = "プレビューするPNGテクスチャを選択してください。",
            ["noPreviewCells"] = "現在の設定では描画できるプレビューセルがありません。",
            ["cellInspector"] = "セルインスペクタ",
            ["selectCell"] = "プレビュー内のセルを選択してください。",
            ["index"] = "番号",
            ["coordinate"] = "座標",
            ["coordinateValue"] = "行 {0}, 列 {1}",
            ["include"] = "含める",
            ["bounds"] = "範囲",
            ["outputFile"] = "出力ファイル",
            ["inspectorPreviewSettings"] = "プレビュー表示",
            ["showOutline"] = "アウトラインを表示",
            ["backgroundColor"] = "背景色",
            ["outlineColor"] = "アウトライン色",
            ["sessionInfo"] = "セッション情報",
            ["source"] = "元画像",
            ["detectedGrid"] = "検出グリッド",
            ["totalCells"] = "総セル数",
            ["included"] = "対象",
            ["excluded"] = "除外",
            ["qualityReport"] = "品質チェックレポート",
            ["reportHeight"] = "レポート高さ",
            ["latestExportSummary"] = "最新の書き出し",
            ["noExportResult"] = "まだ書き出し結果はありません。",
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
            ["quality.readable.fail"] = "元画像を直接読み込めません。書き出し時は一時的な読み取り可能コピーを使用します。",
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
            ["help.readableSource"] = "PNG 書き出しのため、Unity が元画像のピクセルを直接読み取れるかを確認します。直接読めない場合は一時コピーを読み取り可能にして処理します。",
            ["help.outputSettings"] = "出力フォルダとファイル名設定から有効な出力パスを作れるかを確認します。",
            ["help.includedCells"] = "書き出し対象として含まれているセルが 1 つ以上あるかを確認します。",
            ["help.parameterHelp"] = "操作やプレビューを止めずに、各入力項目の説明を別ウィンドウに表示します。",
            ["help.inspectorPreview"] = "セルインスペクタ内の選択セルプレビューの背景色とアウトライン表示を調整します。",
            ["status.sourceCleared"] = "元画像を解除しました。",
            ["status.sourceSelected"] = "元画像を選択しました: {0}",
            ["status.exportNoCells"] = "セルを計算できないため、書き出しを中止しました。",
            ["status.exported"] = "{0} 件を書き出し、{1} 件をスキップしました。",
            ["status.exportFailed"] = "書き出しは完了しましたが、{0} 件のエラーがあります。",
            ["status.selectSourceBeforeSave"] = "セッション保存前に元画像を選択してください。",
            ["status.savedSession"] = "セッションを保存しました: {0}",
            ["status.sessionLoadFailed"] = "セッションの読み込みに失敗しました: {0}",
            ["status.loadedSession"] = "セッションを読み込みました: {0}",
            ["dialog.saveSessionTitle"] = "スライスセッションを保存",
            ["dialog.saveSessionMessage"] = "スライスセッション JSON を保存します。",
            ["error.gridSettingsRequired"] = "グリッド設定が必要です。",
            ["error.imageWidthPositive"] = "画像幅は 0 より大きい必要があります。",
            ["error.imageHeightPositive"] = "画像高さは 0 より大きい必要があります。",
            ["error.rowsPositive"] = "行数は 0 より大きい必要があります。",
            ["error.columnsPositive"] = "列数は 0 より大きい必要があります。",
            ["error.cellWidthPositive"] = "セル幅を指定する場合は 0 より大きい必要があります。",
            ["error.cellHeightPositive"] = "セル高さを指定する場合は 0 より大きい必要があります。",
            ["error.availableWidthPositive"] = "利用可能なグリッド幅は 0 より大きい必要があります。",
            ["error.availableHeightPositive"] = "利用可能なグリッド高さは 0 より大きい必要があります。",
            ["error.availableWidthDivisible"] = "利用可能なグリッド幅をセル数で割り切れません。",
            ["error.availableHeightDivisible"] = "利用可能なグリッド高さをセル数で割り切れません。",
            ["error.outputFolderRequired"] = "出力フォルダが必要です。",
            ["error.startIndexNonNegative"] = "開始番号は 0 以上である必要があります。",
            ["error.numberPaddingNonNegative"] = "連番桁数は 0 以上である必要があります。",
            ["error.exportRequestRequired"] = "PNG 書き出しリクエストが必要です。",
            ["error.sourceTextureRequired"] = "元画像テクスチャが必要です。",
            ["error.fieldNonNegative"] = "{0} は 0 以上である必要があります。",
            ["error.gridWidthExceeds"] = "グリッド幅が元画像の範囲を超えています。",
            ["error.gridHeightExceeds"] = "グリッド高さが元画像の範囲を超えています。",
            ["error.exportBlockedCell"] = "セル {0} の書き出しがブロックされました。",
            ["error.cellNotFound"] = "書き出し対象リスト内にセル {0} が見つかりません。",
            ["error.failedToExportCell"] = "セル {0} を {1} に書き出せませんでした。原因: {2}",
            ["error.sessionValidation"] = "セッション検証に失敗しました。詳細: {0}"
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
