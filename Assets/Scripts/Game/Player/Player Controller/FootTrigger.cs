using UnityEngine;

public class FootTrigger : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
        if( CameraManager.instance.CamState == CameraManager.CameraState.FOLLOWPLAYER_FREE )
        {
            AudioManager.instance.PlayRandomAudioClip( AudioManager.AudioControllerNames.PLAYER_FOOTSTEPS );
            GroundManager.instance.Ground.DrawSplatDecal( transform.position, 0.25f );
        }
	}
}