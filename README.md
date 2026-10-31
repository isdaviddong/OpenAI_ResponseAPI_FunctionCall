# OpenAI Responses API Function Calling 範例

這是一個使用 C#、.NET 10 與 OpenAI .NET SDK 的最小可執行範例，示範如何透過 **Responses API 的 Function Calling**，讓模型決定何時呼叫由應用程式實作的 C# 函式。

本範例不是把加法交給模型自行計算，而是讓模型產生 `Add` 函式呼叫，程式實際執行 `Add(25, 17)`，再將結果交回模型產生最後回答。這是建立工具呼叫、資料查詢或其他外部系統整合的基本模式。

## 執行流程

1. 程式建立 `ResponsesClient`。
2. 定義一個名為 `Add` 的 Function Tool，並提供 `a`、`b` 兩個整數參數。
3. 將「請幫我計算 25 加 17」送至 Responses API。
4. 模型回傳 Function Call 要求後，程式解析參數並執行本機 C# 方法。
5. 程式將加法結果回傳給模型。
6. 模型輸出最終答案。

## 使用前準備

- .NET 10 SDK
- OpenAI API key
- 可使用 OpenAI API 的網路環境

請先確認 SDK 版本與目標框架：

```bash
dotnet --version
```

專案使用 NuGet 套件 `OpenAI` 2.14.0，執行 `dotnet restore` 時會自動還原。

## 設定 API key

程式會讀取名稱完全相同的環境變數 `OPENAI_API_KEY`。請使用環境變數提供金鑰，不要把 API key 直接寫入 `Program.cs`、README 或提交至 Git。

### Linux / macOS

只在目前終端機工作階段設定：

```bash
export OPENAI_API_KEY="你的 OpenAI API key"
dotnet run
```

若要確認變數是否已設定，但不顯示完整金鑰：

```bash
test -n "$OPENAI_API_KEY" && echo "OPENAI_API_KEY 已設定" || echo "OPENAI_API_KEY 尚未設定"
```

### Windows PowerShell

只在目前 PowerShell 工作階段設定：

```powershell
$env:OPENAI_API_KEY = "你的 OpenAI API key"
dotnet run
```

### Windows 命令提示字元（cmd）

```cmd
set OPENAI_API_KEY=你的 OpenAI API key
dotnet run
```

也可以使用 .NET User Secrets 或作業系統的永久環境變數管理金鑰；請避免將包含真實金鑰的 `.env` 檔案提交至版本控制。

## 執行範例

在專案根目錄執行：

```bash
dotnet run
```

預期會看到類似輸出：

```text
[Function Call] Add(25, 17)
AI 回答：25 加 17 等於 42。
```

實際文字可能因模型回應而不同，但應該會先看到本機 `Add` 函式的執行紀錄，再看到模型的最終回答。

## 重要程式碼概念

- `ResponseTool.CreateFunctionTool(...)`：描述模型可以使用的函式名稱、用途與 JSON Schema 參數。
- `response.OutputItems`：保留模型輸出，包含 Function Call；這些項目會加入下一輪輸入。
- `FunctionCallResponseItem`：代表模型要求程式執行函式。
- `FunctionCallOutputResponseItem`：將本機函式的結果交回模型。
- `while` 迴圈：持續處理 Function Call，直到模型不再要求工具並產生最終回答。

## 常見問題

### 出現「請設定 OPENAI_API_KEY」

表示目前執行 `dotnet run` 的終端機沒有讀到環境變數。請依照上方對應作業系統的指令設定，並在同一個終端機執行程式。

### API 回傳驗證或權限錯誤

請確認 API key 有效、帳戶具備 API 使用權限，且沒有多餘的引號或空白字元。也請確認使用的模型 `gpt-4o-mini` 可供該 API key 使用。

### 安全提醒

API key 等同於密碼。若金鑰曾被貼到公開內容或提交到 Git，請立即在 OpenAI 平台撤銷並重新建立，不要只刪除檔案後繼續使用原金鑰。