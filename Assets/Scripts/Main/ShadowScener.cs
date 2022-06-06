using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowScener : MonoBehaviour
{
    public AnimationClip clip_start, clip_end;
    // Start is called before the first frame update
    public IEnumerator StartShadow(float time)
    {
        GetComponent<Animator>().Play("shadowinganimation_1");
        float waittime = time;
        if (clip_end.length > time) waittime = clip_end.length;
        yield return new WaitForSeconds(waittime);
        GetComponent<Animator>().Play("shadowinganimation_0");
        //yield return new WaitForSeconds(clip_end.length);
        //gameObject.SetActive(false);
    }

    public void StartPart(int num)
    {
        GetComponent<Animator>().Play("shadowinganimation_" + num);
    }
}
