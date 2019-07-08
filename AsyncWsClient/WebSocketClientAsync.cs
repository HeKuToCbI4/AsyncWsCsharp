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
using WebAPI_1;
using ProtoBuf;

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
         return Task.Run(()=>_clientWebSocket.ConnectAsync(_URL, CancellationToken.None));
      }

      public const int NotFound = -1;

      public static int GetPositionOfLastByteWithData(byte[] array)
      {
         for (int i = array.Length - 1; i > -1; i--)
         {
            if (array[i] > 0)
            {
               return i;
            }
         }

         return NotFound;
      }

      public String GetCloseDescription()
      {
         return _clientWebSocket.CloseStatusDescription;
      }
      
      public ServerMsg RecvData()
      {
         var buf = new ArraySegment<byte>(new byte[4096]);
         _clientWebSocket.ReceiveAsync(buf, CancellationToken.None).Wait();
         var lastByte = GetPositionOfLastByteWithData(buf.Array);
         if (lastByte != -1)
         {
            var slicedBuf = buf.Array.Take(lastByte+1).ToArray();
            return ProtoDeserialize<ServerMsg>(slicedBuf);
         }

         return null;
      }

      private static byte[] ProtoSerialize<T>
         (T record) where T : class
      {
         if (null == record) return null;
         try
         {
            using (var stream = new MemoryStream())
            {
               Serializer.Serialize(stream, record);
               return stream.ToArray();
            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public Boolean GetConnectionStatus()
      {
         return _clientWebSocket.State == WebSocketState.Open;
      }

      public static T ProtoDeserialize<T>(byte[] data) where T : class
      {
         if (null == data) return null;

         try
         {
            Console.WriteLine(data.Length);
            using (var stream = new MemoryStream(data, 0, data.Length))
            {
               return Serializer.Deserialize<T>(stream);
            }
         }
         catch
         {
            // Log error
            throw;
         }
      }

      public Task Logon()
      {
         var LogonMessage = new ClientMsg();
         LogonMessage.logon = new Logon();
         LogonMessage.logon.user_name = "WebTrader";
         LogonMessage.logon.password = "WebTrader";
         return Task.Run(()=>_clientWebSocket.SendAsync(new ArraySegment<byte>(ProtoSerialize(LogonMessage)),
            WebSocketMessageType.Binary, true, CancellationToken.None));
      }

      public Task Disconnect()
      {
         return Task.Run(()=>_clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Success disconnect...",
            CancellationToken.None));
      }
   }
}