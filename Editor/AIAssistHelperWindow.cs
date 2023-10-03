using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace io.github.azukimochi
{

    public class AIAssistHelperWindow : EditorWindow
    {
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private static string OPENAI_API_KEY = "";

        private string errorLog = "";
        private string solution = "";
        private string settings = "Unityで開発をしていたところ、次のエラーログが出力されました。次に送信するエラーログを解析して問題を特定してください。";
        private Tab _tab = Tab.Main;
        private Vector2 _scrollPosition_Settings = Vector2.zero;
        private Vector2 _scrollPosition_ErrorLog = Vector2.zero;
        private Vector2 _scrollPosition_Solution = Vector2.zero;
        private bool _SolutionButton = false;
        private bool _UpdateSolution = false;
        private string api_key;

        enum Tab
        {
            Main,
            Settings,
        }

        private string[] model = new[] {"gpt-3.5-turbo", "gpt-4" };
        private int modelIndex = 0;
        
        [MenuItem("Window/AI Assist Helper")]
        public static void ShowWindow()
        {
            GetWindow<AIAssistHelperWindow>("AI Assist Helper");
        }

        private void Update()
        {
            if(_UpdateSolution)
            {
                _UpdateSolution = false;
                Repaint();
            }
        }

        private void OnEnable (){
            Application.logMessageReceived += HandleLog;
            api_key = Utils.AesDecrypt(EditorPrefs.GetString("OPENAI_API_KEY", ""));
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
            GUILayout.FlexibleSpace();
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

            EditorGUI.BeginDisabledGroup(_SolutionButton);
            if (GUILayout.Button("Get Solution"))
            {
                _SolutionButton = true;
                solution = "処理中・・・";
                
                Task.Run(async () =>
                {
                    var result = await CallOpenAI();
                    solution = result;
                    _SolutionButton = false;
                    _UpdateSolution = true;
                });
            }
            GUILayout.Label("Solution", EditorStyles.boldLabel);
            _scrollPosition_Solution = EditorGUILayout.BeginScrollView(_scrollPosition_Solution, GUILayout.Height(300));
            {
                EditorGUILayout.TextArea(solution, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();
        }

        public async Task<string> CallOpenAI()
        {
            string result  = ChatGPTHandler.Completion(errorLog, settings, OPENAI_API_KEY, OPENAI_API_URL,
                    model[modelIndex])
                .Item1;
            return result;
        }
        public void DrawSettingsTab()
        {
            GUILayout.Label("OpenAI API Key" , EditorStyles.boldLabel);
            OPENAI_API_KEY = EditorGUILayout.PasswordField(OPENAI_API_KEY);
            OPENAI_API_KEY = api_key;
            
            GUILayout.Space(20);

            var popupModels = new[]
            {
                new GUIContent(model[0]),
                new GUIContent(model[1]),
            };
            
            modelIndex = EditorGUILayout.Popup(label:new GUIContent("Select Model"), selectedIndex:modelIndex, displayedOptions:popupModels);
            
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            _scrollPosition_Settings = EditorGUILayout.BeginScrollView(_scrollPosition_Settings, GUILayout.Height(100));
            {
                settings = EditorGUILayout.TextArea(settings, GUILayout.ExpandHeight(true));
            };
            EditorGUILayout.EndScrollView();
            GUILayout.Space(20);
            if (GUILayout.Button("Apply"))
            {
                Debug.Log("Encrypt" + Utils.AesEncrypt(OPENAI_API_KEY));
                EditorPrefs.SetString("OPENAI_API_KEY", Utils.AesEncrypt(OPENAI_API_KEY));
                Debug.Log("Raw key" + EditorPrefs.GetString("OPENAI_API_KEY", ""));
                Debug.Log("Decrypt" + Utils.AesDecrypt(EditorPrefs.GetString("OPENAI_API_KEY", "")));
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
            public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
        }
    }
}
