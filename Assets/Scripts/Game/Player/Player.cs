using UnityEngine;

[RequireComponent(typeof(PlayerControlManager)), /*RequireComponent(typeof(PlayerAnimationController))*/]
public class Player : MonoBehaviour 
{
	PlayerControlManager _playerControlManager = null;
	public PlayerControlManager ControlManager { get { return _playerControlManager; } }

    PlayerAnimationController _playerAnimationController = null;
    public PlayerAnimationController AnimationController { get { return _playerAnimationController; } }

    SingController _playerSingManager = null;
    public SingController PlayerSingController { get { return _playerSingManager; } }


    // Use this for initialization
    private void Awake() 
	{
        _playerControlManager = this.GetComponent<PlayerControlManager>();
        _playerAnimationController = this.GetComponent<PlayerAnimationController>();
        _playerSingManager = this.GetComponent<SingController>();
    }

	public void Initialize()
	{
		if ( _playerControlManager != null )
		{
            _playerControlManager.Initialize();
        }

	    
        if ( _playerAnimationController != null )
        {
            _playerAnimationController.Initialize();
        }
        
	}

}
