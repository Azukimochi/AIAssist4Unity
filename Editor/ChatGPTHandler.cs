using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace io.github.azukimochi 
{
    
    public class ChatGPTHandler : EditorWindow
    {
        public static (string, List<Dictionary<string, string>>) Completion(string newMessageText, string settingsText = "",
            string api_key = "", string api_url = "", string model = "",List<Dictionary<string, string>> pastMessages = null)
        {
            if (pastMessages == null)
            {
                pastMessages = new List<Dictionary<string, string>>();
            }

            if (pastMessages.Count == 0 && !string.IsNullOrEmpty(settingsText))
            {
                var systemMessage = new Dictionary<string, string>
                {
                    { "role", "system" },
                    { "content", settingsText }
                };
                pastMessages.Add(systemMessage);
            }

            var newMessage = new Dictionary<string, string>
            {
                { "role", "user" },
                { "content", newMessageText }
            };
            pastMessages.Add(newMessage);

            var responseMessageText = CallOpenAI(pastMessages, api_key, api_url, model);
            var responseMessage = new Dictionary<string, string>
            {
                { "role", "assistant" },
                { "content", responseMessageText }
            };
            pastMessages.Add(responseMessage);

            return (responseMessageText, pastMessages);
        }

        private static string CallOpenAI(List<Dictionary<string, string>> messages, string api_key, string api_url, string selectedModel)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {api_key}");
                var requestData = new
                {
                    model = selectedModel,
                    messages = messages
                };
                var response = httpClient.PostAsync(api_url,
                        new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"))
                    .Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;
                var jsonResponse = JObject.Parse(responseBody);
                Debug.Log(jsonResponse.ToString());

                var rusult = "";
                
                try
                {
                    return jsonResponse["choices"][0]["message"]["content"].ToString();
                    
                }catch
                {
                    Debug.Log("GPT renponce error");
                    return jsonResponse["error"]["message"].ToString();
                }
                
                
                if (jsonResponse.TryGetValue("choice", out var choice)
                    && choice.HasValues
                    && ((JObject)choice[0]).TryGetValue("message", out var mess)
                    && ((JObject)mess).TryGetValue("content", out var content)
                    )
                {
                    return content.ToString();
                }
                return jsonResponse["error"]["message"].ToString();
            }
        }
    }
}