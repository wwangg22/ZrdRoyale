using System;
using System.Threading;
using ClashRoyale.Core;
using ClashRoyale.Extensions.Utils;
using ClashRoyale.Utilities.Utils;
using ClashRoyale.Protocol.Commands.Client;
using ClashRoyale.Logic;
using ClashRoyale.Protocol.Commands.Server;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;

namespace ClashRoyale
{
    public static class Program
    {
        private static void Main()
        {
            Console.Title = "ZrdRoyale";
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(
                " _____            ______                    __   \n/__  /  _________/ / __ \\____  __  ______ _/ /__  \n  / /  / ___/ __  / /_/ / __ \\/ / / / __ `/ / _ \\ \n / /__/ /  / /_/ / _, _/ /_/ / /_/ / /_/ / /  __/ \n/____/_/   \\__,_/_/ |_|\\____/\\__, /\\__,_/_/\\___/ \n                            /____/               \n");
            Resources.Initialize();
            Console.WriteLine("Thanks to Incredible for work on orginal version of CR server");
            Console.WriteLine("Fork of RetroRoyale by Zordon1337");

            Console.WriteLine(Resources.Configuration.goldreward);
            Console.WriteLine(Resources.Configuration.gemsreward);
            //WebhookUtils.SendNotify(Resources.Configuration.Srv_Webhook, Resources.LangConfiguration.SrvStarting, "Server Log");
            if (ServerUtils.IsLinux())
            {
                Logger.Log("Press any key to shutdown the server.", null);
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'C' || key.KeyChar == 'c')
                    {
                        break;
                    }
                }
            }
            else
            {
                Logger.Log("Press any key to shutdown the server.", null);
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'C' || key.KeyChar == 'c')
                    {
                        break;
                    }
                    if (key.KeyChar == 'z')
                    {
                        Console.WriteLine("Force starting match");
                        foreach (var player in Resources.Players.Values)
                        {
                            Console.WriteLine("player values" + player.Home.Id);
                            var dev = player.Device;
                            AdminUtilities.ForceStartMatch(dev);
                        }
                    }
                    if (key.KeyChar == 'u')
                    {
                        foreach (var player in Resources.Players.Values)
                        {
                            if (player.Home.Id == 29)
                            {
                                // Start the TCP server once, or for each device if needed
                                Task.Run(() => TcpServerExample.StartTcpServer(player.Device));
                            }
                        }
                    }
                }
            }
            Shutdown();
        }

        public static async void Shutdown()
        {
            Console.WriteLine("Shutting down...");
            await Resources.Netty.Shutdown();

            try
            {
                Console.WriteLine("Saving players...");
                lock (Resources.Players.SyncObject)
                {
                    foreach (var player in Resources.Players.Values)
                        player.Save();
                }
                Console.WriteLine("All players saved.");
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't save all players.");
            }

            await Resources.Netty.ShutdownWorkers();
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }

    public static class TcpServerExample
    {
        // Holds all currently connected clients
        private static readonly List<TcpClient> _connectedClients = new List<TcpClient>();

        // Timer that broadcasts the "tick" (BattleTime in this case) every 0.5s
        private static System.Timers.Timer _tickTimer;

        public static async Task StartTcpServer(Device device)
        {
            // 1) Setup the timer *before* accepting clients
            SetupTickTimer(device);

            // 2) Create a TCP listener on port 5001
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5001);
            listener.Start();
            Console.WriteLine("TCP server started on 127.0.0.1:5001");

            // 3) Continuously accept incoming connections
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                // Add this client to our list of connected clients
                lock (_connectedClients)
                {
                    _connectedClients.Add(client);
                }

                // Handle this client in a separate async Task
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        /// <summary>
        /// Sets up a timer to broadcast the device's battle time every 500ms (0.5s).
        /// </summary>
        private static void SetupTickTimer(Device device)
        {
            _tickTimer = new System.Timers.Timer(250); // 500 ms
            _tickTimer.Elapsed += (sender, e) =>
            {
                // if the player is in a battle, broadcast the current battle time
                if (device?.Player?.Battle != null)
                {
                    // For example, battle time is stored in device.Player.Battle.BattleTime
                    BroadcastTick(device.Player.Battle.BattleTime);
                }
            };
            _tickTimer.AutoReset = true;
            _tickTimer.Enabled = true;
        }

        /// <summary>
        /// Handles one client in a continuous loop.
        /// The server can keep reading commands and responding without closing the connection.
        /// </summary>
        private static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream networkStream = null;
            try
            {
                networkStream = client.GetStream();
                byte[] buffer = new byte[1024];

                // Loop continuously reading data from this client
                while (true)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Client closed the connection
                        Console.WriteLine("Client disconnected (read 0).");
                        break;
                    }

                    // Convert the raw bytes into a string command
                    string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine("Received command: " + command);

                    // Example: "place 26 1 42 0 100 200"
                    // i.e., place <deviceId> <classId> <instanceId> <spellIndex> <x> <y>
                    var parts = command.Split(' ');
                    if (parts.Length == 7 && parts[0].ToLower() == "place")
                    {
                        int deviceId    = int.Parse(parts[1]);
                        int classId     = int.Parse(parts[2]);
                        int instanceId  = int.Parse(parts[3]);
                        int spellIndex  = int.Parse(parts[4]);
                        int x           = int.Parse(parts[5]);
                        int y           = int.Parse(parts[6]);

                        // Find the device by deviceId
                        var player = Resources.Players.Values
                            .FirstOrDefault(p => p.Home.Id == deviceId);

                        if (player != null)
                        {
                            Console.WriteLine("Placing card via AdminCard.Place...");
                            AdminCard.Place(
                                player.Device,
                                classId,
                                instanceId,
                                spellIndex,
                                x,
                                y
                            );

                            // Respond to the client
                            string response = $"OK: Placed card for DeviceId {deviceId}\n";
                            byte[] respBytes = Encoding.UTF8.GetBytes(response);
                            await networkStream.WriteAsync(respBytes, 0, respBytes.Length);
                        }
                        else
                        {
                            string error = $"ERROR: Could not find device with ID {deviceId}\n";
                            byte[] errBytes = Encoding.UTF8.GetBytes(error);
                            await networkStream.WriteAsync(errBytes, 0, errBytes.Length);
                        }
                    }
                    else
                    {
                        string msg = "ERROR: Invalid command format.\n" +
                            "Expected \"place <dId> <cId> <iId> <sIdx> <x> <y>\"\n";
                        byte[] errBytes = Encoding.UTF8.GetBytes(msg);
                        await networkStream.WriteAsync(errBytes, 0, errBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in HandleClientAsync: " + ex);
            }
            finally
            {
                // Remove from connected clients
                lock (_connectedClients)
                {
                    _connectedClients.Remove(client);
                }
                if (networkStream != null) networkStream.Close();
                client.Close();
                Console.WriteLine("Client disconnected (finally block).");
            }
        }

        /// <summary>
        /// Broadcast the given tickValue to all connected clients.
        /// </summary>
        private static void BroadcastTick(int tickValue)
        {
            string tickMessage = $"{tickValue}";
            byte[] tickBytes = Encoding.UTF8.GetBytes(tickMessage);

            lock (_connectedClients)
            {
                // Use ToList() to avoid modifying collection while iterating
                foreach (var c in _connectedClients.ToList())
                {
                    if (!c.Connected)
                    {
                        // If not connected, remove from the list
                        _connectedClients.Remove(c);
                        continue;
                    }

                    try
                    {
                        var stream = c.GetStream();
                        stream.Write(tickBytes, 0, tickBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to broadcast to a client: {ex}");
                        // This client might be in a bad state, so close & remove
                        try { c.Close(); } catch {}
                        _connectedClients.Remove(c);
                    }
                }
            }
        }
    }
}
