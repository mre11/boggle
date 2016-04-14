﻿// Braden Klunker, Morgan Empey, CS3500

using CustomNetworking;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

// TODO add comments everywhere

namespace Boggle
{
    public class BoggleWebServer
    {
        /// <summary>
        /// Starts the BoggleWebServer
        /// </summary>
        public static void Main()
        {
            new BoggleWebServer();
            Console.Read();
        }

        // Listens for http requests to the server.
        private TcpListener server;

        public BoggleService Service;

        public static HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Contructs a new BoggleWebServer initialized to port 60000 and
        /// begins listening for socket requests.
        /// </summary>
        public BoggleWebServer()
        {
            server = new TcpListener(IPAddress.Any, 60000);
            Service = new BoggleService();
            server.Start();
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        /// <summary>
        /// Callback method for when a connection to the server is requested.
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectionRequested(IAsyncResult ar)
        {
            // Momentarily stops the server from listening for socket connections and creates 
            // the current socket.
            Socket s = server.EndAcceptSocket(ar);

            // Sends a new HttpRequest by encoding the current socket into a string socket, and sending
            // along the server.
            new HttpRequest(new StringSocket(s, new UTF8Encoding()), this);

            // Server then begins looking for another connection request to the server.
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        ///// <summary>
        ///// Stops the BoggleWebServer
        ///// </summary>
        //public void Stop()
        //{
        //    server.Stop();
        //}
    }

    /// <summary>
    /// Processes HttpRequests appropriately with the BoggleService.
    /// </summary>
    class HttpRequest
    {
        private StringSocket ss;
        BoggleWebServer server;
        private int lineCount;
        private int contentLength;
        private string method;
        private string url;

        /// <summary>
        /// Constructs a new HttpRequest 
        /// </summary>
        /// <param name="stringSocket"></param>
        /// <param name="server"></param>
        public HttpRequest(StringSocket stringSocket, BoggleWebServer server)
        {
            this.server = server;
            contentLength = 0;
            lineCount = 0;
            ss = stringSocket;
            ss.BeginReceive(LineReceived, null);

        }

        /// <summary>
        /// Callback method for when a line of information is received.
        /// </summary>
        private void LineReceived(string s, Exception e, object payload)
        {
            lineCount++;

            if (s != null)
            {
                if (lineCount == 1)
                {
                    Regex r = new Regex(@"^(\S+)\s+(\S+)");
                    Match m = r.Match(s);
                    method = m.Groups[1].Value;
                    url = m.Groups[2].Value;
                }

                // Do GET requests here since they have no content
                if (method == "GET")
                {
                    if (url == "/BoggleService.svc/api") // API
                    {
                        // SendAPI();
                    }
                    else if (url.Contains("/BoggleService.svc/games/")) // GameStatus
                    {
                        // Find the gameID number.
                        Regex r = new Regex("([1-9]+[0-9]*)");
                        Match m = r.Match(url);

                        SendGameStatus(m.Groups[0].Value, url.ToLower().Contains("brief=yes"));
                    }
                }

                // Content-Length line tells how many chars long the content is
                if (s.StartsWith("Content-Length:"))
                {
                    contentLength = int.Parse(s.Substring(16).Trim());
                }
                if (s == "\r")
                {
                    ss.BeginReceive(ContentReceived, null, contentLength);
                }
                else
                {
                    ss.BeginReceive(LineReceived, null);
                }
            }
        }

        /// <summary>
        /// Callback method for when content is received. Uses the boggle service to properly
        /// handle the data in the content body of the http request.
        /// </summary>
        private void ContentReceived(string contentBody, Exception e, object payload)
        {

            if (contentBody != null)
            {
                string result = "";

                if (method == "POST")
                {
                    if (url == "/BoggleService.svc/users") // CreateUser
                    {
                        result = GetSerializedContent("users", contentBody, null);
                    }
                    else if (url == "/BoggleService.svc/games") // JoinGame
                    {
                        result = GetSerializedContent("games", contentBody, null);
                    }
                }
                else if (method == "PUT")
                {
                    if (url.Contains("/BoggleService.svc/games/")) // PlayWord
                    {
                        Regex r = new Regex("([1-9]+[0-9]*)");
                        Match m = r.Match(url);

                        result = GetSerializedContent("", contentBody, m.Groups[0].Value);
                    }
                    else if (url == "/BoggleService.svc/games") // CancelJoin
                    {
                        User user = JsonConvert.DeserializeObject<User>(contentBody);
                        server.Service.CancelJoin(user);
                    }

                }

                ss.BeginSend("HTTP/1.1 " + (int)BoggleWebServer.StatusCode + " " + BoggleWebServer.StatusCode.ToString() + "\r\n", Ignore, null);
                ss.BeginSend("Content-Type: application/json\r\n", Ignore, null);
                ss.BeginSend("Content-Length: " + result.Length + "\r\n", Ignore, null);
                ss.BeginSend("\r\n", Ignore, null);
                ss.BeginSend(result, (ex, py) => { ss.Shutdown(); }, null);
            }
        }

        /// <summary>
        /// Derserializes the content and then processes the content through the boggleservice and
        /// returns the json serialized string.
        /// </summary>
        private string GetSerializedContent(string type, string content, string gameID)
        {
            object response = null;

            // Deserialize the content then send to the boggle service to be processed.
            if (type == "users")
            {
                User requestedUser = JsonConvert.DeserializeObject<User>(content);
                response = server.Service.CreateUser(requestedUser);
            }
            else if (type == "games")
            {
                JoinGameRequest requestBody = JsonConvert.DeserializeObject<JoinGameRequest>(content);
                response = server.Service.JoinGame(requestBody);
            }
            else
            {
                BoggleWord word = JsonConvert.DeserializeObject<BoggleWord>(content);
                response = server.Service.PlayWord(gameID, word);
            }

            // Return the Json serialized object information.
            return JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        //private void SendAPI()
        //{
        //    var stream = server.Service.API();

        //    ss.BeginSend("HTTP/1.1 " + (int)BoggleWebServer.StatusCode + " " + BoggleWebServer.StatusCode.ToString() + "\r\n", Ignore, null);
        //    ss.BeginSend("Content-Type: text/html\r\n", Ignore, null);
        //    ss.BeginSend("Content-Length: " + stream.ToString().Length + "\r\n", Ignore, null);
        //    ss.BeginSend("\r\n", Ignore, null);
        //    ss.BeginSend(stream.ToString(), (ex, py) => { ss.Shutdown(); }, null);
        //}

        /// <summary>
        /// Sends the GameStatus back to the client.
        /// </summary>
        private void SendGameStatus(string gameID, bool brief)
        {
            BoggleGameResponse response = server.Service.GameStatus(gameID, brief ? "yes" : "");
            string result = JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            ss.BeginSend("HTTP/1.1 " + (int)BoggleWebServer.StatusCode + " " + BoggleWebServer.StatusCode.ToString() + "\r\n", Ignore, null);
            ss.BeginSend("Content-Type: application/json\r\n", Ignore, null);
            ss.BeginSend("Content-Length: " + result.Length + "\r\n", Ignore, null);
            ss.BeginSend("\r\n", Ignore, null);
            ss.BeginSend(result, (ex, py) => { ss.Shutdown(); }, null);
        }

        private void Ignore(Exception e, object payload)
        {
        }
    }
}