﻿using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{

    public int fireRate = 0;
    public LayerMask ToHit;
    int timeToFire;
    public int weap = 1; // 1 - snip, 2 - shotgun, 3 - close combat
    Transform shooter;
    public GameObject proj;

    GameObject _joystickGameObj;
    Joystick _Joystick;

    // Use this for initialization1
    void Start()
    {
        timeToFire = 0;

        shooter = transform.FindChild("Shooter");
        if ( shooter == null )
        {
            Debug.LogError("No shooter detected");
        }

        _joystickGameObj = GameObject.Find("Joystick right");
        _Joystick = _joystickGameObj.GetComponent<Joystick>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = _Joystick.JoystickInput.x;
        float y = _Joystick.JoystickInput.y;

        if ( gameObject.GetComponent<Animator>().GetBool("isShooting") )
        {
            timeToFire++;
            if ( x != 0 || y != 0 )
            {
                if ( fireRate == 00 )
                {
                    AdaptInput(x, y);
                }
                else if ( timeToFire % fireRate == 0 || timeToFire == 0 )
                {
                    AdaptInput(x, y);
                }
            }
        }
    }

    void AdaptInput( float x, float y )
    {
        float modifiedX = x;
        float modifiedY = y;

        //Distance and angle parameter
        if ( x != 0 && y != 0 )
        {
            if ( weap != 1 )
            {
                modifiedX = modifiedX / 1.5f;
                modifiedY = modifiedY / 1.5f;
            }
        }

        if ( weap == 2 )
        {
            modifiedX = modifiedX / 1.5f;
            modifiedY = modifiedY / 1.5f;
        }
        else if ( weap == 3 )
        {
            modifiedX = modifiedX / 5;
            modifiedY = modifiedY / 5;
        }

        //send modified parameter
        Shoot(modifiedX, modifiedY);
    }


    void Shoot( float x, float y )
    {
        Vector2 shooterPosition = new Vector2(shooter.position.x, shooter.position.y);
        GameObject proj1 = Network.Instantiate(proj, shooterPosition, shooter.rotation, 1) as GameObject;
        proj1.GetComponent<ProjController>().FireInTheHole(_Joystick.JoystickInput);

        if ( weap == 2 )
        {
            Vector2 target2 = new Vector2(x, y);
            target2 = Quaternion.Euler(0, 0, 10) * target2;
            GameObject proj2 = Network.Instantiate(proj, shooterPosition, shooter.rotation, 1) as GameObject;
            proj2.GetComponent<ProjController>().FireInTheHole(target2);

            Vector2 target3 = new Vector2(x, y);
            target3 = Quaternion.Euler(0, 0, -10) * target3;
            GameObject proj3 = Network.Instantiate(proj, shooterPosition, shooter.rotation, 1) as GameObject;
            proj3.GetComponent<ProjController>().FireInTheHole(target3);
        }
    }
    public int GetWeapon
    {
        get { return weap; }
    }
}