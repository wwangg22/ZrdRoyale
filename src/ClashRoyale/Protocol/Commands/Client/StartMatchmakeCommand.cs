﻿using System;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Battle;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Commands.Client
{
    public class StartMatchmakeCommand : LogicCommand
    {
        public StartMatchmakeCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public bool Is2V2 { get; set; }

        public override void Decode()
        {
            base.Decode();

            Reader.ReadVInt();
            Reader.ReadVInt();

            Is2V2 = Reader.ReadBoolean();
        }

        public override void Process()
        {
            if (Is2V2)
            {
                //await new MatchmakeFailedMessage(Device).SendAsync();
                //await new CancelMatchmakeDoneMessage(Device).SendAsync();

                var players = Resources.DuoBattles.Dequeue;
                if (players != null)
                {
                    players.Add(Device.Player);

                    var battle = new LogicBattle(false,Device.Player.Home.Arena.CurrentArena + 1, players);

                    Resources.DuoBattles.Add(battle);

                    foreach (var player in players)
                    {
                        player.Battle = battle;
                    }

                    battle.Start();
                }
                else
                {
                    Resources.DuoBattles.Enqueue(Device.Player);
                }
            }
            else
            {
                Console.WriteLine("StartMatchmakeCommand: Process: Is2V2 is false");
                var enemy = Resources.Battles.Dequeue;
                if (enemy != null)
                {
                    var battle = new LogicBattle(false, enemy.Home.Arena.CurrentArena + 1)
                    {
                        Device.Player, enemy
                    };

                    Resources.Battles.Add(battle);

                    Device.Player.Battle = battle;
                    enemy.Battle = battle;

                    battle.Start();
                }
                else
                {
                    Resources.Battles.Enqueue(Device.Player);
                }
            }
        }
    }

    public static class AdminUtilities
    {
        public static void ForceStartMatch(Device device)
        {
            // This replicates the logic in StartMatchmakeCommand.Process()
            Console.WriteLine("ForceStartMatch: Is2V2 is false");
            var enemy = Resources.Battles.Dequeue;
            if (enemy != null)
            {
                var battle = new LogicBattle(false, enemy.Home.Arena.CurrentArena + 1)
                {
                    device.Player,
                    enemy
                };

                Resources.Battles.Add(battle);

                device.Player.Battle = battle;
                device.Player.BattleMaster = false;
                enemy.Battle = battle;
                enemy.BattleMaster = true;

                battle.Start();
            }
            else
            {
                Resources.Battles.Enqueue(device.Player);
            }
        }
    }
}