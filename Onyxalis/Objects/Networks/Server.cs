using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Onyxalis.Objects.Networks
{
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<Client> clients = new List<Client>();
        private const int Port = 3000;

        public Server()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, Port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            string serverIp = "127.0.0.1"; // Replace with actual server IP
            int serverPort = 12345; // Replace with actual server port

            while (true)
            {
                TcpClient clientSocket = this.tcpListener.AcceptTcpClient();
                Client clientObject = new Client(clientSocket, serverIp, serverPort);
                clients.Add(clientObject);

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(clientObject);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            // Handle client communication, e.g., receiving viewport requests
            // and sending tile data back

            tcpClient.Close();
        }
        public static void Main()
        {
            Server server = new Server();
            Console.WriteLine("Server started...");
        }
    }
}
