﻿using System;
using System.Linq;
using System.Runtime.Remoting.Channels;

namespace Konata.Library.JceStruct
{
    public static partial class Jce
    {
        public struct SimpleList : IIndexable
        {
            public Type Type => Type.SimpleList;

            public BaseType BaseType => BaseType.ByteArray;

            public long Length => Value is null ? 0 : Value.LongLength;

            public byte[] Value { get; set; }

            public IObject this[string path]
            {
                get
                {
                    if (Value is null)
                    {
                        throw new NullReferenceException("Jce.Struct.Value is null.");
                    }
                    if (valueStruct is null || !Enumerable.SequenceEqual(Value, valueContent))
                    {
                        valueContent = new byte[Value.Length];
                        Value.CopyTo(valueContent, 0);
                        try
                        {
                            valueStruct = valueContent;
                        }
                        catch
                        {
                            throw new InvalidCastException();
                        }
                    }
                    return valueStruct[path];
                }
            }

            public SimpleList(byte[] value)
            {
                Value = value;
                valueStruct = null;
                valueContent = null;
            }

            public override bool Equals(object obj) =>
                obj is SimpleList other &&
                Enumerable.SequenceEqual(Value, other.Value);

            public static implicit operator byte[](SimpleList value) => value.Value;

            public static implicit operator SimpleList(byte[] value) => new SimpleList(value);

            public static implicit operator Struct(SimpleList value) => value.Value;

            public static implicit operator SimpleList(Struct value) => new SimpleList(value);

            private Struct valueStruct;
            private byte[] valueContent;
        }
    }
}