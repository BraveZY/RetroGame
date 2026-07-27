using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopTextures : MonoBehaviour
{
    [SerializeField] List<Image> loopTextureList;
    int curIndex = 0;
    void Start()
    {
        StartCoroutine(LoopTexture());
    }

    IEnumerator LoopTexture()
    {
        int count = loopTextureList.Count;
        yield return new WaitForSeconds(2f);
        while (true)
        {
            DOTween.To(() => { return 1f; }, (value) =>
            {
                loopTextureList[curIndex].color = new Color(1f, 1f, 1f, value);
                loopTextureList[(curIndex + 1) % count].color = new Color(1f, 1f, 1f, 1 - value);
            }, 0f, 1f).Play().onComplete = () => { curIndex = (curIndex + 1) % count; };
            yield return new WaitForSeconds(3f);
        }
    }
}
