using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUAnimatorInit : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        int row = 10;
        int col = 10;
        for (int i = -row/2; i < row/2; i++)
        {
            for (int j = -col/2; j < col/2; j++)
            {
                GameObject model = Instantiate(prefab);
                model.transform.position = new Vector3(i * 3, 0, j * 3);
            }
        }
    }
}
