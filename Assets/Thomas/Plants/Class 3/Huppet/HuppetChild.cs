using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuppetChild : MonoBehaviour {

    private Animator animator;
    public Animator huppetFruit;
    public float speed = .15f;

    private void Start()
    {
        huppetFruit.enabled = false;
        animator = GetComponent<Animator>();
        SetRandomLayerWeight(animator);
        animator.speed = speed;
        transform.localScale *= Random.Range(2f, 3f);
    }

    private void Update()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !huppetFruit.enabled)
        {
            huppetFruit.enabled = true;
        }
    }

    void SetRandomLayerWeight(Animator anim)
    {
        for (int i = 0; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, Random.Range(.05f, .25f));
        }
    }
}
