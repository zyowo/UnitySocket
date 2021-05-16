using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

namespace Zyowo
{
    public class AsyncClient
    {
        private TcpClient tcpClient;
        bool isRun = false;
        private string serverIP;
        private ClientPlayer player;
        private readonly string TAG = "[Client]";

        public AsyncClient(ClientPlayer player, string serverIP)
        {
            this.player = player;
            this.serverIP = serverIP;
            tcpClient = new TcpClient();
            //tcpClient.NoDelay = true;         // 可以测试粘包
            OnConnectOrDisConnectRequired();
        }
        public string GetClientIP()
        {
            var s = tcpClient.Client;
            // Using the LocalEndPoint property.
            return string.Format("[客户端] " + IPAddress.Parse(((IPEndPoint)s.LocalEndPoint).Address.ToString()) +
                ":" + ((IPEndPoint)s.LocalEndPoint).Port.ToString());
        }

        // 点击按钮，开始连接
        private async void OnConnectOrDisConnectRequired()
        {
            if (!isRun)
            {
                // 连接中
                var isConnectedSuccess = await ConnectAsTcpClientAsync();
                // 是否连接成功
                if (isConnectedSuccess)
                {
                    await UniTask.Yield();
                    player.OnClientConnected();
                }
            }
            else
            {
                tcpClient.Close();
                tcpClient = null;
                isRun = false;
                // 连接失败
            }
        }

        private async Task<bool> ConnectAsTcpClientAsync()
        {
            isRun = true;

            try
            {
                await tcpClient.ConnectAsync(serverIP, 28483);
                _ = Task.Run(StreamReadHandleAsync);
                await UniTask.Yield();      // 在不确定 Task 是否再主线程中执行，务必 await UniTask.Yield() 返回主线程后才能调用Unity组件
                player.LogText(TAG, "连接到 服务端 ---> " + serverIP);
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(AsyncClient)}: [客户端] 连接到服务端失败 {e}");
                await UniTask.Yield();
                Close();
                //小提示：此处分发握手失败事件，值得注意的是必须先返回主线程，本例使用 await UniTask.Yield() 返回主线程
            }
            return isRun;
        }

        async Task StreamReadHandleAsync()
        {
            Debug.Log("开启数据读逻辑");
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

            try
            {
                using (var ns = tcpClient.GetStream())
                {
                    while (isRun && tcpClient.IsOnline())
                    {
                        await ns.ReadAsync(buffer, 0, (int)tcpClient.ReceiveBufferSize);
                        string request = Encoding.UTF8.GetString(buffer);
                        Debug.Log($"[客户端] 接收到服务器消息 {request}!");
                        await UniTask.Yield();
                        try
                        {
                            // UI 显示
                            player.LogText(TAG, request);
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"{nameof(AsyncClient)}: {e}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await UniTask.Yield();
                Debug.LogError($"{nameof(AsyncClient)}: [客户端] 接收消息失败: {e}");
            }
            finally
            {
                Debug.LogError($"{nameof(AsyncClient)}: 与服务器断开连接！");
                await UniTask.Yield();
                Close();
                //小提示：此处分发与服务器断开连接事件，值得注意的是必须先返回主线程，本例使用 await UniTask.Yield() 返回主线程
            }
        }

        void Close()
        {
            tcpClient?.Close();
            tcpClient = null;
            isRun = false;
        }
    }

    /// <summary>
    /// TcpClient.Connected: 属性获取截止到最后一次 I/O 操作时的 Client 套接字的连接状态。
    /// C# TcpClient在连接成功后，对方关闭了网络连接是不能及时的检测到断开的，
    /// 故而使用此扩展检测连接状态
    /// </summary>
    public static class TcpClientEx
    {

        public static bool IsOnline(this TcpClient c)
        {
            return !((c.Client.Poll(1000, SelectMode.SelectRead) && (c.Client.Available == 0)) || !c.Client.Connected);
        }
    }
}