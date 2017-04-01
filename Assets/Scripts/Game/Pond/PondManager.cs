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
    private const float PLAYER_MAXDISTANCE = 32.5f;

    // Use this for initialization
    private void Awake()
    {
        if (_pond == null)
        {
            // Should handle instantiating this if null ?
            _pond = FindObjectOfType<PondTech>();
        }
    }

    public override void Initialize()
    {
        _playerTrans = PlayerManager.instance.Player.transform;

        isInitialized = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if( GameManager.Instance.State == GameManager.GameState.MAIN )
        {           
           if ( Mathf.Abs( ( transform.position - _playerTrans.position ).magnitude ) > PLAYER_MAXDISTANCE )
           {
                HandlePondReturn();
           }
        }
    }

    public void PopPlayerFromPond()
    {
        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_POP );

        StartCoroutine( PopPlayerRoutine() );
    }

    public void HandlePondReturn()
    {
        // Transport player to pond pop point.
        // Tell the Camera to pan back to the pond.      
        PlayerManager.instance.ReturnPlayerToPond(); 

        PlayerManager.instance.Player.ControlManager.SetActiveController<InactiveController>();

        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.POND_RETURNPAN );       
    }

    private IEnumerator PopPlayerRoutine()
    {        
		//AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 2 );

		//PlayerManager.instance.Player.GetComponent<RollerController>().BecomeBall();

        //Vector3 endPos = _pond.transform.forward * 5f;
        //Tween jumpTween = PlayerManager.instance.Player.transform.DOJump( endPos, POP_HEIGHT, 1, POP_DURATION ).SetEase( Ease.Linear );
        //yield return jumpTween.WaitForCompletion();

        Tween popTween = PlayerManager.instance.Player.transform.DOMoveY(PondTech.POND_MIN_Y, 0.75f).SetEase(Ease.OutBack);
        yield return popTween.WaitForCompletion();
        
        PlayerManager.instance.Player.ControlManager.SetActiveController<RollerController>();

        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
        GameManager.Instance.ChangeGameState( GameManager.GameState.MAIN );

        // Zero out velocity.
        PlayerManager.instance.Player.GetComponent<RollerController>().Velocity = 0f;
        
		PlayerManager.instance.Player.GetComponent<RollerController>().BecomeWalker();
        PlayerManager.instance.Player.GetComponent<RollerController>().ChangeState( P_ControlState.WALKING );
    }
}
