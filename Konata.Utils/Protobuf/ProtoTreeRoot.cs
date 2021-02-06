using System;
using System.Collections.Generic;

using Konata.Utils.IO;
using Konata.Utils.Protobuf.ProtoModel;

namespace Konata.Utils.Protobuf
{
    using ProtoLeaves = Dictionary<string, List<IProtoType>>;

    public class ProtoTreeRoot : IProtoType
    {
        private ProtoLeaves _leaves;
        public delegate void TreeRootWriter(ProtoTreeRoot tree);
        public delegate void TreeRootReader(ProtoTreeRoot tree);

        public ProtoTreeRoot()
        {
            _leaves = new ProtoLeaves();
        }

        #region Add Methods

        public void AddTree(ProtoTreeRoot value)
        {
            _leaves = value._leaves;
        }

        public void AddTree(string treePath, ProtoTreeRoot value)
        {
            if (value != null)
                AddLeaf(treePath, value);
        }

        public void AddTree(string treePath, TreeRootWriter writer)
        {
            var newTree = new ProtoTreeRoot();
            {
                writer(newTree);
            }
            AddLeaf(treePath, newTree);
        }

        public void AddLeafString(string leafPath, string value)
        {
            if (value != null)
                AddLeaf(leafPath, ProtoLengthDelimited.Create(value));
        }

        public void AddLeafFix32(string leafPath, int? value)
        {
            if (value != null)
                AddLeaf(leafPath, ProtoBit32.Create((int)value));
        }

        public void AddLeafFix64(string leafPath, long? value)
        {
            if (value != null)
                AddLeaf(leafPath, ProtoBit64.Create((long)value));
        }

        public void AddLeafVar(string leafPath, long? value)
        {
            if (value != null)
                AddLeaf(leafPath, ProtoVarInt.Create((long)value));
        }

        public void AddLeafByteBuffer(string leafPath, ByteBuffer value)
            => AddLeafBytes(leafPath, value?.GetBytes());

        public void AddLeafBytes(string leafPath, byte[] value)
        {
            if (value != null)
                AddLeaf(leafPath, ProtoLengthDelimited.Create(value));
        }

        private void AddLeaf(string leafPath, IProtoType leaf)
        {
            if (!_leaves.TryGetValue(leafPath, out var list))
            {
                list = new List<IProtoType>();
                _leaves.Add(leafPath, list);
            }

            list.Add(leaf);
        }

        #endregion

        #region Get Methods

        public void GetTree(string treePath, TreeRootReader reader)
        {
            reader((ProtoTreeRoot)GetLeaf(treePath));
        }

        public string GetLeafString(string leafPath, out string value)
        {
            return value = GetLeaf<ProtoLengthDelimited>(leafPath).ToString();
        }

        public int GetLeafFix32(string leafPath, out int value)
        {
            return value = GetLeaf<ProtoBit32>(leafPath).Value;
        }

        public long GetLeafFix64(string leafPath, out long value)
        {
            return value = GetLeaf<ProtoBit64>(leafPath).Value;
        }

        public long GetLeafVar(string leafPath, out long value)
        {
            return value = GetLeaf<ProtoVarInt>(leafPath).Value;
        }

        public T GetLeaf<T>(string leafPath)
            where T : IProtoType => (T)GetLeaf(leafPath);

        public IProtoType GetLeaf(string leafPath)
        {
            if (_leaves.TryGetValue(leafPath, out var list))
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            return null;
        }

