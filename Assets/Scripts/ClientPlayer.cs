using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zyowo
{
    public class ClientPlayer : MonoBehaviour
    {
        public string serverIP = "127.0.0.1";
        AsyncClient client;
        public Text title;
        public Text messageText;

        // Start is called before the first frame update
        void Start()
        {
            title = transform.Find("Text").GetComponent<Text>();
            client = new AsyncClient(this, serverIP);
            title.text = string.Empty;

            messageText = transform.Find("MessageBox/Viewport/Content/Text").GetComponent<Text>();
            messageText.text = string.Empty;
        }

        // 测试
        public void OnClientConnected()
        {
            title.text = client.GetClientIP();
        }

        /// <summary>
        /// 将日志显示在场景的日志面板中
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="content">内容</param>
        public void LogText(string prefix, string content)
        {
            if (!messageText) return;
            if (messageText.text == "")
                messageText.text = content;
            else
                messageText.text = messageText.text + '\n' + content;
        }
    }
}