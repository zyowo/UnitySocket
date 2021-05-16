using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Zyowo
{
    public class ServerPlayer : MonoBehaviour
    {
        AsyncServer server;
        public string serverIP = "127.0.0.1";
        public int serverPort = 28483;

        public InputField messageInput;
        public Button messageSendButton;

        // Start is called before the first frame update
        void Start()
        {
            server = new AsyncServer(serverIP, serverPort);
            _ = Task.Run(server.ListenAsync);

            messageInput = transform.Find("InputField").GetComponent<InputField>();
            messageSendButton = transform.Find("Button").GetComponent<Button>();
            messageSendButton.onClick.AddListener(BrocastMessage);
        }

        public void BrocastMessage()
        {
            string text = messageInput.text;
            byte[] textByte = Encoding.UTF8.GetBytes(text);
            server.BroadcastToClients(textByte);
        }
    }
}