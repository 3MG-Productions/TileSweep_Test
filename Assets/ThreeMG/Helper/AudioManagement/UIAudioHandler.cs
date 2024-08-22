using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ThreeMG.Helper.AudioManagement;

public class UIAudioHandler : MonoBehaviour
{
    public const string TAG_BUTTON_TAP = "BUTTON_TAP";

    public void PlayGenericButtonClickAudio()
    {
        AudioManager.Instance.PlayWithTag(TAG_BUTTON_TAP);
    }
}
