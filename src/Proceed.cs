using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Proceed
{
    public static void proceed_and_act()
    {
        string audio_file = Directory.GetCurrentDirectory() + @"\Assets\StreamingAssets\demo.wav";
        string dest_file = Directory.GetCurrentDirectory() + @"\Assets\StreamingAssets\result.txt";
        recognize(audio_file,dest_file);

        try
        {
            StreamReader reader = new StreamReader(dest_file);
            string text = reader.ReadLine();
            UnityEngine.Debug.Log("文字识别内容：" + text);

            JToken result = judge_emotion(text);
            if (result == null)
            {
                UnityEngine.Debug.Log("Fail to judge emotion.");
                return;
            }

            string label = result["label"].ToString();
            JToken subitems = result["subitems"].First;

            UnityEngine.Debug.Log("识别情感标签：" + label);

            act(label, subitems);
        }
        catch(FileNotFoundException e)
        {
            UnityEngine.Debug.Log("文件result.txt无法打开");
            UnityEngine.Debug.Log(e.Message);
        }        
    }

    //根据语音文件audio_file(demo.wav)识别提取文字，保存到文本文件dest_file(result.txt)
    private static void recognize(string audio_file,string dest_file)
    {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = Directory.GetCurrentDirectory() + @"\Assets\baiduai\python.exe";
            process.StartInfo.Arguments = Directory.GetCurrentDirectory() + @"\Assets\StreamingAssets\asr.py";
            //UnityEngine.Debug.Log(Directory.GetCurrentDirectory());//D:\unityproject\Plant-emtion
            //process.StartInfo.Arguments = @"D:\unityproject\Plant-emtion\Assets\StreamingAssets\asr.py";
            process.StartInfo.Arguments += " " + audio_file + " " + dest_file;
            process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            process.StartInfo.CreateNoWindow = true;//是否在新窗口中启动该进程的值(不显示程序窗口)
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += new DataReceivedEventHandler(GetData);
            process.WaitForExit();
            process.Close();
        }catch(Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
        
    }

    //根据文本（字符串）进行情绪识别，text为对话文字内容
    private static JToken judge_emotion(string text)
    {
        // 设置API_KEY/SECRET_KEY
        string API_KEY = "3GebzZfHCI8r9nOuI7hNCzbe";
        string SECRET_KEY = "MMQezeG4enBedZuuNas5bL6N3Zk9Z6nG";

        var client = new Baidu.Aip.Nlp.Nlp(API_KEY, SECRET_KEY);
        client.Timeout = 60000;  // 修改超时时间

        // 带参数调用对话情绪识别接口
        var options = new Dictionary<string, object>
            {
                {"scene", "talk"}
            };
        JObject result = client.Emotion(text, options);
        if (result.Count == 2)
        {
            UnityEngine.Debug.Log(result);
            return null;
        }
        return result.First.First.First;
    }

    //根据情绪label执行相应动画
    private static void act(string label,JToken subitems)
    {
        //UnityEngine.Debug.Log("1");
        if (label == "optimistic")
        {
            // 积极
            if (subitems["label"] == null)
            {
                // 未识别出子情绪
                SceneManager.LoadScene("喜悦");
            }
            else
            {
                string sub_label = subitems["label"].ToString();
                if (sub_label == "like")
                {
                    // 喜悦
                    SceneManager.LoadScene("喜悦");
                }
                else if (sub_label == "happy")
                {
                    // 愉快
                    SceneManager.LoadScene("愉快");
                }
            }
        }
        else if (label == "neutral")
        {
            // 中性
            SceneManager.LoadScene("中性");
        }
        else if (label == "pessimistic")
        {
            // 消极
            if (subitems["label"] == null)
            {
                // 未识别出子情绪
                SceneManager.LoadScene("愤怒");
            }
            else
            {
                string sub_label = subitems["label"].ToString();
                if (sub_label == "angry")
                {
                    // 愤怒
                    SceneManager.LoadScene("愤怒");
                }
                else if (sub_label == "disgusting")
                {
                    // 厌恶
                    SceneManager.LoadScene("悲伤抱怨");
                }
                else if (sub_label == "fearful")
                {
                    // 恐惧
                    SceneManager.LoadScene("恐惧");
                }
                else if (sub_label == "sad")
                {
                    // 悲伤
                    SceneManager.LoadScene("悲伤抱怨");
                }
            }
        }
    }

    private static void GetData(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.Log(e.Data);//接受cmd传来的数据
        }
    }
}
