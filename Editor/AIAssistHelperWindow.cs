using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VRC.SDKBase;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace io.github.azukimochi
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections;

    public class AIAssistHelperWindow : EditorWindow
    {
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private static string OPENAI_API_KEY = "";

        string errorLog = "";
        string solution = "";
        private Tab _tab = Tab.Main;
        private Vector2 _scrollPosition_ErrorLog = Vector2.zero;
        private Vector2 _scrollPosition_Solution = Vector2.zero;

        enum Tab
        {
            Main,
            Settings,
        }
        [MenuItem("Window/AI Assist Helper")]
        public static void ShowWindow()
        {
            GetWindow<AIAssistHelperWindow>("AI Assist Helper");
        }
        private void OnEnable (){
            Application.logMessageReceived += HandleLog;
        }
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                errorLog += logString + "\n" + stackTrace + "\n";
            }
        }

        void OnGUI()
        {
            _tab = (Tab)GUILayout.Toolbar((int)_tab, Styles.TabToggles, Styles.TabButtonStyle, Styles.TabButtonSize);
            
            GUILayout.FlexibleSpace();
            switch (_tab)
            {
                case Tab.Main:
                    DrawMainTab();
                    break;
                case Tab.Settings:
                    DrawSettingsTab();
                    break;
            }
        }
        public void DrawMainTab()
        {
            var style = new GUIStyle(EditorStyles.textArea) {wordWrap = true};
            
            GUILayout.Label("Error Log", EditorStyles.boldLabel);

            _scrollPosition_ErrorLog = EditorGUILayout.BeginScrollView(_scrollPosition_ErrorLog, GUILayout.Height(100));
            {
                errorLog = EditorGUILayout.TextArea(errorLog, GUILayout.ExpandHeight(true));
            };
            EditorGUILayout.EndScrollView(); 
            
            GUI.SetNextControlName("ErrorLogField");
            if(GUILayout.Button("Clear Error Log")) {
                errorLog = "";
                GUI.FocusControl("ErrorLogField");
            }

            if (GUILayout.Button("Get Solution"))
            {
                // OpenAI APIを呼び出して解決策を取得する関数
                //solution = GetSolutionFromChatGPT(errorLog);
                
                string settings = "Unityで開発をしていたところ、次のエラーログが出力されました。次に送信するエラーログを解析して問題を特定してください。";
                solution = Completion(errorLog, settings).Item1;
            }

            
            GUILayout.Label("Solution", EditorStyles.boldLabel);
            _scrollPosition_Solution = EditorGUILayout.BeginScrollView(_scrollPosition_Solution, GUILayout.Height(300));
            {
                solution = EditorGUILayout.TextArea(solution, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();
        }
        public void DrawSettingsTab()
        {
            GUILayout.Label("OpenAI API Key" , EditorStyles.boldLabel);
            OPENAI_API_KEY = EditorGUILayout.TextField(OPENAI_API_KEY);
            GUILayout.Label("SelectModel" , EditorStyles.boldLabel);
        }

        public (string, List<Dictionary<string, string>>) Completion(string newMessageText, string settingsText = "",
            List<Dictionary<string, string>> pastMessages = null)
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

            var responseMessageText = CallOpenAI(pastMessages);
            var responseMessage = new Dictionary<string, string>
            {
                { "role", "assistant" },
                { "content", responseMessageText }
            };
            pastMessages.Add(responseMessage);

            return (responseMessageText, pastMessages);
        }

        private string CallOpenAI(List<Dictionary<string, string>> messages)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_KEY}");
                var requestData = new
                {
                    model = "gpt-3.5-turbo",
                    messages = messages
                };
                var response = httpClient.PostAsync(OPENAI_API_URL,
                        new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"))
                    .Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;
                var jsonResponse = JObject.Parse(responseBody);
                Debug.Log(jsonResponse.ToString());
                return jsonResponse["choices"][0]["message"]["content"].ToString();
            }
        }
        private static class Styles
        {
            private static GUIContent[] _tabToggles = null;
            public static GUIContent[] TabToggles{
                get {
                    if (_tabToggles == null) {
                        _tabToggles = System.Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray();
                    }
                    return _tabToggles;
                }
            }
            public static readonly GUIStyle TabButtonStyle = "LargeButton";

            // GUI.ToolbarButtonSize.FitToContentsも設定できる
            public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
        }
    }
}
