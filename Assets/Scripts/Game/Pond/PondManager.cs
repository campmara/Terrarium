using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PondManager : SingletonBehaviour<PondManager>
{
    [SerializeField] private PondTech _pond = null;
    public PondTech Pond
    {
        get { return _pond; }
    }

    private const float POP_HEIGHT = 5f;
    private const float POP_DURATION = 1.5f;

    // Use this for initialization
    private void Awake()
    {
        if (_pond == null)
        {
            // Should handle instantiating this if null ?
            Debug.LogError("No Pond Prefab Referenced");
        }
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void PopPlayerFromPond()
    {
        StartCoroutine(PopPlayerRoutine());
    }

    public void ReturnPlayerToPond()
    {
        // Transport player to pond pop point.
        // Tell the Camera to pan back to the pond.
        PlayerManager.instance.Player.transform.position = Pond.transform.position + (Vector3.down * 3f);
        PlayerManager.instance.Player.transform.rotation = Quaternion.identity;
        CameraManager.instance.ChangeCameraState(CameraManager.CameraState.POND_RETURNPAN);
    }

    private IEnumerator PopPlayerRoutine()
    {
        Vector3 endPos = Pond.transform.forward * 5f;
        Tween jumpTween = PlayerManager.instance.Player.transform.DOJump(endPos, POP_HEIGHT, 1, POP_DURATION);

        yield return jumpTween.WaitForCompletion();

        PlayerManager.instance.Player.GetComponent<RollerController>().ChangeState(P_ControlState.RITUAL, P_ControlState.WALKING);
        CameraManager.instance.ChangeCameraState(CameraManager.CameraState.FOLLOWPLAYER_FREE);
    }

    IEnumerator DelayedReturnPlayer()
    {
        yield return 0;
    }
}
