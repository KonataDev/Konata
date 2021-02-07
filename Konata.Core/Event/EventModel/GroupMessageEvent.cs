using System;
using System.Collections.Generic;

namespace Konata.Core.Event.EventModel
{
    public class GroupMessageEvent : ProtocolEvent
    {
        public string GroupName { get; set; }

        public uint GroupUin { get; set; }

        public uint MemberUin { get; set; }

        public string MemberCard { get; set; }

        public List<GroupMessageChain> Message { get; set; }

        public uint MessageId { get; set; }

        public uint MessageTime { get; set; }

        public uint SliceTotal { get; set; }

        public uint SliceIndex { get; set; }

        public uint SliceFlags { get; set; }

        public override string ToString()
        {
            if (Message == null)
                return "";

            var content = "";
            foreach (var element in Message)
            {
                content += element.ToString();
            }

            return content;
        }

        public GroupMessageEvent()
            => WaitForResponse = true;
    }

    public class GroupMessageChain
    {
        public enum ChainType
        {
            At,         // At
            Text,       // Ttext
            Image,      // Image
            Record,     // Record
            Emoji,      // QQ Emoji
            QFace,      // QQ Face
        }

        public ChainType Type { get; set; }
    }

    public class ChainAt : GroupMessageChain
    {
        public uint AtUin { get; set; }

        public ChainAt()
            => Type = ChainType.At;

        public override string ToString()
            => $"[KQ:at,qq={(AtUin == 0 ? "all" : AtUin.ToString())}]";
    }

    public class ChainText : GroupMessageChain
    {
        public string Content { get; set; }

        public ChainText()
            => Type = ChainType.Text;

        public override string ToString()
            => Content;
    }

    public class ChainEmoji : GroupMessageChain
    {
        public uint CodePoint;

        public ChainEmoji()
            => Type = ChainType.Emoji;

        public override string ToString()
            => $"[KQ:emoji,id={CodePoint}]";
    }

    public class ChainQFace : GroupMessageChain
    {
        public uint FaceId;

        public ChainQFace()
            => Type = ChainType.QFace;

        public override string ToString()
            => $"[KQ:face,id={FaceId}]";
    }

    public class ChainImage : GroupMessageChain
    {
        public string ImageUrl { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public ChainImage()
            => Type = ChainType.Image;

        public override string ToString()
            => $"[KQ:image,file={FileName}]";
    }

    public class ChainRecord : GroupMessageChain
    {
        public string FileName { get; set; }

        public ChainRecord()
            => Type = ChainType.Record;

        public override string ToString()
            => $"[KQ:record,file={FileName}]";
    }
}
