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
        }
    };

    // globals, prefixed with "g_"
    GameObject g_player;
    GameObject[,,] g_objects;

    void Start() {
        g_objects = load(data0);

        test(g_objects);

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
            Vector3 posInFront = g_player.transform.position + (g_player.transform.rotation * Vector3.forward);
            GameObject objInFront = get(g_objects, posInFront);
            Debug.LogFormat("object in front: {0}", objInFront);
            if (objInFront == null) {
                g_player.transform.position += g_player.transform.rotation * Vector3.forward;
            } else {
                GameObject objOverFront = get(g_objects, posInFront + Vector3.up);
                Debug.LogFormat("object over front: {0}", objOverFront);
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
        return 0 < pos.y && pos.y < objects.GetLength(0)
            && 0 < pos.z && pos.z < objects.GetLength(1)
            && 0 < pos.x && pos.x < objects.GetLength(2)
            ;
    }

    GameObject get(GameObject[,,] objects, Vector3 pos) {
        if (!inBounds(pos, objects)) {
            return null;
        }
        var x = (int) pos.x;
        var y = (int) pos.y;
        var z = (int) pos.z;
        if (x != pos.x || y != pos.y || z != pos.z) {
            Debug.LogWarningFormat("get() was called with non-integer Vector3: {0}", pos);
            Debug.LogWarningFormat("coercing: {0}, {1}, {2}", x, y, z);
        }
        return objects[y, z, x];
    }

    GameObject project(GameObject[,,] objects, Vector3 pos, Vector3 dir) {
        for (var curPos = pos; inBounds(curPos, objects); curPos += dir) {
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
        // test project()
        assert(project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) != null);
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(0, 0, 1)) 
            == project(objects, new Vector3(1, 1, 1), new Vector3(0, 0, 1))
        );
        assert(project(objects, new Vector3(1, 1, 2), new Vector3(1, 0, 0)) == null);

        // test ...
        // TODO

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
