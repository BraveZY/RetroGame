using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZXing;
using ZXing.Common;

public class Game_UI_RecordVideo : MonoBehaviour
{
    //public UITexture bgImage;
    //public UITexture qrImage;

    public void Show()
    {
        //UIRoot uiRoot = FindObjectOfType<UIRoot>();
        //if (uiRoot != null)
        //    bgImage.height = uiRoot.activeHeight;
        //gameObject.SetActive(true);
        //string url = HttpServer.Instance.baseUrl + "?video=" + Path.GetFileNameWithoutExtension(RecordManager.Instance.pathTrim);
        //qrImage.mainTexture = QRImage(url, 256, 256);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
            this.gameObject.SetActive(false);
    }

    Texture2D QRImage(string content, int width, int height)
    {
        EncodingOptions options = new EncodingOptions
        {
            Width = width,
            Height = height,
            Margin = 1,
        };
        options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
        BarcodeWriter writer = new BarcodeWriter();
        writer.Format = BarcodeFormat.QR_CODE;
        writer.Options = options;
        Color32[] pixels = writer.Write(content);
        for (int i = 0; i < pixels.Length; i++)
        {
            bool isForeground = (pixels[i].r + pixels[i].g + pixels[i].b) < 382;
            pixels[i] = isForeground ? new Color32(137, 75, 64, 255) : Color.white;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    void OnEnable()
    {
        RecordManager.Instance.VideoMute = false;
    }

    void OnDisable()
    {
        RecordManager.Instance.VideoMute = true;
    }
}
