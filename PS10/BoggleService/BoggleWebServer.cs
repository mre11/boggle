// Braden Klunker, Morgan Empey, CS3500

using CustomNetworking;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

// TODO we could probably refactor this a lot to reduce code duplication
// TODO add comments everywhere (after refactor?)
// TODO when I tried Joe's tests, we failed some due to weird exceptions.  we'll need to investigate.

namespace Boggle
{
    public class BoggleWebServer
    {

        public static void Main()
        {
            new BoggleWebServer();
            Console.Read();
        }

        private TcpListener server;

        public BoggleService Service;

        public static HttpStatusCode StatusCode { get; set; }

        public BoggleWebServer()
        {
            server = new TcpListener(IPAddress.Any, 60000);
            Service = new BoggleService();
            server.Start();
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        private void ConnectionRequested(IAsyncResult ar)
        {
            Socket s = server.EndAcceptSocket(ar);
            new HttpRequest(new StringSocket(s, new UTF8Encoding()), this);
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        public void Stop()
        {
            server.Stop();
        }
    }

    class HttpRequest
    {
        private StringSocket ss;
        BoggleWebServer server;
        private int lineCount;
        private int contentLength;
        private string method;
        private string url;
        //private Regex finder = new Regex(@"(.games|.users)$|(.games.)([1-9]+[0-9]*)$|(.games.)([1-9]+[0-9]*)(.Brief.)(yes)$");

        public HttpRequest(StringSocket stringSocket, BoggleWebServer server)
        {
            this.server = server;
            contentLength = 0;
            lineCount = 0;
            ss = stringSocket;
            ss.BeginReceive(LineReceived, null);

        }

        private void LineReceived(string s, Exception e, object payload)
        {
            lineCount++;
            Console.WriteLine(s);
            if (s != null)
            {
                if (lineCount == 1)
                {
                    Regex r = new Regex(@"^(\S+)\s+(\S+)");
                    Match m = r.Match(s);
                    method = m.Groups[1].Value;
                    url = m.Groups[2].Value;
                    //[] matches = finder.GetGroupNames();
                }

                // Do GET requests here since they have no content
                if (method == "GET")
                {
                    if (url == "/BoggleService.svc/api") // API
                    {
                        // SendAPI(); // TODO api not working because index.html isn't in the unit test project, decide whether to fix or just not support it
                    }
                    else if (url.Contains("/BoggleService.svc/games/")) // GameStatus
                    {
                        // Find the gameID number.
                        Regex r = new Regex("([1-9]+[0-9]*)");
                        Match m = r.Match(url);

                        if (!url.Contains("Brief"))
                        {
                            SendGameStatus(m.Groups[0].Value, null);
                        }
                        else
                        {
                            SendGameStatus(m.Groups[0].Value, "yes");
                        }
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

                        result = GetSerializedContent(null, contentBody, m.Groups[0].Value);
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

            return JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        }

        private void SendAPI()
        {
            var stream = server.Service.API();

            ss.BeginSend("HTTP/1.1 " + (int)BoggleWebServer.StatusCode + " " + BoggleWebServer.StatusCode.ToString() + "\r\n", Ignore, null);
            ss.BeginSend("Content-Type: text/html\r\n", Ignore, null);
            ss.BeginSend("Content-Length: " + stream.ToString().Length + "\r\n", (ex, py) => { ss.Shutdown(); }, null);
            // TODO send stream?
        }

        private void SendGameStatus(string gameID, string brief)
        {
            BoggleGameResponse response = server.Service.GameStatus(gameID, brief);
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