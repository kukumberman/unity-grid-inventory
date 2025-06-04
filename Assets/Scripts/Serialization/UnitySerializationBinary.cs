using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe.NotBurstCompatible;
using Unity.Serialization.Binary;

public sealed class UnitySerializationBinary : ISerialization
{
    private const int kSignatureSize = 32;

    private BinarySerializationParameters _parameters;

    public UnitySerializationBinary(InventoryManager manager)
    {
        _parameters = new BinarySerializationParameters
        {
            UserDefinedAdapters = new List<IBinaryAdapter>() { new ItemAdapter(manager), }
        };
    }

    public unsafe byte[] Serialize(TetrisInventory inventory)
    {
        var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        BinarySerialization.ToBinary(&stream, inventory, _parameters);
        var bytes = stream.ToBytesNBC();
        stream.Dispose();

        var hash = ComputeHash(bytes);

        var list = new List<byte>(hash.Length + bytes.Length);

        list.AddRange(hash);
        list.AddRange(bytes);

        return list.ToArray();
    }

    public unsafe TetrisInventory Deserialize(byte[] bytes)
    {
        if (bytes.Length < kSignatureSize)
        {
            return null;
        }

        var existingHash = new byte[kSignatureSize];
        Array.Copy(bytes, 0, existingHash, 0, kSignatureSize);

        var contents = new byte[bytes.Length - kSignatureSize];
        Array.Copy(bytes, kSignatureSize, contents, 0, contents.Length);

        var hash = ComputeHash(contents);

        if (!CompareByteArray(existingHash, hash))
        {
            return null;
        }

        fixed (byte* ptr = contents)
        {
            var bufferReader = new UnsafeAppendBuffer.Reader(ptr, bytes.Length);

            var inventory = BinarySerialization.FromBinary<TetrisInventory>(
                &bufferReader,
                _parameters
            );
            return inventory;
        }
    }

    private static byte[] ComputeHash(byte[] bytes)
    {
        using var sha256 = new SHA256Managed();
        byte[] hash = sha256.ComputeHash(bytes);

        return hash;
    }

    private static bool CompareByteArray(byte[] lhs, byte[] rhs)
    {
        if (lhs.Length != rhs.Length)
        {
            return false;
        }

        for (int i = 0; i < lhs.Length; i++)
        {
            if (lhs[i] != rhs[i])
            {
                return false;
            }
        }

        return true;
    }
}
