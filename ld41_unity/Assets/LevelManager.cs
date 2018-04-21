using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelManager : MonoBehaviour {

    public GameObject blockPrefab;

    // MAP LAYOUT
    // layers of y, in order of bottom to top
    // each layer:
    // . x --->
    // z - - -
    // | - - -
    // v - - -
    private static readonly int[,,] data0 = new int[,,]{
        {
            {1, 1, 1},
            {1, 1, 1},
            {1, 1, 1},
            {0, 0, 0},
            {0, 1, 0},
            {0, 0, 0}
        },
        {
            {0, 0, 0},
            {1, 0, 1},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 1, 0}
        }
    };

    void Start() {
        GameObject[,,] objects = load(data0);

        test(objects);
    }

    void Update() {

    }

    GameObject[,,] load(int[,,] data) {
        GameObject[,,] result = new GameObject[data.GetLength(0), data.GetLength(1), data.GetLength(2)];
        for (int y = 0; y < data.GetLength(0); y++) {
            for (int z = 0; z < data.GetLength(1); z++) {
                for (int x = 0; x < data.GetLength(2); x++) {
                    if (data[y,z,x] == 1) {
                        result[y,z,x] = Instantiate(blockPrefab, new Vector3(x, y, z), Quaternion.identity);
                    }
                }
            }
        }
        return result;
    }

    bool inBounds(Vector3 pos, GameObject[,,] data) {
        return 0 < pos.y && pos.y < data.GetLength(0)
            && 0 < pos.z && pos.z < data.GetLength(1)
            && 0 < pos.x && pos.x < data.GetLength(2)
            ;
    }

    GameObject project(GameObject[,,] objects, Vector3 pos, Vector3 dir) {
        for (var curPos = pos + dir; inBounds(curPos, objects); curPos += dir) {
            var curObj = objects[(int)curPos.y,(int)curPos.z,(int)curPos.x];
            if (curObj != null) {
                return curObj;
            }
        }
        return null;
    }

    // my dumb testing framework
    int testsPassed;
    int testsTotal;
    private void test(GameObject[,,] objects) {
        assert(project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) 
            == project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1)));
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(1, 0, 0)) == null);
    }

    private void assert(bool cond) {
        testsTotal++;
        if (cond) {
            testsPassed++;
        } else {
            Debug.LogErrorFormat("test #{0} failed", testsTotal);
        }
    }
}
