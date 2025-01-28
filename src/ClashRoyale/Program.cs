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
                " _____            ______                    __   \n" +
                "/__  /  _________/ / __ \\____  __  ______ _/ /__  \n" +
                "  / /  / ___/ __  / /_/ / __ \\/ / / / __ `/ / _ \\ \n" +
                " / /__/ /  / /_/ / _, _/ /_/ / /_/ / /_/ / /  __/ \n" +
                "/____/_/   \\__,_/_/ |_|\\____/\\__, /\\__,_/_/\\___/ \n" +
                "                            /____/               \n");
            Resources.Initialize();
            Console.WriteLine("Thanks to Incredible for the original CR server");

            // Start your normal server setup
            // ...

            // Start the TCP server so it listens for clients right away
            _ = Task.Run(() => TcpServerExample.StartTcpServer());

            // The rest of your usual main loop or key-press exit:
            Logger.Log("Press 'C' to shutdown the server...", null);
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'C' || key.KeyChar == 'c')
                {
                    break;
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
            Environment.Exit(0);
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }

    public static class TcpServerExample
    {
        // Dictionary: playerId -> ConnectionState
        private static readonly Dictionary<int, ConnectionState> _trackedPlayers = new Dictionary<int, ConnectionState>();

        private static TcpListener _listener;

        /// <summary>
        /// Start the TCP server on 127.0.0.1:5001
        /// and continuously accept new clients.
        /// </summary>
        public static async Task StartTcpServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, 5001);
                _listener.Start();
                Console.WriteLine("TCP server started on 127.0.0.1:5001");

                while (true)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in StartTcpServer: " + ex);
            }
        }

        /// <summary>
        /// Handles a newly accepted client:
        /// 1) Waits for "hello <id>".
        /// 2) If <id> is already tracked, reject.
        /// 3) Otherwise, set up tracking (used to start timer, now we won't).
        /// 4) Then handle commands (e.g., "place ...", "tick ...").
        /// </summary>
        private static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream networkStream = null;
            int trackingId = -1;

            try
            {
                networkStream = client.GetStream();
                byte[] buffer = new byte[1024];

                // 1) First message must be: "hello <id>"
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Client disconnected before sending 'hello'.");
                    return;
                }

                string helloCmd = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                var parts = helloCmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && parts[0].ToLower() == "hello")
                {
                    if (!int.TryParse(parts[1], out trackingId))
                    {
                        await SendError(networkStream, "Invalid ID in hello command.");
                        client.Close();
                        return;
                    }

                    bool alreadyTracked;
                    lock (_trackedPlayers)
                    {
                        alreadyTracked = _trackedPlayers.ContainsKey(trackingId);
                    }

                    if (alreadyTracked)
                    {
                        await SendError(networkStream, $"Player ID {trackingId} is already tracked.");
                        client.Close();
                        return;
                    }

                    // 2) We have a valid new ID. Let's find the Device in your Resources.
                    var player = Resources.Players.Values.FirstOrDefault(p => p.Home.Id == trackingId);
                    Device device = player?.Device;

                    // 3) Create a new ConnectionState and store it in the dictionary
                    var connectionState = new ConnectionState
                    {
                        PlayerId = trackingId,
                        Client = client,
                        NetworkStream = networkStream,
                        Device = device
                    };

                    lock (_trackedPlayers)
                    {
                        _trackedPlayers[trackingId] = connectionState;
                    }

                    // (Removed SetupTickTimer call; we won't auto-send ticks anymore.)

                    string okMsg = $"OK: Now tracking PlayerId {trackingId}\n";
                    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(okMsg), 0, okMsg.Length);
                }
                else
                {
                    await SendError(networkStream, "First command must be 'hello <id>'.");
                    client.Close();
                    return;
                }

                // Now handle further commands
                await ReadCommandsLoop(client, networkStream, trackingId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in HandleClientAsync: {ex}");
            }
            finally
            {
                // Cleanup
                if (networkStream != null) networkStream.Close();
                client.Close();

                if (trackingId != -1)
                {
                    lock (_trackedPlayers)
                    {
                        if (_trackedPlayers.ContainsKey(trackingId))
                        {
                            // If we had a timer, we'd stop it here. (Removed)
                            _trackedPlayers.Remove(trackingId);
                        }
                    }
                }

                Console.WriteLine("Client disconnected (finally block).");
            }
        }

        /// <summary>
        /// Continuously reads commands from the client after successful "hello".
        /// E.g. "place 26 1 42 0 100 200", "battle 26", or "tick 26".
        /// </summary>
        private static async Task ReadCommandsLoop(TcpClient client, NetworkStream networkStream, int trackingId)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch
                {
                    break;  // read fail => disconnect
                }

                if (bytesRead == 0)
                {
                    Console.WriteLine("Client disconnected (read 0).");
                    break;
                }

                string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine("Received command: " + command);

                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // 1) "battle <playerId>"
                if (parts.Length == 2 && parts[0].Equals("battle", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(parts[1], out int deviceId))
                    {
                        await SendError(networkStream, "Invalid deviceId in battle command.");
                        continue;
                    }

                    if (deviceId != trackingId)
                    {
                        string msg = $"ERROR: You are tracking {trackingId}, but tried to battle with {deviceId}.\n";
                        await networkStream.WriteAsync(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                        continue;
                    }

                    ConnectionState state;
                    bool found;
                    lock (_trackedPlayers)
                    {
                        found = _trackedPlayers.TryGetValue(trackingId, out state);
                    }

                    if (!found)
                    {
                        await SendError(networkStream, "Your tracking state was not found.");
                        break;
                    }

                    var device = state.Device;
                    if (device == null)
                    {
                        await SendError(networkStream, "No device found (player might not be loaded).");
                        continue;
                    }

                    Console.WriteLine("Force starting match via AdminUtilities.ForceStartMatch(...)");
                    AdminUtilities.ForceStartMatch(device);

                    string response = $"OK: Started battle for PlayerId {deviceId}\n";
                    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
                // 2) "place <playerId> <classId> <instanceId> <spellIndex> <x> <y>"
                else if (parts.Length == 7 && parts[0].Equals("place", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(parts[1], out int deviceId) ||
                        !int.TryParse(parts[2], out int classId) ||
                        !int.TryParse(parts[3], out int instanceId) ||
                        !int.TryParse(parts[4], out int spellIndex) ||
                        !int.TryParse(parts[5], out int x) ||
                        !int.TryParse(parts[6], out int y))
                    {
                        await SendError(networkStream, "Invalid numbers in place command.");
                        continue;
                    }

                    if (deviceId != trackingId)
                    {
                        string msg = $"ERROR: You are tracking {trackingId}, but tried to place on {deviceId}.\n";
                        await networkStream.WriteAsync(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                        continue;
                    }

                    ConnectionState state;
                    bool found;
                    lock (_trackedPlayers)
                    {
                        found = _trackedPlayers.TryGetValue(trackingId, out state);
                    }

                    if (!found)
                    {
                        await SendError(networkStream, "Your tracking state was not found.");
                        break;
                    }

                    var device = state.Device;
                    if (device == null)
                    {
                        await SendError(networkStream, "No device found (player might not be loaded).");
                        continue;
                    }

                    Console.WriteLine("Placing card via AdminCard.Place(...)");
                    AdminCard.Place(device, classId, instanceId, spellIndex, x, y);

                    string response = $"OK: Placed card for PlayerId {deviceId}\n";
                    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
                // 3) "inbattle <playerId>"
                else if (parts.Length == 2 && parts[0].Equals("inbattle", StringComparison.OrdinalIgnoreCase))
                {
                    bool isInBattle = false;
                    bool isBattleMaster = false;

                    if (int.TryParse(parts[1], out int deviceId))
                    {
                        if (deviceId == trackingId)
                        {
                            ConnectionState state;
                            lock (_trackedPlayers)
                            {
                                _trackedPlayers.TryGetValue(trackingId, out state);
                            }

                            if (state?.Device?.Player != null)
                            {
                                isInBattle = (state.Device.Player.Battle != null);
                                if (isInBattle)
                                {
                                    isBattleMaster = state.Device.Player.BattleMaster;
                                }
                            }
                        }
                    }

                    string response = $"OK: isInBattle={isInBattle}, isBattleMaster={isBattleMaster}\n";
                    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
                // 4) "tick <playerId>"
                //     The client requests the current battle time, which we return on demand.
                else if (parts.Length == 2 && parts[0].Equals("tick", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(parts[1], out int deviceId))
                    {
                        await SendError(networkStream, "Invalid deviceId in tick command.");
                        continue;
                    }

                    if (deviceId != trackingId)
                    {
                        string msg = $"ERROR: You are tracking {trackingId}, but tried to get tick for {deviceId}.\n";
                        await networkStream.WriteAsync(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                        continue;
                    }

                    ConnectionState state;
                    bool found;
                    lock (_trackedPlayers)
                    {
                        found = _trackedPlayers.TryGetValue(trackingId, out state);
                    }

                    if (!found || state.Device?.Player == null)
                    {
                        // Return 0 if we have no device/player
                        await networkStream.WriteAsync(Encoding.UTF8.GetBytes("0\n"), 0, 2);
                        continue;
                    }

                    // If the player isn't in a battle, we can return 0
                    if (state.Device.Player.Battle == null)
                    {
                        await networkStream.WriteAsync(Encoding.UTF8.GetBytes("0\n"), 0, 2);
                    }
                    else
                    {
                        int currentTick = state.Device.Player.Battle.BattleTime;
                        string resp = currentTick.ToString() + "\n";
                        byte[] respBytes = Encoding.UTF8.GetBytes(resp);
                        await networkStream.WriteAsync(respBytes, 0, respBytes.Length);
                    }
                }
                // UNKNOWN COMMAND
                else
                {
                    string msg =
                        "ERROR: Invalid command.\n" +
                        "Commands:\n" +
                        "  battle <playerId>\n" +
                        "  place <playerId> <classId> <instanceId> <spellIndex> <x> <y>\n" +
                        "  inbattle <playerId>\n" +
                        "  tick <playerId>\n";
                    await networkStream.WriteAsync(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                }
            }
        }

        // Removed SetupTickTimer and SendTick logic, since we no longer auto-send ticks.

        private static async Task SendError(NetworkStream stream, string error)
        {
            string msg = "ERROR: " + error + "\n";
            byte[] errBytes = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(errBytes, 0, errBytes.Length);
        }

        private class ConnectionState
        {
            public int PlayerId { get; set; }
            public TcpClient Client { get; set; }
            public NetworkStream NetworkStream { get; set; }
            public Device Device { get; set; }
            // No timer needed now, since we won't auto-send ticks
        }
    }
}
