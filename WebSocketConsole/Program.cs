using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketConsole
{
    class Program
    {
        static  void Main(string[] args)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() < 100)
            {
                Process.Start("WebSocketConsole.exe");
            }
            StartWebSocket().GetAwaiter().GetResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task StartWebSocket()
        {
            var client = new ClientWebSocket();
            Random random = new Random();
            string user = random.Next(1000000000).ToString();
            await client.ConnectAsync(new Uri($"ws://192.168.2.194:5000/websocket/chat?userId={user}&classRoomId=111&name=测试{user}"), CancellationToken.None);
            Console.WriteLine($"{user}web socket connection established @{DateTime.Now.Ticks}");
            var send = Task.Run(async () =>
            {
                string message;
                while ((message = Console.ReadLine()) != null && message != string.Empty)
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                
                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "client close", CancellationToken.None);
            });
            var receive = ReceiveAsync(client);
            await  Task.WhenAll(send, receive);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task ReceiveAsync(ClientWebSocket client)
        {
            var buffer = new byte[1024 * 4];
            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, result.Count));
                if (result.MessageType == WebSocketMessageType.Close || client.State == WebSocketState.CloseReceived)
                {
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "client close", CancellationToken.None);
                    break;
                }
 
            }
        }
    }
}
