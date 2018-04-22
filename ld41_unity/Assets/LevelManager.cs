using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelManager : MonoBehaviour {

    public GameObject blockPrefab;
    public GameObject playerPrefab;
    public GameObject player2dPrefab;

    public Vector3 cameraRotation = new Vector3(0, 45, 0);
    public Vector3 cameraPosition = new Vector3(0, 10, -20);
    public float cameraRotationSpeed = 2.0f;

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

    // --- constants

    // in 2d mode, distance between camera and player.
    // assumes far clipping plane is much further than 500 units away.
    private const float k_cameraDistance = 500.0f;

    // --- globals, prefixed with "g_"

    GameObject     g_player;
    GameObject[,,] g_objects;

    // 2d state
    GameObject[,]  g_board;
    Vector2        g_2dpos;
    GameObject     g_target;
    Vector3        g_from;
    GameObject     g_2dplayer;

    void Start() {
        g_objects = load(data0);

        if (Debug.isDebugBuild) {
            test(g_objects);
        }

        g_player = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        g_2dplayer = Instantiate(player2dPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    void Update() {
        if (g_target == null) {
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
                // no 3d backwalk for now
            }
            if (Input.GetButtonDown("Interact")) {

                Vector3 from = g_player.transform.rotation * Vector3.forward;
                GameObject target = project(g_objects, g_player.transform.position, from);

                if (target != null) {
                    g_from = from;
                    g_target = target;
                    g_board = projection(g_objects, from);
                    if (Debug.isDebugBuild) {
                        Debug.Log(board2string(g_board));
                    }
                    g_2dpos = boardPos(g_objects, target.transform.position, from);
                    g_2dplayer.transform.position = g_target.transform.position - g_from;
                    // make 3d player invisible
                    g_player.SetActive(false);
                    g_2dplayer.SetActive(true);
                }
            }
        } else { // 2d mode
            if (Input.GetButtonDown("Left") || Input.GetButtonDown("Right")
             || Input.GetButtonDown("Up") || Input.GetButtonDown("Down")) {
                Vector2 new2dpos = g_2dpos;
                if (Input.GetButtonDown("Left")) {
                    if (g_from == Vector3.back || g_from == Vector3.right) {
                        new2dpos += Vector2.right;
                    } else {
                        new2dpos += Vector2.left;
                    }
                }
                if (Input.GetButtonDown("Right")) {
                    if (g_from == Vector3.back || g_from == Vector3.right) {
                        new2dpos += Vector2.left;
                    } else {
                        new2dpos += Vector2.right;
                    }
                }
                if (Input.GetButtonDown("Up")) {
                    new2dpos += Vector2.up;
                }
                if (Input.GetButtonDown("Down")) {
                    new2dpos += Vector2.down;
                }
                GameObject target = get2d(g_board, new2dpos);

                if (target != null) {
                    g_target = target;
                    g_2dpos = new2dpos;
                    g_2dplayer.transform.position = g_target.transform.position - g_from;
                    Debug.Log(g_2dpos);
                }
            }
            if (Input.GetButtonDown("Interact")) {
                Vector3 posOut = g_target.transform.position - g_from;
                GameObject objBelow = project(g_objects, posOut, Vector3.down);
                if (objBelow != null) {
                    g_player.transform.position = objBelow.transform.position + Vector3.up;
                    g_target = null;
                    // make 3d player visible
                    g_player.SetActive(true);
                    g_2dplayer.SetActive(false);
                } else {
                    // TODO show 3d perspective and indicate failure state
                    Debug.Log("nothing below, failure state");
                }
            }
        }


        if (g_target == null) {

            cameraRotation +=
                cameraRotationSpeed * -1 * Input.GetAxis("Rotate Camera") * Vector3.up;


            Camera.main.transform.position =
                g_player.transform.position +
                (Quaternion.Euler(cameraRotation) * cameraPosition)
                ;
            Camera.main.transform.LookAt(g_player.transform);
        } else { // 2d mode
            Camera.main.transform.position =
                g_2dplayer.transform.position -
                (k_cameraDistance * g_from);
            Camera.main.transform.LookAt(g_2dplayer.transform);
        }
    }

    string board2string(GameObject[,] board) {
        string result = "";
        result += board.GetLength(0) + "," + board.GetLength(1) + "\n";
        for (int j = board.GetLength(1) - 1; j >= 0 ; j--) {
            string line = "";
            for (int i = 0; i < board.GetLength(0); i++) {
                if (board[i,j] != null) {
                    line += "1";
                } else {
                    line += "0";
                }
            }
            result += line + "\n";
        }
        return result;
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

    GameObject get2d(GameObject[,] board, Vector2 pos) {
        if (!(0 <= pos.x && pos.x < board.GetLength(0)
           && 0 <= pos.y && pos.y < board.GetLength(1))) {
            return null;
        }
        return board[round(pos.x), round(pos.y)];
    }
    GameObject get(GameObject[,,] objects, Vector3 pos) {
        if (!inBounds(pos, objects)) {
            return null;
        }
        return objects[round(pos.y), round(pos.z), round(pos.x)];
    }

    // return the 2d coordinates of a 3d position in projected space.
    Vector2 boardPos(GameObject[,,] objects, Vector3 pos, Vector3 dir) {
        Vector2 result = new Vector2(-1, -1);

        //int ly = objects.GetLength(0);
        int lz = objects.GetLength(1);
        int lx = objects.GetLength(2);

        if (dir == Vector3.forward) {
            return new Vector2(pos.x, pos.y);
        }
        if (dir == Vector3.back) {
            return new Vector2(lx - 1 - pos.x, pos.y);
        }
        if (dir == Vector3.left) {
            return new Vector2(pos.z, pos.y);
        }
        if (dir == Vector3.right) {
            return new Vector2(lx - 1 - pos.z, pos.y);
        }

        Debug.Log("failed to get board coordinates");
        return result;
    }

    // return a 2d projection of the level at a given direction.
    // TODO everything here is horribly untested.
    GameObject[,] projection(GameObject[,,] objects, Vector3 dir) {
        GameObject[,] result = null;

        int ly = objects.GetLength(0);
        int lz = objects.GetLength(1);
        int lx = objects.GetLength(2);

        if (dir == Vector3.forward) {
            result = new GameObject[lx, ly];
            for (int y = 0; y < ly; y++) {
                for (int x = 0; x < lx; x++) {
                    result[x,y] = project(objects, new Vector3(x, y, 0), dir);
                }
            }
            return result;
        }
        if (dir == Vector3.back) {
            result = new GameObject[lx, ly];
            for (int y = 0; y < ly; y++) {
                for (int x = 0; x < lx; x++) {
                    result[lx-1-x,y] = project(objects, new Vector3(lx-1-x, y, lz-1), dir);
                }
            }
            return result;
        }
        if (dir == Vector3.left) {
            Debug.Log("left projection");
            result = new GameObject[lz, ly];
            for (int y = 0; y < ly; y++) {
                for (int z = 0; z < lz; z++) {
                    result[z,y] = project(objects, new Vector3(lx-1, y, z), dir);
                }
            }
            return result;
        }
        if (dir == Vector3.right) {
            result = new GameObject[lz, ly];
            for (int y = 0; y < ly; y++) {
                for (int z = 0; z < lz; z++) {
                    result[lz-1-z,y] = project(objects, new Vector3(0, y, lz-1-z), dir);
                }
            }
            return result;
        }

        Debug.LogWarning("projection failed");
        return result;
    }

    GameObject project(GameObject[,,] objects, Vector3 pos, Vector3 dir) {

        Debug.Log(pos);

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
