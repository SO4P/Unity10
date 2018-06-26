using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Complete
{
    public class UI : NetworkBehaviour
    {

        TankHealth hp;

        // Use this for initialization
        void Start()
        {
            hp = this.GetComponent<TankHealth>();
        }

        void OnGUI()
        {
            if (!isLocalPlayer)
                return;
            string str = "current hp:";
            str += hp.getHp();
            GUI.TextField(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 100, 100, 100), str);
        }
    }
}
