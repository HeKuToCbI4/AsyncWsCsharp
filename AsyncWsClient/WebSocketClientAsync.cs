using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWsClient
{
   public class WebSocketClientAsync
   {
      private Uri _URL;
      private SslClientAuthenticationOptions _sslClientAuthenticationOptions;
      private ClientWebSocket _clientWebSocket;

      public WebSocketClientAsync(Uri url)
      {
         _clientWebSocket = new ClientWebSocket();
         _URL = url;
         _clientWebSocket.Options.RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
         {
            return true;
         };
      }

      public Task Connect()
      {
         Console.WriteLine("Connecting!");
         return _clientWebSocket.ConnectAsync(_URL, CancellationToken.None);
      }
      
      public const int NotFound = -1;
      public static int GetPositionOfLastByteWithData(byte[] array)
      {
         for (int i = array.Length - 1; i > -1; i--)
         {
            if (array[i] > 0) { return i; }
         }
         return NotFound;
      }  
      
      public String? RecvData()
      {
         var buf = new ArraySegment<byte>(new byte[4096]);
         Task.Run(() => _clientWebSocket.ReceiveAsync(buf, CancellationToken.None));
         var lastByte = GetPositionOfLastByteWithData(buf.Array);
         if (lastByte != -1)
         {
            return System.Text.Encoding.Unicode.GetString(buf.Slice(0,lastByte));
         }

         return null;
      }
      
      public Task Disconnect()
      {
         return _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Success disconnect...",
            CancellationToken.None);
      }
   }
}