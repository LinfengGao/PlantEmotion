using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartVideo : MonoBehaviour
{
    public static  VideoPlayer videoPlayer;
    private RawImage rawImage;

    void Start()
    {
        //��ȡVideoPlayer��RawImage������Լ���ʼ����ǰ��Ƶ����
        videoPlayer = this.GetComponent<VideoPlayer>();
        rawImage = this.GetComponent<RawImage>();
        videoPlayer.loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vPlayer)
    {
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        //û����Ƶ�򷵻أ�������
        if (videoPlayer.texture == null)
        {
            return;
        }
        //��Ⱦ��Ƶ��UGUI��
        rawImage.texture = videoPlayer.texture;       
    }

}
