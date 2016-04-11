using CustomNetworking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

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
    }

    class HttpRequest
    {
        private StringSocket ss;
        private int lineCount;
        private int contentLength;

        public HttpRequest(StringSocket stringSocket)
        {
            this.ss = stringSocket;
            this.contentLength = 0;
            this.lineCount = 0;

            ss.BeginReceive(LineReceived, null);
        }

        private string method;
        private string url;

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
                    Console.WriteLine("Method: " + m.Groups[1].Value);
                    Console.WriteLine("URL: " + m.Groups[2].Value);
                    method = m.Groups[1].Value;
                    url = m.Groups[2].Value;
                }
                if (s.StartsWith("Content-Length:"))
                {
                    contentLength = Int32.Parse(s.Substring(16).Trim());
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
                // TODO: Not sure if this is what we need to do. I'm kinda lost on
                // what we need to do at this moment. Just playing around to figure
                // out what we need to do.
                BoggleService service = new BoggleService();
                string result = "";

                if(method == "POST")
                {
                    if (url.Contains("users"))
                    {
                        // CreateUser
                        User requestedUser = JsonConvert.DeserializeObject<User>(contentBody);
                        Console.Write(requestedUser.Nickname);
                        UserResponse response = service.CreateUser(requestedUser);
                        if(service.StatusCode == "Created")
                        {
                            result = JsonConvert.SerializeObject( response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        }
                    }
                    else
                    {
                        // JoinGame
                    }

                }
                else if(method == "PUT")
                {
                    if(url.Contains("games/"))
                    {
                        //PlayWord
                    }
                    else
                    {
                        // CancelJoin
                    }

                }
                else if(method == "GET" && url.Contains("games/"))
                {
                    // GameStatus
                }


                //Person p = JsonConvert.DeserializeObject<Person>(s);
                //Console.WriteLine(p.Name + " " + p.Eyes);
                //BoggleService n = new BoggleService();
                // Call service method
                //string result =
                //    JsonConvert.SerializeObject(
                //            new Person { Name = "June", Eyes = "Blue" },
                //            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                ss.BeginSend("HTTP/1.1", Ignore, HttpStatusCode.Forbidden);
                ss.BeginSend("HTTP/1.1" + service.StatusCode + "\n", Ignore, null);
                ss.BeginSend("Content-Type: application/json\n", Ignore, null);
                ss.BeginSend("Content-Length: " + result.Length + "\n", Ignore, null);
                ss.BeginSend("\r\n", Ignore, null);
                ss.BeginSend(result, (ex, py) => { ss.Shutdown(); }, null);
            }
        }

        private void Ignore(Exception e, object payload)
        {
        }
    }

    public class Person
    {
        public String Name { get; set; }
        public String Eyes { get; set; }
    }
}