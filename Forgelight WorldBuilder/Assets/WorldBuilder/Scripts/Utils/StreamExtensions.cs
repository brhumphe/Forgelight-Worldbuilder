﻿namespace WorldBuilder.Utils
{
    using System.IO;
    using Syroot.BinaryData;
    using UnityEngine;

    /// <summary>
    /// BinaryReader/Writer extensions to include support for basic Unity data types.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads bytes into an existing buffer, creating one, or enlarging the buffer if necessary.
        /// </summary>
        /// <param name="stream">The source stream</param>
        /// <param name="buffer">The target buffer to read to.</param>
        /// <param name="count">The number of bytes to read.</param>
        public static void ReadBytes(this Stream stream, Buffer<byte> buffer, int count)
        {
            buffer.PrepareBuffer(count);
            stream.Read(buffer.Data, 0, count);
        }

        public static Vector2 ReadVector2(this Stream stream)
        {
            return new Vector2(stream.ReadSingle(), stream.ReadSingle());
        }

        public static void Write(this Stream stream, Vector2 vector)
        {
            stream.Write(vector.x);
            stream.Write(vector.y);
        }

        public static Vector3 ReadVector3(this Stream stream)
        {
            return new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }

        public static void Write(this Stream stream, Vector3 vector)
        {
            stream.Write(vector.x);
            stream.Write(vector.y);
            stream.Write(vector.z);
        }

        public static Vector4 ReadVector4(this Stream stream)
        {
            return new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }

        public static void Write(this Stream stream, Vector4 vector)
        {
            stream.Write(vector.x);
            stream.Write(vector.y);
            stream.Write(vector.z);
            stream.Write(vector.w);
        }

        public static Quaternion ReadQuaternion(this Stream stream)
        {
            return new Quaternion(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }

        public static void Write(this Stream stream, Quaternion quaternion)
        {
            stream.Write(quaternion.x);
            stream.Write(quaternion.y);
            stream.Write(quaternion.z);
            stream.Write(quaternion.w);
        }
    }
}