using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class touming : MonoBehaviour
{
	//����ʱ��
	public float fadeInTime = 2.0f;
	//����ʱ��
	public float fadeOutTime = 2.0f;
	/// <summary>
	/// boolֵΪTrue������Ļ�ɰױ��
	/// boolֵΪFalse������Ļ�ɺڱ��
	/// </summary>
	public Action<bool> blackCompleteAction;
	private Color fadeInColor = new Color(0.01f, 0.01f, 0.01f, 1.0f);
	private Color fadeOutColor = new Color(0.01f, 0.01f, 0.01f, 0.01f);
	private Material fadeMaterial = null;
	private bool isFadingIn = false;
	private bool isFadeOut = false;
	//private bool isFade = false;
	private YieldInstruction fadeInstruction = new WaitForEndOfFrame();
	void Awake()
	{
		
		fadeMaterial = new Material(Shader.Find("Unlit/FadeBlack"));
		//fadein();
	}
	void OnDisable()
	{
		isFadeOut = false;
		isFadingIn = false;
	}
	void OnDestroy()
	{
		if (fadeMaterial != null)
		{
			isFadeOut = false;
			isFadingIn = false;
			Destroy(fadeMaterial);
		}
	}
	private void OnPostRender()
	{
		if (isFadingIn)
		{
			fadeMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(fadeMaterial.color);
			GL.Begin(GL.QUADS);
			GL.Vertex3(0f, 0f, -12f);
			GL.Vertex3(0f, 1f, -12f);
			GL.Vertex3(1f, 1f, -12f);
			GL.Vertex3(1f, 0f, -12f);
			GL.End();
			GL.PopMatrix();
			//Debug.Log("isfadein");
		}
		if (isFadeOut)
		{
			fadeMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(fadeMaterial.color);
			GL.Begin(GL.QUADS);
			GL.Vertex3(0f, 0f, -12f);
			GL.Vertex3(0f, 1f, -12f);
			GL.Vertex3(1f, 1f, -12f);
			GL.Vertex3(1f, 0f, -12f);
			GL.End();
			GL.PopMatrix();
			//Debug.Log("isfadeout");
		}
	}
	IEnumerator FadeIEnumator(bool fadeIn)
	{
		float fadeTime = fadeIn ? fadeInTime : fadeOutTime;
		isFadingIn = fadeIn ? true : false;
		isFadeOut = fadeIn ? false : true;
		float elapsedTime = 0.0f;
		Color color = fadeIn ? fadeInColor : fadeOutColor;
		fadeMaterial.color = color;
		while (elapsedTime < fadeTime)
		{
			yield return new WaitForEndOfFrame();
			elapsedTime += Time.deltaTime;
			float a = fadeIn ? 1 - Mathf.Clamp01(elapsedTime / fadeInTime) : Mathf.Clamp01(elapsedTime / fadeOutTime);
			color.a = a;
			fadeMaterial.color = color;
		}
		//isFadeOut = isFadingIn = false;
		if (blackCompleteAction != null)
		{
			blackCompleteAction.Invoke(fadeIn);
		}
	}
	public void Fade(bool isFadeIn)
	{
		StartCoroutine(FadeIEnumator(isFadeIn));
	}
#if UNITY_EDITOR
	public void fadeout()
    {
		Fade(false);
	}
	public void fadein()
    {
		Fade(true);
    }
	void Update()
	{
		/*if (Input.GetMouseButtonDown(1))
		{
			//Debug.Log("���룺�����ɰ�����");
			Fade(true);
		}
		if (Input.GetMouseButtonDown(0))//������Ϊ��⵽��ǰ�ƶ�������
		{
			//��ʾ����
			//Debug.Log("���������������䰵");
			Fade(false);
		}
		if (Input.GetMouseButtonDown(2))
		{
			isFadingIn = false;
			isFadeOut = false;
		}*/
	}
#endif
}
