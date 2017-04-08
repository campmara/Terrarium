using UnityEngine;

public class FootTrigger : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
        AudioManager.instance.PlayRandomAudioClip( AudioManager.AudioControllerNames.PLAYER_FOOTSTEPS );
        GroundManager.instance.Ground.DrawSplatDecal(transform.position, 0.25f);
	}
}