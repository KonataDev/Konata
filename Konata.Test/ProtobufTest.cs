using System;
using NUnit.Framework;

using Konata.Utils.IO;
using Konata.Utils.Protobuf;

namespace Konata.Test
{
    class ProtobufTest : BaseTest
    {
        [Test]
        [Category("Protobuf Construct")]
        public void ProtoTreeFunction()
        {
            var data = new byte[] {
                0x12, 0x13, 0x08, 0x01, 0x10, 0x00, 0x18, 0xD2,
                0x09, 0x22, 0x06, 0x08, 0xCB, 0x3A, 0x10, 0x9D,
                0x12, 0x2A, 0x02, 0x08, 0x00
            };

            var newData = ProtoTreeRoot.Serialize
                (ProtoTreeRoot.Deserialize(data, true));

            var hex = ByteConverter.Hex(data);
            var newHex = ByteConverter.Hex(newData.GetBytes());

            Assert.AreEqual(hex, newHex);
        }
    }
}
