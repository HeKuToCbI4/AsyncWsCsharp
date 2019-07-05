using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace AsyncWsClient
{
   class Program
   {
      static async Task Main(string[] args)
      {
         Console.WriteLine("Hello World!");
         var wsClient = new WebSocketClientAsync(new Uri("wss://localhost:4897"));
         var connTask = wsClient.Connect();
         connTask.Wait();
         while (true)
         {
            var str = wsClient.RecvData();
            if (str != null)
            {
               Console.WriteLine(str);
            }
         }
      }
   }
}