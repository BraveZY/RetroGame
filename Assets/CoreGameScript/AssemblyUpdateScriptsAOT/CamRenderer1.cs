using GameCoreRuntime;
using UnityEngine;
public class CamRenderer1 : MonoBehaviour
{
    public int Width;
    public int Height;
    //public UITexture image;
    //public TextureAspectMode textureAspectMode;
    //public bool isInit=true;
    //void Awake()
    //{
    //    image = this.GetComponent<UITexture>();
    //    if (image != null)
    //    {
    //        Width = image.width;
    //        Height = image.height;
    //    }
    //    //===Add
    //    if (isInit)
    //    {
    //        if (GameCore.IsInit)
    //        {
    //            Debug.Log("DisplayCameraUI_Screen_NGUI=============");
    //            gameObject.AddComponent<DisplayCameraUI_Screen_NGUI>();
    //        }
    //    }
    //    //===
    //}
    ////===Add
    //// void Update()
    //// {
    ////     switch (textureAspectMode)
    ////     {
    ////         case TextureAspectMode.BaseWidth:
    ////             image.height = (int)((float)image.width * (float)CamCenter.Instance.Height / (float)CamCenter.Instance.Width);
    ////             break;
    ////         case TextureAspectMode.BaseHeight:
    ////             image.width = (int)((float)image.height * (float)CamCenter.Instance.Width / (float)CamCenter.Instance.Height);
    ////             break;
    ////         case TextureAspectMode.FillInside:
    ////             {
    ////                 float textureScale = (float)image.width / (float)image.height;
    ////                 float camScale = (float)CamCenter.Instance.Height / (float)CamCenter.Instance.Width;
    ////                 if (textureScale > camScale)
    ////                     image.width = (int)((float)image.height * (float)CamCenter.Instance.Width / (float)CamCenter.Instance.Height);
    ////                 if (textureScale < camScale)
    ////                     image.height = (int)((float)image.width * (float)CamCenter.Instance.Height / (float)CamCenter.Instance.Width);
    ////             }
    ////             break;
    ////         case TextureAspectMode.FitOutside:
    ////             {
    ////                 float textureScale = (float)image.width / (float)image.height;
    ////                 float camScale = (float)CamCenter.Instance.Height / (float)CamCenter.Instance.Width;
    ////                 if (textureScale > camScale)
    ////                     image.height = (int)((float)image.width * (float)CamCenter.Instance.Height / (float)CamCenter.Instance.Width);
    ////                 if (textureScale < camScale)
    ////                     image.width = (int)((float)image.height * (float)CamCenter.Instance.Width / (float)CamCenter.Instance.Height);
    ////             }
    ////             break;
    ////     }
    ////     Width = image.width;
    ////     Height = image.height;
    ////     //Debug.LogError((CamCenter.Instance.Preview != null) + " " + CamCenter.Instance.Width + "x" + CamCenter.Instance.Height);
    ////     image.mainTexture = CamCenter.Instance.Preview;
    ////     image.flip = CamCenter.Instance.Front ? UIBasicSprite.Flip.Horizontally : UIBasicSprite.Flip.Nothing;
    //// }
    ////===
    public enum TextureAspectMode
    {
        BaseWidth,
        BaseHeight,
        FillInside,
        FitOutside,
        None,
    }

}