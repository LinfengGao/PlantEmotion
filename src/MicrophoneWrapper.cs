using System;
using UnityEngine;
using System.IO;

public class MicrophoneWrapper : MonoSingleton<MicrophoneWrapper>
{
    string TAG = "MicrophoneWrapper: ";
    bool isHaveMic = false;                     //标记是否有麦克风
    string currentDeviceName = string.Empty;    //当前录音设备名称
    int recordFrequency = 8000;                 //录音频率,控制录音质量(8000,16000)
    double lastPressTimestamp = 0;              //上次按下时间戳
    int recordMaxLength = 10;                   //表示录音的最大时长
    int trueLength = 0;                         //实际录音长度
    //unity的录音需先指定长度,导致识别上传时候会上传多余的无效字节
    //通过trueLength,获取有效录音长度,上传时候剪切掉无效的字节数据即可

    //存储录音的片段
    [HideInInspector]
    public AudioClip saveAudioClip;

    string filename = "demo.wav";
    string filepath = Application.streamingAssetsPath;

    public void Init()
    {
        //获取麦克风设备，判断是否有麦克风设备
        if (Microphone.devices.Length > 0)
        {
            isHaveMic = true;
            currentDeviceName = Microphone.devices[0];
        }
        else
        {
            Debug.Log(TAG + " Microphone.devices is null(0) ");
        }
    }

    // 按下录音按钮
    public void OnStartRecord()
    {
        StartRecording();
    }

    /// 放开录音按钮
    public AudioClip OnStopRecord()
    {
        trueLength = EndRecording();
        if (trueLength > 1)
        {
            //Debug.Log(TAG + " return AudioClip data ");
            SaveMusic(filename);
            return saveAudioClip;
        }
        
        return null;
    }

    /// 开始录音
    private bool StartRecording(bool isLoop = false) //8000,16000
    {
        //Debug.Log(TAG + "StartRecording   ");
        if (isHaveMic == false || Microphone.IsRecording(currentDeviceName))
        {
            return false;
        }

        //开始录音
        /*
         * public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency);
         * deviceName   录音设备名称.
         * loop         如果达到长度,是否继续记录
         * lengthSec    指定录音的长度.
         * frequency    音频采样率   
         */
        lastPressTimestamp = GetTimestampOfNowWithMillisecond();
        saveAudioClip = Microphone.Start(currentDeviceName, isLoop, recordMaxLength, recordFrequency);
        return true;
    }

    // 录音结束,返回实际的录音时长
    private int EndRecording()
    {
        //Debug.Log(TAG + "EndRecording   ");
        if (isHaveMic == false || !Microphone.IsRecording(currentDeviceName))
        {
            Debug.Log(TAG + "EndRecording  Failed ");
            return 0;
        }
        //结束录音
        Microphone.End(currentDeviceName);

        //向上取整,避免遗漏录音末尾
        return Mathf.CeilToInt((float)(GetTimestampOfNowWithMillisecond() - lastPressTimestamp) / 1000f);
    }

    public void SaveMusic(string filename)
    {
        Save(filename, saveAudioClip);
    }

    public bool Save(string filename, AudioClip clip)
    {
        if (!filename.EndsWith(".wav"))
        {
            Debug.Log(filename+" doesn't end with '.wav'");
            filename = ".wav";
        }

        //Debug.Log("The filename is :" + filename);
        //Debug.Log("The filepath is :"+filepath);

        //Make sure directory exists if user is saving to sub dir.  
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        string file = filepath + "\\" + filename;
        //Debug.Log("The file is :" + file);

        using (FileStream fileStream = CreateEmpty(file))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }
        return true;
    }

    static FileStream CreateEmpty(string file)
    {
        FileStream fileStream = new FileStream(file, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 50; i++) //preparing the header  
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {

        float[] samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]  

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of  
        //dataSource array because a float converted in Int16 is 2 bytes.  

        int rescaleFactor = 32767; //to convert float to Int16  

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {

        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2  
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        //fileStream.Close();  
    }

    // 获取毫秒级别的时间戳,用于计算按下录音时长
    private double GetTimestampOfNowWithMillisecond()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }
}