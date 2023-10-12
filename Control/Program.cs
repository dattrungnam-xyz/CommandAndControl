﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;

namespace Control
{
     class Program
    {
        //static IPAddress address = IPAddress.Parse("192.168.1.102");
        //static TcpListener listener = new TcpListener(address, 6969);
        private const int BUFFER_SIZE = 1024;
        private const int PORT_NUMBER = 9999;
        static ASCIIEncoding encoding = new ASCIIEncoding();
        static void Main(string[] args)
        {
            try
            {
                IPAddress address = IPAddress.Parse("192.168.1.102");

                TcpListener listener = new TcpListener(address, PORT_NUMBER);

                // 1. listen
                listener.Start();

                Console.WriteLine("Server started on " + listener.LocalEndpoint);
                Console.WriteLine("Waiting for a connection...");

                Socket socket = listener.AcceptSocket();
                Console.WriteLine("Connection received from " + socket.RemoteEndPoint);

                // 2. receive
                byte[] data = new byte[BUFFER_SIZE];
                socket.Receive(data);

                string str = encoding.GetString(data);

                // 3. send
                socket.Send(encoding.GetBytes("Hello " + str));

                // 4. close
                //socket.Close();
                //listener.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
    }
}