using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities : ScriptableObject
{
    public new string name;
    public float cdTime;
    public float activeTime;

    public virtual void Activate(GameObject parent) { }
    public virtual void CoolDown(GameObject parent) { }

    public virtual void ResetLuope() { }
}