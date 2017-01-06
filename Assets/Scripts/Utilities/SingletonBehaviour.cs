// from https://github.com/cjacobwade/YouAreAHoverboard/blob/master/Assets/Scripts/Utilities/SingletonBehaviour.cs
// Thx Chris Wade l o l <3
using UnityEngine;

// Make a subclass of this class with T as the subclass to make a singleton
// Need to make an initialization method in this!
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance = null;

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<T>();

				if (_instance == null)
				{
					GameObject go = new GameObject();
					go.name = typeof(T).Name;
					_instance = go.AddComponent<T>();
				}
			}

			return _instance;
		}
	}

	public static bool DoesExist()
	{
		return _instance != null;
	}

	// Call this to upgrade a singleton to a persistent singleton.
	// This is most often done in Awake().
	// This will kill an instance that tries to be a persistent singleton but isn't the current instance.
	public static void DontDestroyElseKill(SingletonBehaviour<T> mb)
	{
		if (mb == instance)
		{
			MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
		}
		else
		{
			MonoBehaviour.Destroy(mb);
		}
	}

	// Makes this object a persistent singleton unless the singleton already exists in which case
	// the current object is destroyed
	protected void MakeMeAPersistentSingleton()
	{
		DontDestroyElseKill(this);
	}

	protected bool isInitialized = false;
	public bool IsInitialized { get { return isInitialized; } }

	public virtual void Initialize()
	{
		
	}
}