using UnityEngine;

public class Main_UI_Hand : MonoBehaviour
{
    public int playerId = 0;
    public Transform head;


    public void Start()
    {

    }
    private void Update()
    {
        if (GameResManager.instance.isSingle)
        {
            if (playerId == 0)
            {
                head.gameObject.SetActive(true);
            }
            else
            {
                head.gameObject.SetActive(false);
            }
        }
        else
        {
            head.gameObject.SetActive(true);
        }
        UpdateHandPos();
        if (!GameResManager.instance.isSingle)
        {
            if (playerId == 0)
            {
                if (head.localPosition.x > -50)
                {
                    head.localPosition = new Vector3(-50, head.localPosition.y, 0);
                }

            }
            else
            {
                if (head.localPosition.x < 50)
                {
                    head.localPosition = new Vector3(50, head.localPosition.y, 0);
                }
            }
        }
    }

    public Vector3 leftOriPos, rightOriPos, leftHandlePos, rightHandlePos, leftUpward, rightUpward;
    bool leftOriTracked, rightOriTracked;
    void UpdateHandPos()
    {
        //Debug.Log("IMIPlayerManager.Instance===" + IMIPlayerManager.Instance);


        if (IMIPlayerManager.Instance == null)
            return;
        if (playerId == 0)
        {
            skeleton iplayerInfos = IMIPlayerManager.Instance.GetMainPlayerInfo2();
            if (iplayerInfos == null)
                return;
            if (iplayerInfos.points == null)
                return;
            leftOriPos = new Vector3(iplayerInfos.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].x,
                iplayerInfos.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].y, 0);
            rightOriPos = new Vector3(iplayerInfos.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].x,
            iplayerInfos.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].y, 0);

        }
        else
        {
            skeleton iplayerInfos2 = IMIPlayerManager.Instance.GetSubPlayerInfo2();
            if (iplayerInfos2 == null)
                return;
            if (iplayerInfos2.points == null)
                return;
            leftOriPos = new Vector3(iplayerInfos2.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].x,
                    iplayerInfos2.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].y, 0);
            rightOriPos = new Vector3(iplayerInfos2.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].x,
            iplayerInfos2.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].y, 0);

        }

        int w = SkeletonCenter.Instance.Width;
        int h = SkeletonCenter.Instance.Height;
        if (w == 0 || h == 0)
        {
            return;
        }

        float x = (leftOriPos.x - 1920 / 2f) / 1920 * 1920;
        float y = (-leftOriPos.y + 1080 / 2f) / 1080 * 1080;
        float rx = (rightOriPos.x - 1920 / 2f) / 1920 * 1920;
        float ry = (-rightOriPos.y + 1080 / 2f) / 1080 * 1080;

        //根据 屏幕尺寸处理一下 手的位置
        leftHandlePos.x = x;
        leftHandlePos.y = -y;
        rightHandlePos.x = rx;
        rightHandlePos.y = -ry;
        rightHandlePos.y = (-ry * 2f - 200);
        if (GameResManager.instance.isSingle == true)
        {
            rightHandlePos.x = rx * 2f;
        }
        else
        {
            if (playerId == 0)
            {
                rightHandlePos.x = rx * 2f+400;
            }
            else
            {
                rightHandlePos.x = rx *2f-400;
            }
        }
            //Debug.Log("==========" + rightOriPos + "===========" + rightHandlePos);
            head.localPosition = new Vector3(Mathf.Lerp(head.localPosition.x, rightHandlePos.x, Time.deltaTime * 10), Mathf.Lerp(head.localPosition.y, rightHandlePos.y, Time.deltaTime * 10), 0);
    }

}





