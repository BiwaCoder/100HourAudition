using System;
namespace HundredHour.RealtimeVoice
{
    // Audio-thread safe rolling window. FFT work happens only on the UI thread.
    public sealed class VoiceSignalBuffer
    {
        const int Size = 1024;
        readonly object gate = new object();
        readonly float[] ring = new float[Size], real = new float[Size], imaginary = new float[Size];
        int cursor, sampleRate = 24000;
        public void Clear() { lock (gate) { Array.Clear(ring, 0, Size); cursor = 0; } }
        public void WritePcm16(byte[] bytes)
        {
            lock (gate)
            {
                sampleRate = 24000;
                for (int i = 0; i + 1 < bytes.Length; i += 2)
                    ring[cursor++ & (Size - 1)] = (short)(bytes[i] | bytes[i + 1] << 8) / 32768f;
            }
        }
        public void WriteOutput(float[] samples, int channels, int rate)
        {
            lock (gate)
            {
                sampleRate = rate;
                for (int i = 0; i < samples.Length; i += channels)
                    ring[cursor++ & (Size - 1)] = samples[i];
            }
        }
        // One main-thread reader per buffer. No frame allocations; normalized log-frequency bands.
        public float ReadBands(float[] bands)
        {
            int rate; double energy = 0;
            lock (gate)
            {
                rate = sampleRate;
                for (int i = 0; i < Size; i++)
                {
                    float sample = ring[(cursor + i) & (Size - 1)];
                    energy += sample * sample;
                    real[i] = sample * (.5f - .5f * (float)Math.Cos(2 * Math.PI * i / (Size - 1)));
                    imaginary[i] = 0;
                }
            }
            for (int i = 1, j = 0; i < Size; i++)
            {
                int bit = Size >> 1;
                for (; (j & bit) != 0; bit >>= 1) j ^= bit;
                j ^= bit;
                if (i < j) { float v = real[i]; real[i] = real[j]; real[j] = v; }
            }
            for (int length = 2; length <= Size; length <<= 1)
            {
                double angle = -2 * Math.PI / length;
                float stepR = (float)Math.Cos(angle), stepI = (float)Math.Sin(angle);
                for (int start = 0; start < Size; start += length)
                {
                    float wr = 1, wi = 0;
                    for (int j = 0; j < length / 2; j++)
                    {
                        int a = start + j, b = a + length / 2;
                        float vr = real[b] * wr - imaginary[b] * wi, vi = real[b] * wi + imaginary[b] * wr;
                        real[b] = real[a] - vr; imaginary[b] = imaginary[a] - vi;
                        real[a] += vr; imaginary[a] += vi;
                        float next = wr * stepR - wi * stepI; wi = wr * stepI + wi * stepR; wr = next;
                    }
                }
            }
            for (int band = 0; band < bands.Length; band++)
            {
                int lo = Math.Max(1, (int)(80 * Math.Pow(75, (double)band / bands.Length) * Size / rate));
                int hi = Math.Min(Size / 2 - 1, Math.Max(lo, (int)(80 * Math.Pow(75, (double)(band + 1) / bands.Length) * Size / rate)));
                double peak = 0;
                for (int bin = lo; bin <= hi; bin++) peak = Math.Max(peak, Math.Sqrt(real[bin] * real[bin] + imaginary[bin] * imaginary[bin]) * 4 / Size);
                bands[band] = (float)Math.Min(1, Math.Log10(1 + peak * 25) / Math.Log10(26));
            }
            return (float)Math.Sqrt(energy / Size);
        }
    }
}
