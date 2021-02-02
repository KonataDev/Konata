using System;

using Konata.Utils;

namespace Konata.Test
{
    public class BaseTest
    {
        public void PrintBytes(params object[] args)
        {
            Console.Write($"[ .... ] ");
            {
                foreach (var element in args)
                {
                    if (element.GetType() == typeof(byte[]))
                        Console.Write(Hex.Bytes2HexStr((byte[])element));
                    else
                        Console.Write(element.ToString());

                    Console.Write(" ");
                }
            }
            Console.Write("\n");
        }
    }
}
