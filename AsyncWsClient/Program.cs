using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using google.protobuf;
using WebAPI_1;

namespace AsyncWsClient
{
   class Program
   {
      static void Main(string[] args)
      {
         var wsClient = new WebSocketClientAsync(new Uri("wss://127.0.0.1:4897"));
         var connTask = wsClient.Connect();
         connTask.Wait();
         Console.WriteLine("Connected!");
         Console.WriteLine("Logging on!");
         var LogonTask = wsClient.Logon();
         LogonTask.Wait();
         Console.WriteLine("Logon sent!");
         while (true)
         {
            if (wsClient.GetConnectionStatus())
            {
               var pm = wsClient.RecvData();
               if (pm != null)
               {
                  Console.WriteLine(pm.account_logoff_result);
                  Console.WriteLine(pm);
               }
            }
            else
            {
               Console.WriteLine("Failure in connection!");
               Console.WriteLine(wsClient.GetCloseDescription());
               return;
            }
         }
      }
   }
}