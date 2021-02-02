using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Konata.Core.Component
{
    [Component("SocketComponent", "Konata Socket Client Component")]
    public class SocketComponent : BaseComponent
    {
        public struct ServerInfo
        {
            public string Host;
            public int Port;
        }

        public static ServerInfo[] DefaultServers { get; } =
        {
            new ServerInfo { Host = "127.0.0.1", Port = 8080 },
            new ServerInfo { Host = "msfwifi.3g.qq.com", Port = 8080 },
            new ServerInfo { Host = "14.215.138.110", Port = 8080 },
            new ServerInfo { Host = "113.96.12.224", Port = 8080 },
            new ServerInfo { Host = "157.255.13.77", Port = 14000 },
            new ServerInfo { Host = "120.232.18.27", Port = 443 },
            new ServerInfo { Host = "183.3.235.162", Port = 14000 },
            new ServerInfo { Host = "163.177.89.195", Port = 443 },
            new ServerInfo { Host = "183.232.94.44", Port = 80 },
            new ServerInfo { Host = "203.205.255.224", Port = 8080 },
            new ServerInfo { Host = "203.205.255.221", Port = 8080 },
        };

        private TcpClient _client;
        private NetworkStream _clientStream;

        ~SocketComponent()
            => _client.Dispose();

        public SocketComponent()
        {
            _client = new TcpClient();
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="useLowLentency"><b>[Opt] </b>Auto select low letency server to connect.</param>
        /// <returns></returns>
        public bool Connect(bool useLowLentency = false)
        {
            var lowestTime = long.MaxValue;
            var selectHost = DefaultServers[0];

            if (useLowLentency)
            {
                foreach (var item in DefaultServers)
                {
                    var time = PingTest(item.Host, 2000);
                    {
                        if (time < lowestTime)
                        {
                            lowestTime = time;
                            selectHost = item;
                        }
                    }
                }
            }

            return Connect(selectHost.Host, selectHost.Port);
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="useLowLentency"><b>[Opt] </b>Auto select low letency server to connect.</param>
        /// <returns></returns>
        public bool Connect(string hostIp, int port)
        {
            _client.Connect(hostIp, port);
            {
                if (_client.Connected)
                {
                    _clientStream = _client.GetStream();
                }
            }

            return _client.Connected;
        }

        /// <summary>
        /// Disconnect to server
        /// </summary>
        /// <returns></returns>
        public void DisConnect()
        {
            if (_client.Connected)
            {
                _client.Close();
            }

            // BroadcastEvent(null);
        }

        /// <summary>
        /// Send data to server
        /// </summary>
        /// <param name="data"><b>[In] </b>Data buffer to send</param>
        /// <param name="recv"><b>[Out] </b>Return received buffer</param>
        /// <returns></returns>
        public bool SendData(byte[] data, out byte[] recv)
        {
            recv = null;

            if (!_client.Connected)
            {
                return false;
            }

            _clientStream.Write(data, 0, data.Length);
            return true;
        }

        //private bool RecvData(out byte[] recv)
        //{
        //    if (!_client.Connected)
        //    {

        //    }
        //}

        /// <summary>
        /// Pinging IP
        /// </summary>
        /// <param name="hostIp"><b>[In] </b>Host IP adress</param>
        /// <param name="timeout"><b>[Opt] </b>Pinging timeout default 1000 ms</param>
        /// <returns></returns>
        public static long PingTest(string hostIp, int timeout = 1000)
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send(hostIp, timeout);
                {
                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;
                    }
                }
            }

            return long.MaxValue;
        }
    }
}
