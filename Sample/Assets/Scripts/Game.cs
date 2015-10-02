using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject Player;

	void Start ()
    {
        Player = ModManager.Instance.Spawn("Character", Vector3.zero);
	}
}
