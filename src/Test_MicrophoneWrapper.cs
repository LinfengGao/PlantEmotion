using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Test_MicrophoneWrapper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private AudioSource mAudioSource;
    private AudioClip mAudioClip;

    void Start()
    {
        MicrophoneWrapper.Instance.Init();
        mAudioSource = GetComponent<AudioSource>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.transform.GetChild(0).GetComponent<Text>().text = "松开停止";
        MicrophoneWrapper.Instance.OnStartRecord();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.transform.GetChild(0).GetComponent<Text>().text = "按下说话录音";
        mAudioClip = MicrophoneWrapper.Instance.OnStopRecord();

        //if (mAudioSource != null && mAudioClip != null)
        //{
        //    mAudioSource.PlayOneShot(mAudioClip);
        //}
        //else
        //{
        //    Debug.Log(" mAudioSource or mAudioClip is null ");
        //}

        Proceed.proceed_and_act();
    }
}