using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PondManager : SingletonBehaviour<PondManager>
{
    [SerializeField] private PondTech _pond;
    public PondTech Pond
    {
        get { return _pond; }
    }

    private const float POP_HEIGHT = 5f;
    private const float POP_DURATION = 1f;

    private Transform _playerTrans;
    private const float PLAYER_MAXDISTANCE = 75.0f;

    // Use this for initialization
    private void Awake()
    {
        if (_pond == null)
        {
            // Should handle instantiating this if null ?
            _pond = FindObjectOfType<PondTech>();
        }
    }

    private void Start()
    {
        _playerTrans = PlayerManager.instance.Player.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        if( GameManager.Instance.State == GameManager.GameState.MAIN )
        {           
           if (Mathf.Abs((transform.position - _playerTrans.position).magnitude ) > PLAYER_MAXDISTANCE)
           {
                ReturnPlayerToPond();
           }
        }
    }

    public void PopPlayerFromPond()
    {
        GameManager.Instance.ChangeGameState(GameManager.GameState.POND_POP);

        StartCoroutine(PopPlayerRoutine());
    }

    public void ReturnPlayerToPond()
    {
        // Transport player to pond pop point.
        // Tell the Camera to pan back to the pond.
        PlayerManager.instance.Player.transform.position = Pond.transform.position + (Vector3.down * 3f);
        PlayerManager.instance.Player.transform.rotation = Quaternion.identity;
        CameraManager.instance.ChangeCameraState(CameraManager.CameraState.POND_RETURNPAN);
        GameManager.Instance.ChangeGameState(GameManager.GameState.POND_RETURN);
    }

    private IEnumerator PopPlayerRoutine()
    {        
        Vector3 endPos = Pond.transform.forward * 5f;
        Tween jumpTween = PlayerManager.instance.Player.transform.DOJump(endPos, POP_HEIGHT, 1, POP_DURATION).SetEase(Ease.Linear);

        yield return jumpTween.WaitForCompletion();

        PlayerManager.instance.Player.GetComponent<RollerController>().ChangeState(P_ControlState.RITUAL, P_ControlState.WALKING);
        CameraManager.instance.ChangeCameraState(CameraManager.CameraState.FOLLOWPLAYER_FREE);
        GameManager.Instance.ChangeGameState( GameManager.GameState.MAIN );
    }
}
