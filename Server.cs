using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace IPSuiteCalculator
{
    class Server
    {
        private readonly TcpListener _server;

        private readonly IDistributedCache _cache;

        public const int ArraySize = 2018;
        public Server(string ip, string p, IDistributedCache cache)
        {
            IPAddress address = IPAddress.Parse(ip);
            int port = int.Parse(p);
            _server = new TcpListener(address, port);
            _server.Start();
            _cache = cache;
            StartListener();
        }

        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                _server.Stop();
            }
        }

        public void HandleClient(object? obj)
        {
            TcpClient? client = (TcpClient?)obj;
            Handle(client).GetAwaiter().GetResult();
        }

        private async Task Handle(TcpClient? client)
        {
            NetworkStream? stream = client?.GetStream();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string numbersKey = $"{threadId}numbers";
            string replyKey = $"{threadId}reply";
            byte[] bytes = new byte[256];
            int i;
            try
            {
                while (stream != null && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    List<int> numbers = new List<int>();
                    string? numbersData = await _cache.GetStringAsync(numbersKey);
                    if (!string.IsNullOrEmpty(numbersData))
                    {
                        numbers = JsonSerializer.Deserialize<List<int>>(numbersData) ?? new List<int>();
                    }
                    string dataRaw = Coder.Decode(bytes);
                    string data = dataRaw.Substring(0, dataRaw.IndexOf('\n'));

                    Console.WriteLine("{1}: Received: {0}", data, threadId);
                    string reply;
                    int num;

                    if (int.TryParse(data, out num))
                    {
                        int n = Calculator.Number(num);
                        reply = $"{new string('*', n)}\n";
                        Write(reply, stream);
                        numbers.Add(num);
                        await _cache.SetStringAsync(numbersKey, JsonSerializer.Serialize(numbers));
                        await _cache.SetStringAsync(replyKey, reply);
                    }
                    else if (numbers.Any())
                    {
                        switch (data.ToLowerInvariant())
                        {
                            case string s when s.StartsWith("check"):
                                reply = await _cache.GetStringAsync(replyKey);
                                Write(reply, stream);
                                break;
                            case "median":
                                decimal d = Calculator.Median(numbers);
                                reply = $"{d}\n";
                                Write(reply, stream);
                                await _cache.SetStringAsync(replyKey, reply);
                                break;
                            case "last":
                                reply = $"{numbers.Last()}\n";
                                Write(reply, stream);
                                await _cache.SetStringAsync(replyKey, reply);
                                break;
                            case "delete":
                                numbers.RemoveAt(numbers.Count - 1);
                                await _cache.SetStringAsync(numbersKey, JsonSerializer.Serialize(numbers));
                                reply = "Last number has been deleted!\n";
                                Write(reply, stream);
                                await _cache.SetStringAsync(replyKey, reply);
                                break;
                            case "clear":
                                numbers.Clear();
                                await _cache.RemoveAsync(numbersKey);
                                await _cache.RemoveAsync(replyKey);
                                Write($"Input cleared!\n", stream);
                                break;
                            default:
                                Write($"By the Holy Light! Unrecognized input line. Please check input string.\n", stream);
                                break;
                        }
                    }
                    else
                    {
                        switch (data.ToLowerInvariant())
                        {
                            case string s when s.StartsWith("check"):
                            case "median":
                            case "last":
                            case "delete":
                            case "clear":
                                Write("No number has been recieved yet!\n", stream);
                                break;
                            default:
                                Write($"By the Holy Light! Unrecognized input line. Please check input string.\n", stream);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await _cache.RemoveAsync(numbersKey);
                await _cache.RemoveAsync(replyKey);
                Console.WriteLine("Exception: {0}", e.ToString());
                client?.Close();
            }
        }

        private void Write(string str, NetworkStream stream)
        {
            byte[] reply = Coder.Encode(str);
            stream.Write(reply, 0, reply.Length);
            Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
        }
    }
}
