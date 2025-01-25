using System;
using System.Collections.Generic;
using ClashRoyale.Logic;
using ClashRoyale.Utilities.Netty;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class SectorHearbeatMessage : PiranhaMessage
    {
        public SectorHearbeatMessage(Device device) : base(device)
        {
            Id = 21902;
        }

        public int Turn { get; set; }
        public Queue<byte[]> Commands { get; set; }

        public override void Encode()
        {
            Writer.WriteVInt(Turn);
            Writer.WriteVInt(0);

            Writer.WriteVInt(Commands.Count);
            // Console.WriteLine($"Commands count: {Commands}");
            // foreach (var commandBytes in Commands)
            // {
            //     // Convert the byte[] to a readable format, e.g., a hex string
            //     string byteString = BitConverter.ToString(commandBytes);
            //     Console.WriteLine($"Queue item: {byteString}");
            // }
            //  foreach (var cmdBytes in Commands)
            // {
            //    foreach (var b in cmdBytes)
            //     {
            //         int intValue = b;  // implicit conversion from byte to int
            //         Console.WriteLine($"Byte (as int): {intValue}");
            //     }
            // }

            for (int i = 0; i < Commands.Count; i++)
            {
                // Dequeue once
                var cmd = Commands.Dequeue();

                // Send it twice
                Writer.WriteBytes(cmd); // 1st time
                // Writer.WriteBytes(cmd); //    2nd time
            }
        }
    }
}