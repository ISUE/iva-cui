using System;
using System.IO;
using UnityEngine;

public class AudioMemoryStreamHandler
{
    public MemoryStream PrepareAudioStream(AudioClip clip)
    {
        var rawData = new float[clip.samples];
        clip.GetData(rawData, 0);

        var memoryStream = new MemoryStream();

        // Write the header information
        WriteHeader(memoryStream, clip);

        // Write your audio data to memory stream
        ConvertAndWriteRawData(memoryStream, ref rawData);

        // Return the stream with data ready to be read
        return memoryStream;
    }

    private static void WriteHeader(Stream stream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(36 + samples * channels * 2);
        stream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        //UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        stream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        stream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        stream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        stream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        stream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        stream.Write(subChunk2, 0, 4);
    }

    private static void ConvertAndWriteRawData(Stream stream, ref float[] rawData)
    {
        Int16 intData;
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[rawData.Length * 2];
        //bytesData array is twice the size of dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767;

        //to convert float to Int16
        for (int i = 0; i < rawData.Length; i++)
        {
            //check sample float overflow
            if (rawData[i] > 1.0f)
            {
                intData = (short)rescaleFactor;
            }
            else if (rawData[i] < -1.0f)
            {
                intData = (short)-rescaleFactor;
            }
            else
            {
                intData = (short)(rawData[i] * rescaleFactor);
            }
            BitConverter.GetBytes(intData).CopyTo(bytesData, i * 2);
        }

        stream.Write(bytesData, 0, bytesData.Length);
    }
}