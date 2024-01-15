using Onyxalis.Objects.Worlds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
namespace Onyxalis.Objects.Systems
{
    public static class ObjectSerializer
    {


        public static string SerializeToHex(Chunk chunk)
        {
            // Serialize object to a byte array
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, chunk);
                byte[] bytes = memoryStream.ToArray();

                // Convert byte array to hex string
                var stringBuilder = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    stringBuilder.AppendFormat("{0:x2}", b);
                }

                return stringBuilder.ToString();
            }
        }

        public static Chunk DeserializeFromHex(string hexString)
        {
            // Convert hex string to byte array
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

            // Deserialize byte array to a Chunk object
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(bytes))
            {
                return (Chunk)binaryFormatter.Deserialize(memoryStream);
            }
        }

    }
}
