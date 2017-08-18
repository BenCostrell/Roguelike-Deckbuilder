using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputManager
{
    private readonly string[] buttons = { "A", "B", "X", "Y" };

    public void GetInput()
    {
        for (int i = 0; i < 1; i++)
        {
            int playerNum = i + 1;
            foreach (string button in buttons)
                if (Input.GetButtonDown(button + "_P" + playerNum))
                    Services.EventManager.Fire(new ButtonPressed(button, playerNum));
        }
        
        if (Input.GetButtonDown("Reset")) Services.EventManager.Fire(new Reset());
    }
}
