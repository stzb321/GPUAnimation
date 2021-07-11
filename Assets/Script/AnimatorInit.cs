using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorInit : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        int row = 20;
        int col = 20;
        for (int i = -row / 2; i < row / 2; i++)
        {
            for (int j = -col / 2; j < col / 2; j++)
            {
                GameObject model = Instantiate(prefab);
                model.transform.position = new Vector3(i * 3, 0, j * 3);
            }
        }
    }
}
