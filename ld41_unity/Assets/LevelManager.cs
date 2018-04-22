using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelManager : MonoBehaviour {

    public GameObject blockPrefab;
    public GameObject playerPrefab;

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
        },
        {
            {0, 0, 0},
            {1, 0, 0},
            {0, 0, 1},
            {0, 0, 0},
            {0, 0, 0},
            {0, 1, 0}
        }
    };

    // globals, prefixed with "g_"
    GameObject g_player;
    GameObject[,,] g_objects;

    void Start() {
        g_objects = load(data0);

        if (Debug.isDebugBuild) {
            test(g_objects);
        }

        g_player = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
    }

    void Update() {
        if (Input.GetButtonDown("Left")) {
            g_player.transform.Rotate(new Vector3(0, -90, 0));
        }
        if (Input.GetButtonDown("Right")) {
            g_player.transform.Rotate(new Vector3(0, 90, 0));
        }
        if (Input.GetButtonDown("Up")) {

            Vector3 posFront = g_player.transform.position + (g_player.transform.rotation * Vector3.forward);
            Vector3 posAbove = g_player.transform.position + Vector3.up;
            Vector3 posAboveFront = posFront + Vector3.up;

            GameObject objFront = get(g_objects, posFront);
            GameObject objAbove = get(g_objects, posAbove);
            GameObject objAboveFront = get(g_objects, posAboveFront);

            if (objFront != null) {
                Debug.Log("detected object in front");
                if (objAbove == null && objAboveFront == null) {
                    g_player.transform.position = posAboveFront;
                }
            } else {
                Debug.Log("nothing in front");
                var objBelow = project(g_objects, posFront, Vector3.down);
                if (objBelow != null) {
                    g_player.transform.position = objBelow.transform.position + Vector3.up;
                }
            }
        }
        if (Input.GetButtonDown("Down")) {
        }
        if (Input.GetButtonDown("Interact")) {
        }
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

    bool inBounds(Vector3 pos, GameObject[,,] objects) {
        return 0 <= round(pos.y) && round(pos.y) < objects.GetLength(0)
            && 0 <= round(pos.z) && round(pos.z) < objects.GetLength(1)
            && 0 <= round(pos.x) && round(pos.x) < objects.GetLength(2)
            ;
    }

    int round(float a) {
        return (int) Mathf.Round(a);
    }

    GameObject get(GameObject[,,] objects, Vector3 pos) {
        if (!inBounds(pos, objects)) {
            return null;
        }
        return objects[round(pos.y), round(pos.z), round(pos.x)];
    }

    GameObject project(GameObject[,,] objects, Vector3 pos, Vector3 dir) {

        int maxSteps = objects.GetLength(0) + objects.GetLength(1) + objects.GetLength(2);

        int count = 0;
        var curPos = pos;

        GameObject curObj = null;
        while (count < maxSteps) {
            curObj = get(objects, curPos);

            if (curObj != null) {
                break;
            }

            count++;
            curPos += dir;
        }

        Debug.LogFormat("projection stepped {0} times", count);
        return curObj;
    }


    // my dumb testing framework
    int testsPassed;
    int testsTotal;
    private void test(GameObject[,,] objects) {
        // test project()
        assert(project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) 
            == project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1))
        );
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(1, 0, 0)) == null);

        // test get()
        assert(get(objects, new Vector3(0, 0, 0)) != null);
        assert(get(objects, new Vector3(0, 1, 0)) == null);
        assert(get(objects, new Vector3(0, 1, 1)) != null);
        assert(get(objects, new Vector3(1, 1, 0)) == null);
        assert(get(objects, new Vector3(1, 0, 0)) != null);
        assert(get(objects, new Vector3(1, 0, 1)) != null);
        assert(get(objects, new Vector3(0, 0, 1)) != null);

        // TODO

        Debug.LogFormat("{0} out of {1} tests passed.", testsPassed, testsTotal);
        if (testsTotal == testsPassed) {
            Debug.Log("All tests passed!");
        }
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
