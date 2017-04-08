using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class groundPlantAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject leaf;
    [SerializeField]
    private GameObject fruit;
    [SerializeField]
    private int leafNumber;
    [SerializeField]
    private int layerCount;
    [SerializeField]
    private float speed;

    private void Start()
    {
        for (int lin = 0; lin < layerCount; lin++)
        {
            GameObject newLayer = new GameObject();
            newLayer.name = "Layer" + lin;
            newLayer.transform.parent = transform;
            newLayer.transform.position = transform.position;

            for (int i = 0; i < leafNumber; i++)
            {
                float scale = 1 - lin * .2f;
                GameObject newLeaf = Instantiate(leaf);
                newLeaf.transform.position = transform.position + Vector3.up * (lin * .01f);
                newLeaf.transform.parent = transform;
                newLeaf.transform.localScale = new Vector3(scale, scale, scale);
                newLeaf.transform.Rotate(new Vector3(0, i * 360 / leafNumber + i, 0));

                newLeaf.transform.parent = newLayer.transform;

                newLeaf.GetComponent<Animator>().speed *= speed;

                StartCoroutine(WaitAndStart(newLeaf.GetComponent<Animator>(), (.5f + ((lin*leafNumber) + i) * .05f) / speed));
            }
            StartCoroutine(TweenLocalScale(newLayer.transform, Vector3.zero, Vector3.one * (1 - lin * .2f), (5 + lin) * speed));
        }

        GameObject f = Instantiate(fruit);
        f.transform.position = transform.position;
        f.transform.parent = transform;

        StartCoroutine(TweenLocalScale(f.transform, Vector3.zero, f.transform.localScale, 7 * speed));
    }

    private IEnumerator TweenLocalScale(Transform focusTransform, Vector3 startScale, Vector3 endScale, float moveTime)
    {
        float timer = 0.0f;

        while (timer < moveTime)
        {
            focusTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0,1, timer / moveTime));
            timer += Time.deltaTime;
            yield return 0;
        }

        focusTransform.localScale = endScale;
    }

    private IEnumerator WaitAndStart(Animator anim, float waitTime)
    {
        anim.enabled = false;
        yield return new WaitForSeconds(waitTime);
        anim.enabled = true;
        anim.Play(0);

        //yield return 0;
    }

}
