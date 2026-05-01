using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.AI
{
    public static class WavEncoder
    {
        public static byte[] FromAudioClip(AudioClip clip, int sampleCount, int sampleOffset = 0)
        {
            if (clip == null) return null;

            int channels = clip.channels;
            int frequency = clip.frequency;
            int totalSamples = Mathf.Clamp(sampleCount, 0, clip.samples - sampleOffset);
            if (totalSamples <= 0) return null;

            float[] samples = new float[totalSamples * channels];
            clip.GetData(samples, sampleOffset);

            int bytesPerSample = 2;
            int dataSize = totalSamples * channels * bytesPerSample;

            using (MemoryStream ms = new MemoryStream(44 + dataSize))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
                bw.Write(36 + dataSize);
                bw.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });
                bw.Write(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
                bw.Write(16);
                bw.Write((short)1);
                bw.Write((short)channels);
                bw.Write(frequency);
                bw.Write(frequency * channels * bytesPerSample);
                bw.Write((short)(channels * bytesPerSample));
                bw.Write((short)(bytesPerSample * 8));
                bw.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
                bw.Write(dataSize);

                for (int i = 0; i < samples.Length; i++)
                {
                    short s = FloatToInt16(samples[i]);
                    bw.Write(s);
                }

                bw.Flush();
                return ms.ToArray();
            }
        }

        private static short FloatToInt16(float v)
        {
            v = Mathf.Clamp(v, -1f, 1f);
            return (short)Mathf.RoundToInt(v * short.MaxValue);
        }
    }
}

