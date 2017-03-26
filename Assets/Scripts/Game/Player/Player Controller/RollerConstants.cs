public class RollerConstants : ScriptableObjectSingleton<RollerConstants>
{
	// INPUT
	public float INPUT_DEADZONE = 0.3f;

	// WALK
	public float WALK_SPEED = 3f;
	public float CARRY_SPEED = 3f;
	public float SING_WALK_SPEED = 2f;
	public float WALK_ACCELERATION = 0.05f;
	public float WALK_DECELERATION = 4.0f;
    public float WALK_TURNDAMPENING = 0.025f;   // Used to dampen velocity when turning really hard
    public float BODY_MINMOVESPEED = 2.5f;
	public float IDLE_SITTING_TIMER = 30f;

	// WALK TURNING
	public float WALK_TURN_SPEED = 5f;
	public float CARRY_TURN_SPEED = 7f;
    public float WALK_TURNANGLE_MIN = 0.5f;
    public float WALK_TURNANGLE_MAX = 60.0f;

	// ROLL
	public float ROLL_SPEED = 10f;
	public float ROLL_MAX_SPEED = 13f;
	public float REVERSE_ROLL_SPEED = 6f;
	public float ROLL_ACCELERATION = 1f;
	public float ROLL_DECELERATION = 10f;
	public float ROLL_SPHERE_SPIN = 1250f;

	// ROLL TURNING
	public float TURN_SPEED = 125f;
	public float REVERSE_TURN_SPEED = 100f;
	public float TURN_ACCELERATION = 15f;
	public float TURN_DECELERATION = 700f;
    public float TURN_MINSPEED = 25.0f;

    // PICKUP
    public float PICKUP_CHECKHEIGHT = 0.5f;
	public float PICKUP_CHECKRADIUS = 1.0f;
    public float PICKUP_FORWARDSCALAR_PART1 = 0.58f;
    public float PICKUP_UPSCALAR_PART1 = 0.5f;
    public float PICKUP_FORWARDSCALAR_PART2 = 0.08f;
	public float PICKUP_UPSCALAR_PART2 = 2.0f;
	public float PICKUP_TIME = 0.75f;    

    // IDLE
    public float IDLE_MAXMAG = 0.01f;
	public float IDLE_WAITTIME = 0.1f;

	// TRANSITIONS
	public float TRANSITION_TIME = 1f;
	public float TRANSITION_DECELERATION = 20f;

	// RITUAL DANCE
	public float RITUAL_TIME = 2.0f;
	public float RITUAL_TURN_SPEED = 2000f;
    public float RITUAL_COMPLETEWAIT = 3.0f;

	// PLANTING
	public float PLANTING_TIME = 0.75f;
	public float PLANTING_ENDY = 0f;
    public float PLANTING_ENDX = 1.25f;

    // SINGING
    public float SINGING_RETURN_TIME = 0.6f;
    public float PITCH_LERP_SPEED = 7f;

    public float IK_TARGETWORLDSCALAR = 35.0f;

    // IK
    public float IK_REACH_CHECKRADIUS = 8.0f;
    public float IK_REACH_WAITMIN = 1.5f;
    public float IK_REACH_WAITMAX = 10.0f;

    
}


//public class RollerDebugWindow : EditorWindow
//{
//    SerializedObject settings = null;
//
//    [MenuItem("RollerDebug/DebugWindow")]
//    public static void ShowWindow()
//    {
//        // Get Inspector type, so we can try to autodock beside it.
//        Assembly editorAsm = typeof( Editor ).Assembly;
//        Type inspWndType = editorAsm.GetType( "UnityEditor.InspectorWindow" );
//
//        // Get and show window.
//        RollerDebugWindow window;
//        if (inspWndType != null)
//        {
//            window = EditorWindow.GetWindow<RollerDebugWindow>( inspWndType );
//        }
//        else
//        {
//            window = EditorWindow.GetWindow<RollerDebugWindow>();
//        }
//
//        window.Show();
//    }
//
//    protected void OnInspectorUpdate()
//    {
//        Repaint();
//    }
//
//    protected void OnGUI()
//    {
//        Init();
//    }
//
//    private void Init()
//    {
//        if (settings == null)
//            settings = new SerializedObject( RollerConstants );
//
//    }
//}