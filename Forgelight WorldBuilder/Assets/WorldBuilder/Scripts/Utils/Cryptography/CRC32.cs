﻿/******************************************************************************
*  Code used from psemu's ModLauncher <https://github.com/psemu/ModLauncher>  *
*  Copyright (c) 2015 psemu                                                   *
******************************************************************************/

namespace WorldBuilder.Utils.Cryptography
{
    using System.Security.Cryptography;

    public class Crc32 : HashAlgorithm
    {
        public const   uint   DefaultPolynomial = 0xedb88320;
        public const   uint   DefaultSeed       = 0xffffffff;
        private static uint[] defaultTable;

        private          uint   hash;
        private readonly uint   seed;
        private readonly uint[] table;

        public override int HashSize => 32;

        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed  = DefaultSeed;
            Initialize();
        }

        public Crc32(uint polynomial, uint seed)
        {
            table     = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        public sealed override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public static uint Compute(byte[] buffer) => ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);

        public static uint Compute(uint seed, byte[] buffer) => ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);

        public static uint Compute(uint polynomial, uint seed, byte[] buffer) => ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
            {
                return defaultTable;
            }

            uint[] createTable = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                    {
                        entry = (entry >> 1) ^ polynomial;
                    }
                    else
                    {
                        entry = entry >> 1;
                    }
                }

                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
            {
                defaultTable = createTable;
            }

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            uint crc = seed;
            for (int i = start; i < size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ (crc & 0xff)];
                }
            }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(uint x) => new[]
        {
            (byte)((x >> 24) & 0xff),
            (byte)((x >> 16) & 0xff),
            (byte)((x >> 8) & 0xff),
            (byte)(x & 0xff)
        };
    }
}