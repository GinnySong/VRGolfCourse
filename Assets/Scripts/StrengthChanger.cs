using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StrengthChanger : MonoBehaviour
{
    public int Strength = 20;

    public TextMeshProUGUI StrengthText;

    public SwingCalculator Club;

    public void UpdateStrength(int value) 
    {
        Strength = Strength + value;
        StrengthText.text = string.Format("Current Power: {0}", Strength);
        Club.UpdateSwingMultiplier(Strength);
    }
}
