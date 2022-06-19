using System;
using UnityEngine;
using System.IO;

public class MicrophoneWrapper : MonoSingleton<MicrophoneWrapper>
{
    string TAG = "MicrophoneWrapper: ";
    bool isHaveMic = false;                     //����Ƿ�����˷�
    string currentDeviceName = string.Empty;    //��ǰ¼���豸����
    int recordFrequency = 8000;                 //¼��Ƶ��,����¼������(8000,16000)
    double lastPressTimestamp = 0;              //�ϴΰ���ʱ���
    int recordMaxLength = 10;                   //��ʾ¼�������ʱ��
    int trueLength = 0;                         //ʵ��¼������
    //unity��¼������ָ������,����ʶ���ϴ�ʱ����ϴ��������Ч�ֽ�
    //ͨ��trueLength,��ȡ��Ч¼������,�ϴ�ʱ����е���Ч���ֽ����ݼ���

    //�洢¼����Ƭ��
    [HideInInspector]
    public AudioClip saveAudioClip;

    string filename = "demo.wav";
    string filepath = Application.streamingAssetsPath;

    public void Init()
    {
        //��ȡ��˷��豸���ж��Ƿ�����˷��豸
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

    // ����¼����ť
    public void OnStartRecord()
    {
        StartRecording();
    }

    /// �ſ�¼����ť
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

    /// ��ʼ¼��
    private bool StartRecording(bool isLoop = false) //8000,16000
    {
        //Debug.Log(TAG + "StartRecording   ");
        if (isHaveMic == false || Microphone.IsRecording(currentDeviceName))
        {
            return false;
        }

        //��ʼ¼��
        /*
         * public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency);
         * deviceName   ¼���豸����.
         * loop         ����ﵽ����,�Ƿ������¼
         * lengthSec    ָ��¼���ĳ���.
         * frequency    ��Ƶ������   
         */
        lastPressTimestamp = GetTimestampOfNowWithMillisecond();
        saveAudioClip = Microphone.Start(currentDeviceName, isLoop, recordMaxLength, recordFrequency);
        return true;
    }

    // ¼������,����ʵ�ʵ�¼��ʱ��
    private int EndRecording()
    {
        //Debug.Log(TAG + "EndRecording   ");
        if (isHaveMic == false || !Microphone.IsRecording(currentDeviceName))
        {
            Debug.Log(TAG + "EndRecording  Failed ");
            return 0;
        }
        //����¼��
        Microphone.End(currentDeviceName);

        //����ȡ��,������©¼��ĩβ
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

    // ��ȡ���뼶���ʱ���,���ڼ��㰴��¼��ʱ��
    private double GetTimestampOfNowWithMillisecond()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }
}