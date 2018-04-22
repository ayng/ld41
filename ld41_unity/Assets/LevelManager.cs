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

            Vector3 posFront = g_player.transform.position + (g_player.transform.rotation * Vector3.forward);
            Vector3 posAbove = posFront + Vector3.up;
            Vector3 posBelow = posFront + Vector3.down;

            GameObject objFront = get(g_objects, posFront);
            GameObject objAbove = get(g_objects, posAbove);
            GameObject objBelow = get(g_objects, posBelow);

            Debug.LogFormat("front: {0} {1}", posFront, objFront != null);
            Debug.LogFormat("above: {0} {1}", posAbove, objAbove != null);
            Debug.LogFormat("below: {0} {1}", posBelow, objBelow != null);

            if (objFront == null && objBelow != null) {
                Debug.Log("forward");
                g_player.transform.position = posFront;
            } else if (objFront != null && objAbove == null) {
                Debug.Log("climb");
                g_player.transform.position = posAbove;
            } else {
                Debug.Log("can't move forward");
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
        return 0 <= pos.y && pos.y < objects.GetLength(0)
            && 0 <= pos.z && pos.z < objects.GetLength(1)
            && 0 <= pos.x && pos.x < objects.GetLength(2)
            ;
    }

    GameObject get(GameObject[,,] objects, Vector3 pos) {
        if (!inBounds(pos, objects)) {
            return null;
        }
        int x = (int) Mathf.Round(pos.x);
        int y = (int) Mathf.Round(pos.y);
        int z = (int) Mathf.Round(pos.z);
        return objects[y, z, x];
    }

    GameObject project(GameObject[,,] objects, Vector3 pos, Vector3 dir) {
        for (var curPos = pos; inBounds(curPos, objects); curPos += dir) {
            var curObj = get(objects, curPos);
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

        // test get()
        assert(get(objects, new Vector3(0, 0, 0)) != null);
        assert(get(objects, new Vector3(0, 1, 0)) == null);
        assert(get(objects, new Vector3(0, 1, 1)) != null);
        assert(get(objects, new Vector3(1, 1, 0)) == null);

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
