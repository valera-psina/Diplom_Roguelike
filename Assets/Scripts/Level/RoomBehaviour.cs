using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject[] walls; // 0 - Up 1 -Down 2 - Right 3- Left
    [SerializeField] private GameObject[] doors;
    [SerializeField] private GameObject[] lights;

    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            walls[i].SetActive(!status[i]);
        }
        for (int i = 0; i < walls.Length && i < lights.Length; i++)
        {
            lights[i].SetActive(!walls[i].activeSelf);
            doors[i].SetActive(!walls[i].activeSelf);
        }
    }
}
