using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Text;

namespace Zyowo
{
    public class AsyncServer : IDisposable
    {
        public TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        volatile bool acceptLoop = true;

        public AsyncServer(string serverIP, int serverPort)
        {
            IPAddress ip = IPAddress.Parse(serverIP);
            listener = new TcpListener(ip, serverPort);
        }

        /// <summary>
        /// 异步任务：服务端监听客户端连接
        /// </summary>
        /// <returns></returns>
        public async Task ListenAsync()
        {
            listener.Start();
            Debug.Log("[播放器] 开始监听！");
            while (acceptLoop)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    _ = Task.Run(() => OnConnectClientAsync(client));
                    await UniTask.Yield(); //通过该语句，程序将返回主线程上下文，其他地方一个意思
                    // TODO：发送连接消息
                }
                catch (ObjectDisposedException e)// thrown if the listener socket is closed
                {
                    Debug.Log($"{nameof(AsyncServer)}: Server was Closed! {e}");
                }
                catch (SocketException e)// Some socket error
                {
                    Debug.Log($"{nameof(AsyncServer)}: Some socket error occurred! {e}");
                }
                finally
                {
                    await UniTask.Yield();
                    // TODO：发送服务器关闭消息
                }
            }
        }

        /// <summary>
        /// 异步任务：添加客户端，异步获取数据
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        async Task OnConnectClientAsync(TcpClient client)
        {
            var clientEndpoint = client.Client.RemoteEndPoint;
            Debug.Log($"完成握手 {clientEndpoint}");
            clients.Add(client);
            try
            {
                await HandleNetworkStreamAsync(client);
            }
            catch (Exception e) //连接断开时，stream 会抛出dispose相关异常,捕获避免向上传递中断了监听。
            {
                Debug.Log($"{nameof(AsyncServer)}: 客户端意外断开连接 {e}");
            }
            finally
            {
                Debug.Log($"连接断开 {clientEndpoint}");
                clients.Remove(client);
            }
        }

        /// <summary>
        /// 获取后 NetworkStream ，调用 Write 方法将数据发送到远程主机。 
        /// 调用 Read 方法以接收从远程主机传入的数据。 这两种方法都将一直阻塞，直到执行指定的操作。 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        async Task HandleNetworkStreamAsync(TcpClient client)
        {
            
            byte[] buffer = new byte[client.ReceiveBufferSize];

            using (NetworkStream ns = client.GetStream())
            {
                while (client.IsOnline())
                {
                    var data = await ns.ReadAsync(buffer, 0, 10);

                    Debug.Log($"[控制器] 接收到播放器消息!");
                }
            }
        }

        /// <summary>
        /// 服务端给多个客户端发消息
        /// </summary>
        /// <param name="data"></param>
        public void BroadcastToClients(byte[] data)
        {
            Debug.Log($"Clients.Count : {clients.Count}");
            foreach (var c in clients)
            {
                SendMessageToClient(c, data);
            }
        }

        /// <summary>
        /// 服务端给指定的客户端发消息
        /// </summary>
        /// <param name="c"></param>
        /// <param name="data"></param>
        public void SendMessageToClient(TcpClient c, byte[] data)
        {
            if (null != c)
            {
                try
                {
                    // TODO：这里需要做分包
                    c.GetStream().Write(data, 0, data.Length);
                    c.GetStream().Flush();
                }
                catch (Exception e)
                {
                    Debug.Log($"{nameof(AsyncServer)}: Send Message To Client Failed - {e}");
                }
            }
        }


        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            //先通知在线的客户端
            lock (clients)
            {
                foreach (var c in clients)
                {
                    c?.Close();
                }
                clients.Clear();
            }
            //然后关断自身
            lock (this)
            {
                if (listener == null)
                    throw new InvalidOperationException("Not started");
                acceptLoop = false;
                listener.Stop();
                listener = null;
            }
        }

    }
}