using System.Text.Json;
using System.Text;

namespace AI_SQL_Project
{
    public class Main
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _apiKey;
        private static string _endpoint = "https://api.anthropic.com/v1/messages";
        private static string _model = "claude-3-5-sonnet-20240620";

        public static void Initialize(string apiKey, string model = "claude-3-5-sonnet-20240620")
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public static async Task<string> SendMessageAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_endpoint))
            {
                throw new InvalidOperationException("AICloudLibrary must be initialized with API key and endpoint before use.");
            }


            try
            {
                var request = new
                {
                    model = _model,
                    max_tokens = 1024,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_endpoint, content);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw Response: {responseBody}"); // Log the raw response

                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // Try to extract the content safely
                if (responseObject.TryGetProperty("content", out var contentElement))
                {
                    if (contentElement.ValueKind == JsonValueKind.Array && contentElement.GetArrayLength() > 0)
                    {
                        var firstContent = contentElement[0];
                        if (firstContent.TryGetProperty("text", out var textElement))
                        {
                            return textElement.GetString();
                        }
                    }
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return string.Empty;
            }
        }
    }


}