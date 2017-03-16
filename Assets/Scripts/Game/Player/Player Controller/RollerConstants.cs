using System;
using System.Reflection;
using UnityEngine;

public static class RollerConstants 
{
	// INPUT
	public const float INPUT_DEADZONE = 0.3f;

	// WALK
	public const float WALK_SPEED = 3f;
	public const float CARRY_SPEED = 3f;
	public const float SING_WALK_SPEED = 2f;
	public const float WALK_ACCELERATION = 0.05f;
	public const float WALK_DECELERATION = 4.0f;
    public const float WALK_TURNDAMPENING = 0.025f;   // Used to dampen velocity when turning really hard
    public const float BODY_MINMOVESPEED = 2.5f;

	// WALK TURNING
	public const float WALK_TURN_SPEED = 5f;
	public const float CARRY_TURN_SPEED = 7f;
    public const float WALK_TURNANGLE_MIN = 0.5f;
    public const float WALK_TURNANGLE_MAX = 60.0f;

	// ROLL
	public const float ROLL_SPEED = 10f;
	public const float ROLL_MAX_SPEED = 13f;
	public const float REVERSE_ROLL_SPEED = 6f;
	public const float ROLL_ACCELERATION = 1f;
	public const float ROLL_DECELERATION = 10f;
	public const float ROLL_SPHERE_SPIN = 1250f;

	// ROLL TURNING
	public const float TURN_SPEED = 125f;
	public const float REVERSE_TURN_SPEED = 100f;
	public const float TURN_ACCELERATION = 15f;
	public const float TURN_DECELERATION = 700f;
    public const float TURN_MINSPEED = 25.0f;

    // PICKUP
    public const float PICKUP_CHECKHEIGHT = 0.5f;
	public const float PICKUP_CHECKRADIUS = 1.0f;
    public const float PICKUP_FORWARDSCALAR_PART1 = 0.58f;
    public const float PICKUP_UPSCALAR_PART1 = 0.5f;
    public const float PICKUP_FORWARDSCALAR_PART2 = 0.08f;
	public const float PICKUP_UPSCALAR_PART2 = 2.0f;
	public const float PICKUP_TIME = 0.75f;    

    // IDLE
    public const float IDLE_MAXMAG = 0.01f;
	public const float IDLE_WAITTIME = 0.1f;

	// TRANSITIONS
	public const float TRANSITION_TIME = 1f;
	public const float TRANSITION_DECELERATION = 20f;

	// RITUAL DANCE
	public const float RITUAL_TIME = 4.5f;
	public const float RITUAL_TURN_SPEED = 2000f;
    public const float RITUAL_COMPLETEWAIT = 1.0f;

	// PLANTING
	public const float PLANTING_TIME = 0.75f;
	public const float PLANTING_ENDY = 0f;
    public const float PLANTING_ENDX = 1.25f;

    // SINGING
    public const float SINGING_RETURN_TIME = 0.6f;
    public const float PITCH_LERP_SPEED = 7f;

    public const float IK_TARGETWORLDSCALAR = 35.0f;

    // IK
    public const float IK_REACH_CHECKRADIUS = 8.0f;
    public const float IK_REACH_WAITMIN = 1.5f;
    public const float IK_REACH_WAITMAX = 10.0f;

    
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