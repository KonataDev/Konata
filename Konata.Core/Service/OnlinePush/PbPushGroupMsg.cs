using System;
using System.Collections.Generic;
using System.Linq;
using Konata.Core.Event;
using Konata.Core.Event.EventModel;
using Konata.Core.Packet;
using Konata.Utils.IO;
using Konata.Utils.Protobuf;

namespace Konata.Core.Service.OnlinePush
{
    [Service("OnlinePush.PbPushGroupMsg", "Receive group message from server")]
    [Event(typeof(GroupMessageEvent))]
    public class PbPushGroupMsg : IService
    {
        public bool Parse(SSOFrame input, SignInfo signInfo, out ProtocolEvent output)
        {
            var message = new GroupMessageEvent();
            {
                var root = ProtoTreeRoot.Deserialize(input.Payload, true);
                {
                    // Parse message source information
                    var sourceRoot = (ProtoTreeRoot)ProtoTreeRoot.PathTo(root, "0A.0A");
                    {
                        message.MemberUin = (uint)sourceRoot.GetLeafVar("08");
                        message.MessageTime = (uint)sourceRoot.GetLeafVar("30");

                        sourceRoot = (ProtoTreeRoot)ProtoTreeRoot.PathTo(sourceRoot, "4A");
                        {
                            message.GroupUin = (uint)sourceRoot.GetLeafVar("08");
                            message.MemberCard = sourceRoot.GetLeafString("22");
                            message.GroupName = sourceRoot.GetLeafString("42");
                        }
                    }

                    // Parse message slice information
                    var sliceInfoRoot = (ProtoTreeRoot)ProtoTreeRoot.PathTo(root, "0A.12");
                    {
                        message.SliceTotal = (uint)sliceInfoRoot.GetLeafVar("08");
                        message.SliceIndex = (uint)sliceInfoRoot.GetLeafVar("10");
                        message.SliceFlags = (uint)sliceInfoRoot.GetLeafVar("18");
                    }

                    // Parse message content
                    var contentRoot = (ProtoTreeRoot)ProtoTreeRoot.PathTo(root, "0A.1A.0A");
                    {
                        List<GroupMessageChain> list = new List<GroupMessageChain>();

                        contentRoot.ForEach((_, __) =>
                        {
                            if (_ == "12")
                            {
                                ((ProtoTreeRoot)__).ForEach((key, value) =>
                                {
                                    GroupMessageChain chain = null;
                                    try
                                    {
                                        switch (key)
                                        {
                                            case "0A":
                                                chain = ParsePlainText((ProtoTreeRoot)value);
                                                break;

                                            case "12":
                                                chain = ParseQFace((ProtoTreeRoot)value);
                                                break;

                                            case "42":
                                                chain = ParsePicture((ProtoTreeRoot)value);
                                                break;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message, e.StackTrace);
                                    }

                                    if (chain != null)
                                    {
                                        list.Add(chain);
                                    }
                                });
                            }
                        });

                        message.Message = list;
                    }

                }
            }

            output = message;
            return true;
        }

        /// <summary>
        /// Process Picture chain
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private GroupMessageChain ParsePicture(ProtoTreeRoot tree)
        {
            return new ChainImage
            {
                ImageUrl = tree.GetLeafString("8201"),
                FileHash = ByteConverter.Hex(tree.GetLeafBytes("6A")),
                FileName = tree.GetLeafString("12").Replace("{", "").Replace("}", "").Replace("-", "")
            };
        }

        /// <summary>
        /// Process Text and At chain
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private GroupMessageChain ParsePlainText(ProtoTreeRoot tree)
        {
            // At chain
            if (tree.TryGetLeafBytes("1A", out var atBytes))
            {
                var at = ByteConverter.BytesToUInt32
                    (atBytes.Skip(7).Take(4).ToArray(), 0, Endian.Big);

                return new ChainAt { AtUin = at };
            }

            // Plain text chain
            if (tree.TryGetLeafString("0A", out var content))
            {
                return new ChainText { Content = content };
            }

            return null;
        }

        /// <summary>
        /// Process QFace chain
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private GroupMessageChain ParseQFace(ProtoTreeRoot tree)
        {
            return new ChainQFace
            {
                FaceId = (uint)tree.GetLeafVar("08")
            };
        }

        public bool Build(Sequence sequence, ProtocolEvent input, SignInfo signInfo,
            out int newSequence, out byte[] output) => throw new NotImplementedException();
    }
}
