using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_UI_EndRole3D : MonoBehaviour
{
    public GameObject Win;
    public GameObject Loss;
    public GameObject Draw;
    float animationSpeed = 0.7f;

    public void init( int types)
    {
        Win.SetActive(types==0);
        Loss.SetActive(types == 1);
        Draw.SetActive(types == 2);
        PlayAnim(Win);
        PlayAnim(Loss);
        PlayAnim(Draw);
    }

    void PlayAnim(GameObject obj)
    {
        if (obj.activeSelf)
        {
            Animation anim = obj.GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play();
                foreach (AnimationState state in anim)
                {
                    state.speed = animationSpeed;
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
