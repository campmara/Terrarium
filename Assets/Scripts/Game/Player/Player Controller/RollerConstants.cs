using UnityEngine;

public class RollerConstants : ScriptableObjectSingleton<RollerConstants>
{
    // INPUT
    [Header("Input Variables"), Space( 10 )]
    public float InputDeadzone = 0.3f;

    // WALK
    [Header( "Walk Variables" ), Space( 10 )]
    public float WalkSpeed = 5.5f;
	public float CarrySpeed = 4f;
	public float SingWalkSpeed = 2f;
	public float WalkAcceleration = 0.2f;
	public float WalkDeceleration = 3.0f;
    public float WalkTurnDampening = 0f;   // Used to dampen velocity when turning really hard
    public float BodyMinMoveSpeed = 2.5f;
	public float IdleSittingTimer = 30f;

    // WALK TURNING    
    public float WalkTurnSpeed = 5f;
	public float CarryTurnSpeed = 7f;
    public float WalkTurnAngleMin = 0.5f;
    public float WalkTurnAngleMax = 60.0f;

    // BREATHING VARIABLES
    public float BreathSpeed = 0.35f;
    public float BreathSpherizeScale = 3.0f;
    public AnimationCurve BreathSpherizeCurve;

    // ROLL
    [Header( "Roll Variables" ), Space( 10 )]
    public float RollSpeed = 10f;
	public float RollMaxSpeed = 13f;
	public float ReverseRollSpeed = 6f;
	public float RollAcceleration = 1f;
	public float RollDeceleration = 10f;
	public float RollSphereSpin = 1250f;
    public float RollEnterSpeed = 0.5f;
    public float RollExitSpeed = 0.15f;

    // ROLL TURNING   
    public float TurnSpeed = 125f;
	public float ReverseTurnSpeed = 100f;
	public float TurnAcceleration = 15f;
	public float TurnDeceleration = 700f;
    public float TurnMinSpeed = 25.0f;

    // ROLL PAINTING
    public Vector2 RollPaintSize = new Vector2( 0.5f, 1.0f );

    // ROLL TRANSITION
    public float RollSpherizeScale = 1.0f;
    public float RollSpherizeMaxSize = 1.0f;

    // PICKUP
    [Header( "Pickup Variables" ), Space( 10 )]
    public float PickupCheckHeight = 0.5f;
	public float PickupCheckRadius = 1.0f;
    public float PickupForwardScalarPart1 = 0.58f;
    public float PickupUpScalarPart1 = 0.5f;
    public float PickupForwardScalarPart2 = 0.08f;
	public float PickupUpScalarPart2 = 2.0f;
	public float PickupTime = 0.75f;


	public Vector2 HugWidthRange = new Vector2( 14.0f, 30.0f);
	public float HugLerpSpeed = 10.0f;

    // IDLE
    [Header( "Idle Variables" ), Space( 10 )]
    public float IdleMaxMag = 0.01f;
	public float IdleWaitTime = 0.1f;

    // TRANSITIONS
    [Header( "Transition Variables" ), Space( 10 )]
    public float TransitionTime = 1f;
	public float TransitionDeceleration = 20f;

    // RITUAL DANCE
    [Header( "Ritual Variables" ), Space( 10 )]
    public float RitualTime = 0.25f;
    public AnimationCurve RitualPopCurve;
    public float RitualCompleteWait = 0.25f;
    public float RitualSphereizeScale = 5.0f;
    public float RitualMaxSpherize = 0.1f;
    public float RitualDeflateSpeed = 2.0f;

    // PLANTING
    [Header( "Planting Variables" ), Space( 10 )]
    public float PlantingTime = 0.75f;
	public float PlantingEndY = 0f;
    public float PlantingEndX = 1.25f;
    public float PlantingEffectRadius = 5.0f;

    // SINGING
    [Header( "Singing Variables" ), Space( 10 )]
    public float SingingReturnTime = 0.6f;
    public float PitchLerpSpeed = 7f;

    [Header( "IK Variables" ), Space( 10 )]
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