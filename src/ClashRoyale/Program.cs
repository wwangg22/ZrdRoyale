using System;
using System.Threading;
using ClashRoyale.Core;
using ClashRoyale.Extensions.Utils;
using ClashRoyale.Utilities.Utils;

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
            WebhookUtils.SendNotify(Resources.Configuration.Srv_Webhook, Resources.LangConfiguration.SrvStarting, "Server Log");
            if (ServerUtils.IsLinux())
            {
                // idk why orginal dev removed this lol
                Logger.Log("Press any key to shutdown the server.", null);
                Console.Read();
            }
            else
            {
                Logger.Log("Press any key to shutdown the server.", null);
                
                Console.Read();
            }
            Shutdown();
            WebhookUtils.SendError(Resources.Configuration.Srv_Webhook, Resources.LangConfiguration.SrvClosing, "Server Log");
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
                    foreach (var player in Resources.Players.Values) player.Save();
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
}
