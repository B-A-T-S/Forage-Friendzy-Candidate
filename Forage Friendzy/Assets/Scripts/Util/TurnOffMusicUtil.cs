using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffMusicUtil : MonoBehaviour
{
    public void TurnOffMusic()
    {
        GameManager.Instance.TurnOffMenuMusic();
    }
}
