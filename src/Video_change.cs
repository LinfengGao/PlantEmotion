using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Video_change : MonoBehaviour
{
    double video_time, currentTime;
    //�����video_img����������RawImage�ģ����ؽű���RawImage���뼴��
    public GameObject video_img;
    void Start()
    {
        video_time = video_img.GetComponent<VideoPlayer>().clip.length;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= video_time)
        {
            //��Ƶ���Ž������������д��Ƶ���Ž�������¼�
            SceneManager.LoadScene("ԭ����");
        }
    }
}