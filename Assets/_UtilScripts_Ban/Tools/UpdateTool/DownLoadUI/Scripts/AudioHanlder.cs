using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHanlder : MonoBehaviour
{

    [SerializeField] AudioSource bgm;
    float[] audioData;
    [SerializeField] Transform effectGo;

    void Start()
    {
        audioData = new float[64];
    }
    float fromX = 630f, toX = 330f;
    float fromY = 80f, toY = 0f;

    void Update()
    {
        bgm.GetSpectrumData(audioData, 0, FFTWindow.Blackman);
        if (audioData[0] > 0.3f)
        {
            effectGo.localPosition = new Vector3(fromX - audioData[0] * (fromX - toX) / 0.7f, fromY - audioData[0] * (fromY - toY) / 0.7f, 0);
        }
        else
            effectGo.localPosition = Vector3.Lerp(effectGo.localPosition, new Vector3(fromX, fromY, 0), Time.deltaTime * 5f);

    }
}
