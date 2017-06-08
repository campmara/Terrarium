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
        //HandleOutOfBounds();
    }

    void HandleOutOfBounds()
    {
        if( GameManager.Instance.State == GameManager.GameState.MAIN )
        {           
           if ( Mathf.Abs( ( transform.position - _playerTrans.position ).magnitude ) > PLAYER_MAXDISTANCE )
           {
               PlayerManager.instance.Player.GetComponent<RollerController>().HandleOutOfBounds();
           }
        }
    }

    public void HandlePondWait()
    {
        // Transport player to pond pop point.
        // Tell the Camera to pan back to the pond.      
        PlayerManager.instance.PutPlayerInPond();

        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_WAIT );
        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.POND_WAIT );
    }

    public void HandlePondReturn()
    {
        PlayerManager.instance.PutPlayerInPond();

        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_RETURN );
        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.POND_RETURNPAN );       
    }

    public void PopPlayerFromPond()
    {
        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_POP );

        StartCoroutine( PopPlayerRoutine() );
    }

    private IEnumerator PopPlayerRoutine()
    {
        Tween popTween = PlayerManager.instance.Player.transform.DOMoveY(PondTech.POND_MIN_Y, 0.75f).SetEase(Ease.OutBack);
		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 2 );

        yield return popTween.WaitForCompletion();

        yield return 0;

        PlayerManager.instance.Player.GetComponent<Rigidbody>().isKinematic = false;

        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
        GameManager.Instance.ChangeGameState( GameManager.GameState.MAIN );
    }
}
