using System;
using NUnit.Framework;

using Konata.Utils.IO;
using Konata.Utils.Protobuf;

namespace Konata.Test
{
    [TestFixture(Description = "工具类组件测试")]
    public class UtilsTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine($"开始进行工具类测试");
        }

        [Test]
        [Category("byte缓存字节测试")]
        public void ByteBufferFunction()
        {
            var buffer = new ByteBuffer();

            buffer.PutBytes(new byte[0],
                ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
            buffer.PutString("Test",
                ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

            buffer.TakeUintBE(out var length);
            buffer.TakeString(out var outstr,
                ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

            Assert.AreEqual(length, 4);
            Assert.AreEqual(outstr, "Test");
        }
    }
}
