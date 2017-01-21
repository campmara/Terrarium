using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mossTreeAnimator : MonoBehaviour {

    [SerializeField]
    private Transform b_stem_root;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private GameObject leaf;
    [SerializeField]
    private float animSpeed;

    private int childnum;
    private Transform[] bones;
    


    private void Start()
    {
        bones = b_stem_root.GetComponentsInChildren<Transform>();
        childnum = bones.Length;

        anim.speed = animSpeed;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = (info.length / anim.speed) / childnum;
        print(animTime);
        print(childnum);

        for (int i = 0; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, Random.Range(0, 2));
        }

        StartCoroutine(SpawnLeaves(animTime, 1));
    }

    private IEnumerator SpawnLeaves(float delay, int index)
    {
        //int ringnumber = 5;
        int inverseIndex = childnum - index;
        Transform currentParent = bones[inverseIndex];

        float offset = Random.Range(0, 100);
        int ringnumber = Random.Range(5, 8);

        for (int i = 0; i < ringnumber; i++)
        {
            GameObject l = Instantiate(leaf);
            l.transform.SetParent(currentParent);
            l.transform.position = currentParent.position;
            l.transform.localScale = currentParent.localScale * inverseIndex*.2f;//(inverseIndex * inverseIndex * .05f);
            l.transform.Rotate(new Vector3(0,i*360/ringnumber + offset, 0));
            l.transform.position -= l.transform.forward * .015f * transform.localScale.x;

            l.GetComponent<Animator>().speed *= anim.GetComponent<Animator>().speed;
        }

        yield return new WaitForSeconds(delay);

        index++;
        if (index < childnum)
        {
            StartCoroutine(SpawnLeaves(delay, index));
        }
    }
}
