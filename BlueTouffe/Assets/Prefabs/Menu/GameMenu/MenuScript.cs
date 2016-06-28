using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    public GameObject _menu;
    public GameObject _BMenu;
    public GameObject _joystick;
    public GameObject _button;
    public GameObject _chara;

    //public bool _isMenuButton;
    //public bool _isResumeButton;
    //public bool _isQuitButton;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MenuButton()
    { 
        _menu.SetActive(true);
        _joystick.SetActive(false);
        _BMenu.SetActive(false);
        _button.SetActive(false);
    }

    public void ResumeButton()
    {
        _menu.SetActive(false);
        _joystick.SetActive(true);
        _BMenu.SetActive(true);
        _button.SetActive(true);
    }

    public void QuitButton()
    {
        Network.Destroy(_chara.GetComponent<NetworkView>().viewID);
        Network.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void SwitchWeaponButton()
    {
        Weapon weapon = _chara.GetComponent<Weapon>();

        if (weapon.weap == 0 || weapon.weap == 1) weapon.weap = weapon.weap + 1;
        else weapon.weap = 0;

        if (weapon.weap == 1 || weapon.weap == 0) weapon.fireRate = 10;
        if (weapon.weap == 2) weapon.fireRate = 20;
    }
}
