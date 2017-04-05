public class RollerConstants : ScriptableObjectSingleton<RollerConstants>
{
	// INPUT
	public float InputDeadzone = 0.3f;

	// WALK
	public float WalkSpeed = 3f;
	public float CarrySpeed = 3f;
	public float SingWalkSpeed = 2f;
	public float WalkAcceleration = 0.05f;
	public float WalkDeceleration = 4.0f;
    public float WalkTurnDampening = 0.025f;   // Used to dampen velocity when turning really hard
    public float BodyMinMoveSpeed = 2.5f;
	public float IdleSittingTimer = 30f;

	// WALK TURNING
	public float WalkTurnSpeed = 5f;
	public float CarryTurnSpeed = 7f;
    public float WalkTurnAngleMin = 0.5f;
    public float WalkTurnAngleMax = 60.0f;

	// ROLL
	public float RollSpeed = 10f;
	public float RollMaxSpeed = 13f;
	public float ReverseRollSpeed = 6f;
	public float RollAcceleration = 1f;
	public float RollDeceleration = 10f;
	public float RollSphereSpin = 1250f;

	// ROLL TURNING
	public float TurnSpeed = 125f;
	public float ReverseTurnSpeed = 100f;
	public float TurnAcceleration = 15f;
	public float TurnDeceleration = 700f;
    public float TurnMinSpeed = 25.0f;

    // PICKUP
    public float PickupCheckHeight = 0.5f;
	public float PickupCheckRadius = 1.0f;
    public float PickupForwardScalarPart1 = 0.58f;
    public float PickupUpScalarPart1 = 0.5f;
    public float PickupForwardScalarPart2 = 0.08f;
	public float PickupUpScalarPart2 = 2.0f;
	public float PickupTime = 0.75f;    

    // IDLE
    public float IdleMaxMag = 0.01f;
	public float IdleWaitTime = 0.1f;

	// TRANSITIONS
	public float TransitionTime = 1f;
	public float TransitionDeceleration = 20f;

	// RITUAL DANCE
	public float RitualTime = 0.25f;
	public float RitualTurnSpeed = 2000f;
    public float RitualCompleteWait = 1.5f;

	// PLANTING
	public float PlantingTime = 0.75f;
	public float PlantingEndY = 0f;
    public float PlantingEndX = 1.25f;

    // SINGING
    public float SingingReturnTime = 0.6f;
    public float PitchLerpSpeed = 7f;

    public float IKTargetWorldScalar = 35.0f;

    // IK
    public float IKReachCheckRadius = 8.0f;
    public float IKReachWaitMin = 1.5f;
    public float IKReachWaitMax = 10.0f;
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