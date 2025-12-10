using MCP_Server.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MCP_Server.Helpers
{
    public static class ToolPromptBuilder
    {


        public static string BuildPrompt(IToolService toolService, string userQuery)
        {
            // Step 1: Get all tools
            var toolDefs = toolService.GetToolDefinitions();
            var sb = new StringBuilder();

            // Step 2: Instructions
            sb.AppendLine("Map the user query to a tool. Return ONLY valid JSON. NO extra text.");
            sb.AppendLine("The JSON format must be:");
            sb.AppendLine(@"
{
  ""Tool"": ""<ToolName>"",
  ""Parameters"": {
    ""param1"": ""value"",
    ""param2"": [""value1"", ""value2""]
  }
}");
            sb.AppendLine();

            sb.AppendLine("Available tools and parameters:");

            // Step 3: Include tool definitions
            foreach (var tool in toolDefs)
            {
                try
                {
                    var toolDict = tool as dynamic;
                    string toolName = toolDict.name ?? "UnknownTool";
                    string toolDesc = toolDict.description ?? "No description";

                    sb.Append($"- {toolName}");

                    var parametersObj = toolDict.parameters?.properties;
                    if (parametersObj is IDictionary<string, object> props && props.Count > 0)
                    {
                        var paramList = new List<string>();
                        foreach (var kvp in props)
                        {
                            try
                            {
                                var p = kvp.Value as dynamic;
                                string desc = p?.description ?? kvp.Key;
                                paramList.Add($"{kvp.Key}: {desc}");
                            }
                            catch
                            {
                                paramList.Add(kvp.Key);
                            }
                        }

                        sb.Append($"({string.Join(", ", paramList)})");
                    }
                    else
                    {
                        sb.Append("(no parameters)");
                    }

                    sb.AppendLine($" - {toolDesc}");
                }
                catch
                {
                    // Ignore tools that fail at runtime
                    sb.AppendLine("- <Failed to read tool metadata>");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Examples:");

            // Add examples dynamically (safe version)
            foreach (var tool in toolDefs)
            {
                try
                {
                    var toolDict = tool as dynamic;
                    string toolName = toolDict.name ?? "UnknownTool";

                    var parametersObj = toolDict.parameters?.properties;
                    string exampleParams = "{}";

                    if (parametersObj is IDictionary<string, object> props && props.Count > 0)
                    {
                        var paramPairs = new List<string>();
                        foreach (var kvp in props)
                        {
                            string value = $"<value_for_{kvp.Key}>";
                            paramPairs.Add($"\"{kvp.Key}\":\"{value}\"");
                        }

                        exampleParams = "{" + string.Join(",", paramPairs) + "}";
                    }

                    sb.AppendLine($@"""example query for {toolName}"" -> {{""Tool"":""{toolName}"",""Parameters"":{exampleParams}}}");
                }
                catch
                {
                    sb.AppendLine($@"""example query for UnknownTool"" -> {{""Tool"":""UnknownTool"",""Parameters"":{{}}}}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($@"""User query: {userQuery}"" -> ");

            return sb.ToString();
        }
            
        



        public static string CleanJsonResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return null;

            response = response.Trim();

            Console.WriteLine($"[1] Raw response: {response}");

            // Remove markdown code blocks
            response = Regex.Replace(response, @"```json\s*", "");
            response = Regex.Replace(response, @"```\s*", "");

            // CRITICAL: Remove \r and \t that are breaking the JSON
            response = response.Replace("\\r", "");
            response = response.Replace("\\t", "");
            response = response.Replace("\\n", "");

            // Also remove actual whitespace control characters
            response = response.Replace("\r", "");
            response = response.Replace("\t", "");
            response = response.Replace("\n", " ");

            Console.WriteLine($"[2] After cleaning control chars: {response}");

            // Remove wrapping quotes if present
            if (response.StartsWith("\"") && response.EndsWith("\"") && response.Length > 2)
            {
                response = response.Substring(1, response.Length - 2);
                Console.WriteLine($"[3] After removing wrapping quotes: {response}");
            }

            // Unescape any escaped quotes
            if (response.Contains("\\\""))
            {
                response = response.Replace("\\\"", "\"");
                Console.WriteLine($"[4] After unescaping quotes: {response}");
            }

            // Clean up any extra whitespace
            response = Regex.Replace(response, @"\s+", " ").Trim();

            Console.WriteLine($"[5] Final cleaned: {response}");

            // Extract JSON object
            var match = Regex.Match(response, @"\{[^\}]+\}");

            if (match.Success)
            {
                var json = match.Value;
                Console.WriteLine($"[6] Extracted JSON: {json}");

                // Validate it's proper JSON
                try
                {
                    JsonDocument.Parse(json);
                    Console.WriteLine($"[7] ✓ JSON is valid!");
                    return json;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[7] ❌ JSON validation failed: {ex.Message}");
                    return null;
                }
            }

            Console.WriteLine($"[6] ❌ No valid JSON found");
            return null;
        }

    }


}