        public List<IProtoType> GetLeaves(string leafPath)
        {
            if (_leaves.TryGetValue(leafPath, out var list))
            {
                return list;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Retrieve proto tree values with an expression
        /// </summary>
        /// <param name="root"></param>
        /// <param name="leafPath"></param>
        /// <returns></returns>
        public static IProtoType PathTo(ProtoTreeRoot root, string leafPath)
        {
            var split = leafPath.Split('.');
            {
                if (split.Length <= 0)
                    return null;

                IProtoType nextNode;
                ProtoTreeRoot currentRoot = root;

                // Enumerate the tree
                var itor = split.GetEnumerator();
                for (int i = 0; itor.MoveNext(); ++i)
                {
                    var element = (string)itor.Current;

                    // Find first bracket '['
                    var bracketFirst = element.IndexOf("[");
                    if (bracketFirst != -1)
                    {
                        // Find reverse bracket start with bracket position
                        var bracketEnd = element.IndexOf("]", bracketFirst);
                        if (bracketEnd == -1)
                        {
                            throw new FormatException($"Expected character ']' in expression near '{element}'.");
                        }

                        // Get index
                        var index = int.Parse(element.Substring(bracketFirst + 1, bracketEnd - bracketFirst - 1));

                        // Check if value is presents
                        var leaves = currentRoot.GetLeaves(element.Replace($"[{index}]", ""));
                        if (leaves == null || leaves.Count < index)
                            return null;

                        nextNode = leaves[index];
                    }
                    else
                    {
                        nextNode = currentRoot.GetLeaf(element);
                    }

                    // If next node is not a valid root
                    if (!(nextNode is ProtoTreeRoot))
                    {
                        // And it's position at end of path
                        if (split.Length == i + 1)
                            return nextNode;
                    }

                    currentRoot = (ProtoTreeRoot)nextNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Serialize proto tree to bytebuffer
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static byte[] Serialize(ProtoTreeRoot root)
        {
            var buffer = new ByteBuffer();
            {
                foreach (var node in root._leaves)
                {
                    var pbBytes = new byte[0];

                    foreach (var element in node.Value)
                    {
                        buffer.PutBytes(ByteConverter.UnHex(node.Key));
                        {
                            switch (element)
                            {
                                case ProtoBit32 protoBit32:
                                    pbBytes = ProtoBit32.Serialize(protoBit32);
                                    break;

                                case ProtoBit64 protoBit64:
                                    pbBytes = ProtoBit64.Serialize(protoBit64);
                                    break;

                                case ProtoVarInt protoVarint:
                                    pbBytes = ProtoVarInt.Serialize(protoVarint);
                                    break;

                                case ProtoLengthDelimited protoLengthDelimited:
                                    pbBytes = ProtoLengthDelimited.Serialize(protoLengthDelimited);
                                    break;

                                case ProtoTreeRoot protoRoot:
                                    pbBytes = Serialize(protoRoot);
                                    buffer.PutBytes(ByteConverter.NumberToVarint(pbBytes.Length));
                                    break;
                            }
                        }
                        buffer.PutBytes(pbBytes);
                    }
                }
            }

            return buffer.GetBytes();
        }

        /// <summary>
        /// Construct proto tree from binary buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="recursion"></param>
        /// <returns></returns>
        public static ProtoTreeRoot Deserialize(byte[] data, bool recursion)
        {
            var tree = new ProtoTreeRoot();
            var buffer = new ByteBuffer(data);
            {
                while (buffer.RemainLength > 0)
                {
                    var pbTag = buffer.TakeVarIntBytes(out var _);
                    var pbType = ProtoUtils.VarintGetPbType(pbTag);
                    var pbPath = ByteConverter.Hex(pbTag);
                    var pbData = new byte[0];

                    switch (pbType)
                    {
                        case ProtoType.VarInt:
                            buffer.TakeVarIntBytes(out pbData);
                            tree.AddLeaf(pbPath, ProtoVarInt.Create(pbData));
                            break;

                        case ProtoType.Bit32:
                            buffer.TakeBytes(out pbData, 4);
                            tree.AddLeaf(pbPath, ProtoBit32.Create(pbData));
                            break;

                        case ProtoType.Bit64:
                            buffer.TakeBytes(out pbData, 8);
                            tree.AddLeaf(pbPath, ProtoBit64.Create(pbData));
                            break;

                        default:
                        case ProtoType.LengthDelimited:
                            buffer.TakeBytes(out pbData, (uint)buffer.TakeVarIntValueLE(out var _));

                            if (recursion)
                            {
                                try
                                {
                                    var newRoot = Deserialize(pbData, recursion);

                                    if (newRoot != null)
                                    {
                                        tree.AddLeaf(pbPath, newRoot);
                                    }
                                    else throw new Exception("catch me > w <!");
                                }
                                catch
                                {
                                    tree.AddLeaf(pbPath, ProtoLengthDelimited.Create(pbData));
                                }
                            }

                            break;
                    }
                }
            }
            return tree;
        }
    }
}
