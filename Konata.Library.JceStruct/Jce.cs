﻿using Konata.Library.IO;
using System;
using System.Collections.Generic;

namespace Konata.Library.JceStruct
{
    public static partial class Jce
    {
        public static byte[] Serialize(Struct jce)
        {
            Buffer buffer = new Buffer();

            void PutObject(IObject obj, byte tag = 0)
            {
                Type type = obj.Type;
                buffer.PutJceHead(tag, type);
                switch (type)
                {
                case Type.Byte:
                    buffer.PutSbyte(((Number)obj).ValueByte);
                    break;
                case Type.Short:
                    buffer.PutShortBE(((Number)obj).ValueShort);
                    break;
                case Type.Int:
                    buffer.PutIntBE(((Number)obj).ValueInt);
                    break;
                case Type.Long:
                    buffer.PutLongBE((Number)obj);
                    break;
                case Type.Float:
                    buffer.PutFloatBE((Float)obj);
                    break;
                case Type.Double:
                    buffer.PutDoubleBE((Double)obj);
                    break;
                case Type.String1:
                    buffer.PutString((String)obj, ByteBuffer.Prefix.Uint8);
                    break;
                case Type.String4:
                    buffer.PutString((String)obj, ByteBuffer.Prefix.Uint32);
                    break;
                case Type.Map:
                    {
                        Map map = (Map)obj;
                        PutObject((Number)map.Count);
                        if (map.Count > 0)
                        {
                            foreach (IObject key in map.Keys)
                            {
                                PutObject(key);
                            }
                            foreach (IObject value in map.Values)
                            {
                                PutObject(value, 1);
                            }
                        }
                    }
                    break;
                case Type.List:
                    {
                        List list = (List)obj;
                        PutObject((Number)list.Count);
                        if (list.Count > 0)
                        {
                            foreach (IObject value in list)
                            {
                                PutObject(value);
                            }
                        }
                    }
                    break;
                case Type.StructBegin:
                    buffer.PutBytes(Serialize((Struct)obj));
                    buffer.PutByte((byte)Type.StructEnd);
                    break;
                case Type.ZeroTag:
                    break;
                case Type.SimpleList:
                    buffer.PutByte(0);
                    PutObject((Number)((SimpleList)obj).Length);
                    buffer.PutBytes(((SimpleList)obj).Value);
                    break;
                default:
                    throw new NotImplementedException();
                }
            }

            foreach (KeyValuePair<byte, IObject> pair in jce)
            {
                PutObject(pair.Value, pair.Key);
            }
            return buffer.GetBytes();
        }

        public static Struct Deserialize(byte[] data)
        {
            Buffer buffer = new Buffer(data);

            int TakeJceInt()
            {
                buffer.TakeJceHead(out Type type);
                switch (type)
                {
                case Type.Byte:
                    return buffer.TakeSbyte(out _);
                case Type.Short:
                    return buffer.TakeShortBE(out _);
                case Type.Int:
                    return buffer.TakeIntBE(out _);
                case Type.ZeroTag:
                    return 0;
                default:
                    throw new NotImplementedException();
                }
            }

            IObject TakeJceObject(out byte tag)
            {
                tag = buffer.TakeJceHead(out Type type);
                switch (type)
                {
                case Type.Byte:
                    return (Number)buffer.TakeSbyte(out _);
                case Type.Short:
                    return (Number)buffer.TakeShortBE(out _);
                case Type.Int:
                    return (Number)buffer.TakeIntBE(out _);
                case Type.Long:
                    return (Number)buffer.TakeLongBE(out _);
                case Type.Float:
                    return (Float)buffer.TakeFloatBE(out _);
                case Type.Double:
                    return (Double)buffer.TakeDoubleBE(out _);
                case Type.String1:
                    return (String)buffer.TakeString(out _, ByteBuffer.Prefix.Uint8);
                case Type.String4:
                    return (String)buffer.TakeString(out _, ByteBuffer.Prefix.Uint32);
                case Type.Map:
                    {
                        int count = TakeJceInt();
                        if (count > 0)
                        {
                            List<IObject> keys = new List<IObject>(count);
                            for (int i = 0; i < count; ++i)
                            {
                                keys.Add(TakeJceObject(out _));
                            }
                            List<IObject> values = new List<IObject>(count);
                            for (int i = 0; i < count; ++i)
                            {
                                values.Add(TakeJceObject(out _));
                            }
                            return new Map(keys, values);
                        }
                        return new Map();
                    }
                case Type.List:
                    {
                        int count = TakeJceInt();
                        if (count > 0)
                        {
                            List list = new List(count);
                            for (int i = 0; i < count; ++i)
                            {
                                list.Add(TakeJceObject(out _));
                            }
                            return list;
                        }
                        return new List();
                    }
                case Type.StructBegin:
                    return TakeJceStruct();
                case Type.StructEnd:
                    return null; // Null object is only allowed here.
                case Type.ZeroTag:
                    return default(Number);
                case Type.SimpleList:
                    buffer.EatBytes(1);
                    return (SimpleList)buffer.TakeBytes(out _, (uint)TakeJceInt());
                default:
                    throw new NotImplementedException();
                }
            }

            Struct TakeJceStruct()
            {
                Struct result = new Struct();
                while (buffer.RemainLength > 0)
                {
                    IObject obj = TakeJceObject(out byte tag);
                    if (obj is null) // Meets JceType.StructEnd.
                    {
                        break;
                    }
                    else
                    {
                        result.Add(tag, obj);
                    }
                }
                return result;
            }

            return TakeJceStruct();
        }
    }
}