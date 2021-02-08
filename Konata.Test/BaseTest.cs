using System;

using Konata.Utils;
using Konata.Utils.IO;

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
                        Console.Write(ByteConverter.Hex((byte[])element));
                    else
                        Console.Write(element.ToString());

                    Console.Write(" ");
                }
            }
            Console.Write("\n");
        }
    }
}
