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
        //获取VideoPlayer和RawImage组件，以及初始化当前视频索引
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
        //没有视频则返回，不播放
        if (videoPlayer.texture == null)
        {
            return;
        }
        //渲染视频到UGUI上
        rawImage.texture = videoPlayer.texture;       
    }

}
