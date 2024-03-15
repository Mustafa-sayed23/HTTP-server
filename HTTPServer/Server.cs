using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;
        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            LoadRedirectionRules(redirectionMatrixPath);

            //TODO: initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint HostEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
            serverSocket.Bind(HostEndPoint);
            Console.WriteLine("connecting...");
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(100);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket ClientSocket = serverSocket.Accept();
                Console.WriteLine("New Client Accepted : {0}", ClientSocket.RemoteEndPoint);
                Thread NewThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                NewThread.Start(ClientSocket);
            }

        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket ClientSocket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            ClientSocket.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] Data = new byte[1024];
                    int ReceivedLen = ClientSocket.Receive(Data);

                    // TODO: break the while loop if receivedLen==0
                    if(ReceivedLen == 0)
                    {
                        Console.WriteLine("Client : {0} ended the connection ", ClientSocket.RemoteEndPoint);
                        break;
                    }

                    // TODO: Create a Request object using received request string
                    Request RequestOpject = new Request(Encoding.ASCII.GetString(Data));
                    // TODO: Call HandleRequest Method that returns the response
                    Response resp = HandleRequest(RequestOpject);
                    // TODO: Send Response back to client
                    ClientSocket.Send(Encoding.ASCII.GetBytes(resp.ResponseString));

                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                    Logger.LogException(ex);
                }

            }
            // TODO: close client socket
            ClientSocket.Close();
        }
         
        Response HandleRequest(Request request)
        {
            try
            {
                if (request.relativeURI == "/")
                {
                    request.relativeURI = "/main.html";
                }
                //TODO: check for bad request 
                bool flag1 = request.ParseRequest();
                if(!flag1)
                {
                    string p = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    Response RSP = new Response(StatusCode.BadRequest, "text/html", p, "");
                    return RSP;
                }

                //TODO: map the relativeURI in request to get the physical path of the resource
                String content = LoadDefaultPage(request.relativeURI);

                //TODO: check for redirect
                String flag2 = GetRedirectionPagePathIFExist(request.relativeURI);
                if (flag2 != "")
                {
                    string s = LoadDefaultPage('/'+flag2);
                    Response RSP = new Response(StatusCode.Redirect, "text/html", s, flag2);
                    return RSP;
                }

                //TODO: check file exists
                if (content == "")
                {
                    string g = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                    Response RSP = new Response(StatusCode.NotFound, "text/html", g, "");
                    return RSP;
                }

                //TODO: read the physical file

                // Create OK response
                Response RS = new Response(StatusCode.OK , "text/html", content, "");
                return RS;
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                // TODO: in case of exception,
                Logger.LogException(ex);
                string l = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                Response RSP = new Response(StatusCode.InternalServerError, "text/html", l, "");
                return RSP;
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            string[] ss = relativePath.Split('/');
            foreach (var keyValue in Configuration.RedirectionRules)
            {
                if (keyValue.Key == ss[1])
                {
                    return keyValue.Value;
                }
            }
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            string filePath = Configuration.RootPath + defaultPageName;
            bool flag = File.Exists(filePath);
            if (!flag)
            {
                Exception ex = new Exception(Configuration.NotFoundDefaultPageName);
                Logger.LogException(ex);
                return string.Empty;
            }
            // else read file and return its content
            else
            {
                string c = File.ReadAllText(filePath);
                return c;
            }
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                // TODO: using the filepath paramter read the redirection rules from file 
                string s = File.ReadAllText(filePath);
                string[] ss = s.Split(',');
                // then fill Configuration.RedirectionRules dictionary 
                Configuration.RedirectionRules.Add(ss[0], ss[1]);
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
            }
        }
    }
}
