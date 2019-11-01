using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechManager
{   


    public TechManager()
    {

    }
}

public class Technology
{
    int id;
    bool unlocked;
    bool unlockable;

    public Technology(int idd, bool unl, bool unlb)
    {
        id = idd;
        unlocked = unl;
        unlockable = unlb;
    }

    public Technology()
    {

    }

    class SmallTech
    {
        string attr;
        float value;

        public SmallTech()
        {

        }
    }
}
