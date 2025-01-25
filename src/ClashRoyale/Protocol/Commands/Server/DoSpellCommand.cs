using System;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Home.Decks.Items;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Commands.Server
{
    public class DoSpellCommand : LogicCommand
    {
        public DoSpellCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Type = 1;
        }

        public int ClientTick { get; set; }
        public int Checksum { get; set; }
        public int SenderHighId { get; set; }
        public int SenderLowId { get; set; }
        public int SpellDeckIndex { get; set; }
        public int SpellIndex { get; set; }
        public int ClassId { get; set; }
        public int InstanceId { get; set; }
        public int TroopLevel { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override void Decode()
        {
            // Header
            {
                ClientTick = Reader.ReadVInt();
                Checksum = Reader.ReadVInt();

                SenderHighId = Reader.ReadVInt();
                SenderLowId = Reader.ReadVInt();
            }

            SpellDeckIndex = Reader.ReadVInt();
            // SpellDeckIndex = 0;
            Console.WriteLine($"[Decode] SpellDeckIndex={SpellDeckIndex}");

            ClassId = Reader.ReadVInt();
            InstanceId = Reader.ReadVInt();
            Console.WriteLine($"[Decode] ClassId={ClassId}, InstanceId={InstanceId}");

            SpellIndex = Reader.ReadVInt();
            Console.WriteLine($"[Decode] SpellIndex={SpellIndex}");

            TroopLevel = Reader.ReadVInt();
            Console.WriteLine($"[Decode] TroopLevel={TroopLevel}");

            X = Reader.ReadVInt();
            Console.WriteLine($"[Decode] X={X}");
            Y = Reader.ReadVInt();
            // Y = 8499;
            Console.WriteLine($"[Decode] Y={Y}");
        }

        public override void Encode()
        {
            // Header
            {
                Data.WriteVInt(Type);

                Data.WriteVInt(ClientTick);
                Data.WriteVInt(Checksum);

                Data.WriteVInt(SenderHighId);
                Data.WriteVInt(SenderLowId);
            }

            Data.WriteVInt(SpellDeckIndex);

            Data.WriteVInt(ClassId);
            Data.WriteVInt(InstanceId);

            Data.WriteVInt(SpellIndex);
        }

        public override void Process()
        {
            var battle = Device.Player.Battle;
            if (battle == null) return;

            // Save the current ReaderIndex:
    int savedIndex = Data.ReaderIndex;  

    // "Peek" each field:
    int type           = Data.ReadVInt();
    int clientTick     = Data.ReadVInt();
    int checksum       = Data.ReadVInt();
    int senderHighId   = Data.ReadVInt();
    int senderLowId    = Data.ReadVInt();
    int spellDeckIndex = Data.ReadVInt();
    int classId        = Data.ReadVInt();
    int instanceId     = Data.ReadVInt();
    int spellIndex     = Data.ReadVInt();

    // Print them:
    Console.WriteLine($"[Process] Type={type}, ClientTick={clientTick}, Checksum={checksum},");
    Console.WriteLine($"           SenderHighId={senderHighId}, SenderLowId={senderLowId}, SpellDeckIndex={spellDeckIndex},");
    Console.WriteLine($"           ClassId={classId}, InstanceId={instanceId}, SpellIndex={spellIndex}");
    int server_tick = battle.BattleTime;
    Console.WriteLine($"[Process] ServerTick={server_tick}");
    // Restore the ReaderIndex so future code (like reading raw bytes) still works:
    Data.SetReaderIndex(savedIndex);

    // Now do the final read for raw bytes if you want to keep the existing logic:
    var data = Data.ReadBytes(Data.ReadableBytes).Array;

            var buffer = Unpooled.Buffer(9);
            {
                buffer.WriteBytes(data); 

                buffer.WriteVInt(TroopLevel);

                buffer.WriteVInt(X);
                buffer.WriteVInt(Y);

                battle.GetOwnQueue(Device.Player.Home.Id).Enqueue(buffer.Array);
            }

            var enemyBuffer = Unpooled.Buffer(9);
            {
                enemyBuffer.WriteBytes(data);

                enemyBuffer.WriteVInt(1); // IsAttack
                {
                    enemyBuffer.WriteVInt(Card.Id(ClassId, InstanceId));
                }

                enemyBuffer.WriteVInt(TroopLevel);

                enemyBuffer.WriteVInt(X);
                enemyBuffer.WriteVInt(Y);

                if (battle.Is2V2)
                {
                    var others = battle.GetOtherQueues(Device.Player.Home.Id);

                    foreach (var queue in others)
                    {
                        queue.Enqueue(enemyBuffer.Array);
                    }
                }
                else 
                    battle.GetEnemyQueue(Device.Player.Home.Id).Enqueue(enemyBuffer.Array);
            }

            //battle.Replay.AddCommand(Type, ClientTick - 20, ClientTick, SenderHighId, SenderLowId, ClassId * 1000000 + InstanceId, X, Y, SpellDeckIndex);
        }
    }

    public class AdminCard {
        public static void Place(Device Device, int classId, int instanceId,int spellIndex,  int x, int y){
            var battle = Device.Player.Battle;
            if (battle == null) return;
            Console.WriteLine("Placing card");


            // var data = Data.ReadBytes(Data.ReadableBytes).Array;

            var buffer = Unpooled.Buffer(9);
            {
                 {
                buffer.WriteVInt(1);

                buffer.WriteVInt(battle.BattleTime * 10);
                buffer.WriteVInt(-1);

                buffer.WriteVInt(0);
                buffer.WriteVInt((int) Device.Player.Home.Id);
                }

                buffer.WriteVInt(spellIndex);

                buffer.WriteVInt(classId);
                buffer.WriteVInt(instanceId);

                buffer.WriteVInt(-1); 

                buffer.WriteVInt(0);

                buffer.WriteVInt(x);
                buffer.WriteVInt(y);

                battle.GetOwnQueue(Device.Player.Home.Id).Enqueue(buffer.Array);
            }

            var enemyBuffer = Unpooled.Buffer(9);
            {
                 {
                enemyBuffer.WriteVInt(1);

                enemyBuffer.WriteVInt(battle.BattleTime * 10);
                enemyBuffer.WriteVInt(-1);

                enemyBuffer.WriteVInt(0);
                enemyBuffer.WriteVInt((int) Device.Player.Home.Id);
                }

                enemyBuffer.WriteVInt(spellIndex);

                enemyBuffer.WriteVInt(classId);
                enemyBuffer.WriteVInt(instanceId);

                enemyBuffer.WriteVInt(-1); 


                enemyBuffer.WriteVInt(1); // IsAttack
                {
                    enemyBuffer.WriteVInt(Card.Id(classId, instanceId));
                }

                enemyBuffer.WriteVInt(0);

                enemyBuffer.WriteVInt(x);
                enemyBuffer.WriteVInt(y);

                if (battle.Is2V2)
                {
                    var others = battle.GetOtherQueues(Device.Player.Home.Id);

                    foreach (var queue in others)
                    {
                        queue.Enqueue(enemyBuffer.Array);
                    }
                }
                else 
                    battle.GetEnemyQueue(Device.Player.Home.Id).Enqueue(enemyBuffer.Array);
            }
        }
    }
    
}