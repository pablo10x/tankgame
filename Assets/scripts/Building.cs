using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour {
    protected int HP = 1000;

    public Building(int hp)
    {
        this.HP = hp;
    }
}
