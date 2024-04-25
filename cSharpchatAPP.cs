using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleChatApp
{
    class Program
    {
        static Socket listener;
        static Socket client;
        static IPEndPoint localEndPoint;
        static IPEndPoint remoteEndPoint;
        static bool listening = true;
        static bool connected = false;
        static Thread receiveThread;
        static ManualResetEvent connectEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER to start the chat application...");
            Console.ReadLine();

            // Set up the local port for listening
            int localPort = 12345;
            localEndPoint = new IPEndPoint(IPAddress.Any, localPort);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start the thread for receiving messages
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (listening)
            {
                Console.WriteLine("\nWaiting for a connection...");

                // Set up the client socket.
                client = listener.Accept();
                remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
                connected = true;
                connectEvent.Set();
            }

            // Clean up resources
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            listener.Close();
        }

        private static void ReceiveMessages()
        {
            byte[] bytesFrom = new byte[256];
            string dataFrom;

            while (connected)
            {
                int receivedBytes = client.Receive(bytesFrom);

                dataFrom = Encoding.ASCII.GetString(bytesFrom, 0, receivedBytes);
                Console.WriteLine("Received: {0}", dataFrom);

                // Send the received message back to the sender
                SendMessage(dataFrom);
            }
        }

        private static void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            // Send the message to the connected client.
            client.Send(messageBytes);
        }
    }
}