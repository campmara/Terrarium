using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientCreature : MonoBehaviour
{
    public enum CreatureType
    {
        NONE = -1,
        BUTTERFLY = 0
    }

    [SerializeField]
    protected CreatureType _creatureType = CreatureType.NONE;
    public CreatureType CType { get { return _creatureType; } set { _creatureType = value; } }

    public virtual void InitializeCreature( Vector3 startPos ) { }

}
