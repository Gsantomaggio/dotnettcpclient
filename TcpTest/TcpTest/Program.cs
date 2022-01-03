// See https://aka.ms/new-console-template for more information

using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpTest
{
    public class Connection
    {
        private Socket socket;
        private PipeWriter writer;
        private PipeReader reader;
        private Task readerTask;

        static void Main(string[] args)
        {
            
            Console.WriteLine("Connecting...");
            new Connection().Connect();
            Console.WriteLine("Connected...");
            Console.ReadLine();
            
            
        }

        private void Connect()
        {
            var ep = new IPEndPoint(IPAddress.Loopback, 5555);
        
            var sck = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var taskConnection = sck.ConnectAsync(ep);
            taskConnection.Wait(1000);

            this.socket = sck;
            var stream = new NetworkStream(sck);
            writer = PipeWriter.Create(stream);
            reader = PipeReader.Create(stream);
            readerTask = Task.Run(ProcessIncomingFrames);
        }

        private async Task ProcessIncomingFrames()
        {
            while (true)
            {

                TcpClient c;
                if (!socket.Connected)
                {
                    Console.WriteLine("Nope");
                }
                
                if (!reader.TryRead(out ReadResult result))
                {
                    result = await reader.ReadAsync().ConfigureAwait(false);
                }

                var buffer = result.Buffer;
                if (buffer.Length == 0)
                {
                    // We're not going to receive any more bytes from the connection.
                    break;
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }
    }
}