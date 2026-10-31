
using System.Text.Json;
using OpenAI.Responses;

#pragma warning disable OPENAI001

// 1. 建立 OpenAI Client
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new Exception("請設定 OPENAI_API_KEY");

ResponsesClient client = new(apiKey);

// 2. 定義 Function Tool
var addTool = ResponseTool.CreateFunctionTool(
    functionName: nameof(Add),
    functionDescription: "計算兩個整數的總和",
    functionParameters: BinaryData.FromBytes("""
    {
        "type": "object",
        "properties": {
            "a": { "type": "integer", "description": "第一個整數" },
            "b": { "type": "integer", "description": "第二個整數" }
        },
        "required": ["a", "b"],
        "additionalProperties": false
    }
    """u8.ToArray()),
    strictModeEnabled: true
);

// 3. 使用自然語言呼叫 Responses API
List<ResponseItem> inputItems =
[
    ResponseItem.CreateUserMessageItem("請幫我計算 25 加 17")
];

while (true)
{
    CreateResponseOptions options = new("gpt-4o-mini", inputItems)
    {
        Instructions = "你是數學助理，計算加法時必須使用 Add 函式。",
        Tools = { addTool }
    };

    ResponseResult response = await client.CreateResponseAsync(options);

    // 保留模型輸出，包括 Function Call
    inputItems.AddRange(response.OutputItems);

    bool functionCalled = false;

    // 4. 檢查模型是否要求執行 Function
    foreach (var item in response.OutputItems)
    {
        if (item is FunctionCallResponseItem call)
        {
            if (call.FunctionName != nameof(Add))
                throw new Exception("未知的 Function");

            using JsonDocument json = JsonDocument.Parse(call.FunctionArguments);

            int a = json.RootElement.GetProperty("a").GetInt32();
            int b = json.RootElement.GetProperty("b").GetInt32();

            // 5. 執行自己撰寫的 C# Function
            int sum = Add(a, b);

            // 6. 將結果交回模型
            inputItems.Add(new FunctionCallOutputResponseItem(
                call.CallId,
                sum.ToString()
            ));

            functionCalled = true;
        }
    }

    // 7. 沒有 Function Call，代表取得最終回答
    if (!functionCalled)
    {
        Console.WriteLine($"AI 回答：{response.GetOutputText()}");
        break;
    }
}

#pragma warning restore OPENAI001

// 8. 自己撰寫的 C# Function
static int Add(int a, int b)
{
    Console.WriteLine($"[Function Call] Add({a}, {b})");
    return a + b;
}
