using CustomNetworking;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleWebServer
    {
        public static BoggleService service = new BoggleService();

        public static void Main()
        {
            new BoggleWebServer();
            Console.Read();
        }

        private TcpListener server;

        public static HttpStatusCode StatusCode { get; set; }

        public BoggleWebServer()
        {
            server = new TcpListener(IPAddress.Any, 60000);
            server.Start();
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        private void ConnectionRequested(IAsyncResult ar)
        {
            Socket s = server.EndAcceptSocket(ar);
            new HttpRequest(new StringSocket(s, new UTF8Encoding()));
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
        private int lineCount;
        private int contentLength;
        private string method;
        private string url;
        private Regex finder = new Regex(@"(.games|.users)$|(.games.)([1-9]+[0-9]*)$|(.games.)([1-9]+[0-9]*)(.Brief.)(yes)$");

        public HttpRequest(StringSocket stringSocket)
        {
            /*service = new BoggleService();*/ // TODO TypeInitializationException gets thrown here when we call a test method...
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
                    string[] matches = finder.GetGroupNames();
                }

                // Do GET requests here since they have no content
                if (method == "GET")
                {
                    if (url == "/BoggleService.svc/api")
                    {
                        SendAPI();
                    }
                    else if (url.Contains("games")) // TODO maybe check a regex here for the gameID?
                    {
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
                    if (url.Contains("users"))
                    {
                        // CreateUser
                        result = GetSerializedContent("users", contentBody, null);
                    }
                    else if (url.Contains("games"))
                    {
                        // JoinGame
                        result = GetSerializedContent("games", contentBody, null);
                    }

                }
                else if (method == "PUT")
                {
                    if (url.Contains("/games/")) // maybe instead check if it matches a regex pattern?
                    {
                        //PlayWord
                        Regex r = new Regex("([1-9]+[0-9]*)");
                        Match m = r.Match(url);

                        result = GetSerializedContent(null, contentBody, m.Groups[0].Value);
                    }
                    else if (url == "/games")
                    {
                        // CancelJoin
                        User user = JsonConvert.DeserializeObject<User>(contentBody);
                        BoggleWebServer.service.CancelJoin(user);
                        // TODO: Only return the status.
                    }

                }
                // TODO: StatusCode is not properly being returned. Need to include the HttpStatusCode Number like so.
                //ss.BeginSend("HTTP/1.1", Ignore, HttpStatusCode.Forbidden);
                ss.BeginSend("HTTP/1.1 " + "201" + BoggleWebServer.StatusCode.ToString() + "\n", Ignore, null);
                ss.BeginSend("Content-Type: application/json\n", Ignore, null);
                ss.BeginSend("Content-Length: " + result.Length + "\n", Ignore, null);
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
            if (type == "users")
            {
                User requestedUser = JsonConvert.DeserializeObject<User>(content);
                UserResponse response = BoggleWebServer.service.CreateUser(requestedUser);

                return JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            else if (type == "games")
            {
                JoinGameRequest requestBody = JsonConvert.DeserializeObject<JoinGameRequest>(content);
                BoggleGameResponse response = BoggleWebServer.service.JoinGame(requestBody);

                return JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            else
            {
                BoggleWord word = JsonConvert.DeserializeObject<BoggleWord>(content);
                BoggleWordResponse response = BoggleWebServer.service.PlayWord(gameID, word);

                return JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
        }

        private void SendAPI()
        {
            var stream = BoggleWebServer.service.API();
            
            ss.BeginSend("HTTP/1.1" + BoggleWebServer.StatusCode.ToString() + "\n", Ignore, null);
            ss.BeginSend("Content-Type: text/html\n", Ignore, null); // TODO make content type a property of the server like status code?
            ss.BeginSend("Content-Length: " + stream.ToString().Length + "\n", (ex, py) => { ss.Shutdown(); }, null);
        }

        private void SendGameStatus(string gameID, string brief)
        {
            BoggleGameResponse response = BoggleWebServer.service.GameStatus(gameID, brief);
            string result = JsonConvert.SerializeObject( response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            ss.BeginSend("HTTP/1.1" + BoggleWebServer.StatusCode.ToString() + "\n", Ignore, null);
            ss.BeginSend("Content-Type: application/json\n", Ignore, null);
            ss.BeginSend("Content-Length: " + result.Length + "\n", Ignore, null);
            ss.BeginSend("\r\n", Ignore, null);
            ss.BeginSend(result, (ex, py) => { ss.Shutdown(); }, null);

        }

        private void Ignore(Exception e, object payload)
        {
        }
    }
}