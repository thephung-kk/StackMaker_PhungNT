using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unbrick : MonoBehaviour
{
    [SerializeField] private GameObject fillTileObj;

    private bool hasFill = false;

    public bool CanFill()
    {
        return !hasFill;
    }

    public void PlayerFillBrick()
    {
        hasFill = true;
        fillTileObj.SetActive(true);
    }
    public void ResetUnbrick()
    {
        hasFill = false;
        fillTileObj.SetActive(false);
    }
}
