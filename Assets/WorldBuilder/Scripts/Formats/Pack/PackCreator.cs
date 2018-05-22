﻿/******************************************************************************
*  Code modified from psemu's ModLauncher <https://github.com/psemu/ModLauncher>  *
*  Copyright (c) 2015 psemu                                                   *
******************************************************************************/

namespace WorldBuilder.Formats.Pack
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Syroot.BinaryData;
    using Utils.Cryptography;

    public class PackCreator
    {
        private class FileHeader
        {
            public uint name_len;
            public byte[] name;
            public uint offset;
            public uint length;
            public uint crc32;

            public byte[] Encode()
            {
                byte[] data;

                using (MemoryStream memStream = new MemoryStream())
                using (BinaryStream wr = new BinaryStream(memStream, ByteConverter.Big))
                {
                    wr.Write(name_len);
                    wr.Write(name);
                    wr.Write(offset);
                    wr.Write(length);
                    wr.Write(crc32);

                    data = memStream.ToArray();
                }

                return data;
            }
        }

        private class Chunk
        {
            public List<FileHeader> fheader = new List<FileHeader>();
            public List<byte[]> fileData = new List<byte[]>();

            public uint NextChunkOffset;
            public uint FileCount;
            public FileHeader[] files;

            public byte[] Encode()
            {
                byte[] data;

                using (MemoryStream memStream = new MemoryStream())
                using (BinaryStream stream = new BinaryStream(memStream, ByteConverter.Big))
                {
                    stream.Write(NextChunkOffset);
                    stream.Write(FileCount);

                    foreach (FileHeader h in files)
                    {
                        stream.Write(h.Encode());
                    }

                    data = memStream.ToArray();
                }

                return data;
            }
        }

        public static void PackDirectory()
        {
            throw new NotImplementedException();
        }

        private static void CreatePackFromFiles(string[] files, string savePath)
        {
            List<Chunk> chunks = new List<Chunk>();

            Chunk chunk = new Chunk();

            uint chunkFilesProcessed = 0;

            //Create File Headers for each file to pack
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                byte[] fdata = File.ReadAllBytes(file);
                chunk.fileData.Add(fdata);

                Crc32 crc = new Crc32();
                byte[] c = crc.ComputeHash(fdata);

                string filename = file.Substring(file.LastIndexOf("\\") + 1);

                FileHeader h = new FileHeader
                {
                    name_len = (uint)filename.Length,
                    name = Encoding.ASCII.GetBytes(filename),
                    offset = 0,
                    length = (uint)fdata.Length,
                    crc32 = (uint)BitConverter.ToInt32(c, 0)
                };

                chunk.fheader.Add(h);

                chunkFilesProcessed++;

                if (chunkFilesProcessed != 255) //Chunks can only hold 255 assets. We create a new chunk after we hit this limit.
                {
                    continue;
                }

                chunk.FileCount = chunkFilesProcessed;
                chunks.Add(chunk);

                chunk = new Chunk();

                chunkFilesProcessed = 0;
            }

            chunk.FileCount = chunkFilesProcessed;
            chunks.Add(chunk);

            // Create a chunk header for each chunk
            for (int i = 0; i < chunks.Count; i++)
            {
                Chunk chunki = chunks[i];

                chunki.NextChunkOffset = 0;
                chunki.files = chunki.fheader.ToArray();

                // Update the files with the offset of each data chunk of the file
                int length = chunki.Encode().Length;
                int offset = 0;
                if (i > 0)
                {
                    offset = (int)chunks[i - 1].NextChunkOffset;
                }

                for (int j = 0; j < chunki.fheader.Count; j++)
                {
                    if (j != 0)
                    {
                        offset += chunki.fileData[j - 1].Length;
                    }

                    chunki.fheader[j].offset = (uint)length + (uint)offset;
                }

                offset += chunki.fileData[chunki.fheader.Count - 1].Length;
                offset += length;

                // There is no next chunk.
                if (i == chunks.Count - 1)
                {
                    break;
                }

                chunki.NextChunkOffset = (uint)offset;
            }

            // Write the chunks to a file
            using (FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate))
            using (BinaryStream wr = new BinaryStream(fileStream, ByteConverter.Big))
            {
                foreach (Chunk chunki in chunks)
                {
                    byte[] ph = chunki.Encode();
                    wr.Write(ph);

                    foreach (byte[] t in chunki.fileData)
                    {
                        wr.Write(t);
                    }
                }
            }
        }
    }
}
