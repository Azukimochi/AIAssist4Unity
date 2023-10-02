using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.azukimochi 
{
    /// <summary>
    /// エラーログをWindowに流すためのクラス
    /// ChatGPTに書かせたものを一旦写しただけなので全部書き換え予定
    /// </summary>
    public class ErrorLogger : MonoBehaviour
    {
        private AIAssistHelperWindow chatGPT; // ChatGPTのスクリプトへの参照

        private void OnEnable()
        {
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
                // エラーメッセージをChatGPTに送信
                SendErrorMessageToChatGPT(logString + "\n" + stackTrace);
            }
        }

        void SendErrorMessageToChatGPT(string errorMessage)
        {
            if (chatGPT == null)
            {
                chatGPT = GetComponent<AIAssistHelperWindow>();
            }

            // ChatGPTにエラーメッセージを送信
            //chatGPT.AskChatGPT(errorMessage, (response) =>
            //{
            //    // 応答を処理するコードをここに書く
            //    Debug.Log("ChatGPT Response: " + response);
            //});
        }
    }

}