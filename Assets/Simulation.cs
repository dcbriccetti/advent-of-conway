using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulation : MonoBehaviour {
    public Transform cubePrefab;
    private const int Dim = 16;
    private Transform[,,] space;

    private void Start() {
        space = BuildSpace();
        StartCoroutine(nameof(Generator));
    }

    private IEnumerator Generator() {
        for (int i = 1; i <= 6; i++) {
            yield return new WaitForSeconds(4);
            var numActive = GenerateNext();
            print($"Cycle {i} has {numActive}");
        }
    }

    private Transform[,,] BuildSpace() {
        var newSpace = new Transform[Dim, Dim, Dim];
        string[] lines = Resources.Load<TextAsset>("starting-config").text.Split('\n');
        var startingConfigSize = new Vector2Int(lines[0].Length, lines.Length);
        var halfDims = new Vector2Int(Dim, Dim) / 2;
        Vector2Int centerOffset = halfDims - startingConfigSize / 2;

        ForAllCells(0, Dim-1, (x, y, z) => {
            var alive = false;
            var cube = Instantiate(cubePrefab, new Vector3(x, Dim - y, z), Quaternion.identity);
            if (z == Dim / 2) {
                var inputIndex = new Vector2Int(x, y) - centerOffset;

                if (inputIndex.x >= 0 && inputIndex.x < startingConfigSize.x &&
                    inputIndex.y >= 0 && inputIndex.y < startingConfigSize.y &&
                    lines[inputIndex.y][inputIndex.x] == '#')
                    alive = true;
            }

            if (! alive) cube.gameObject.SetActive(false);
            newSpace[x, y, z] = cube;
        });

        return newSpace;
    }

    private int GenerateNext() {
        var newActives = new bool[Dim, Dim, Dim];
        var numNewActives = 0;
        ForAllCells(0, Dim-1, (x, y, z) => {
            var nn = NumNeighbors(new Vector3Int(x, y, z));
            var active = space[x, y, z].gameObject.activeSelf;
            var newActive = active ? nn == 2 || nn == 3 : nn == 3;
            if (active != newActive || nn == 2 || nn == 3)
                Debug.Log($"x: {x}, y: {y}, z: {z}, nn: {nn}, {active}->{newActive}");
            if (newActive) ++numNewActives;
            newActives[x, y, z] = newActive;
        });
        ForAllCells(0, Dim-1, (x, y, z) => space[x, y, z].gameObject.SetActive(newActives[x, y, z]));

        return numNewActives;
    }
    
    private static void ForAllCells(int start, int end, Action<int, int, int> action) {
        for (int x = start; x <= end; x++) {
            for (int y = start; y <= end; y++) {
                for (int z = start; z <= end; z++) {
                    action(x, y, z);
                }
            }
        }
    }

    private int NumNeighbors(Vector3Int point) {
        int num = 0;
        ForAllCells(-1, 1, (dx, dy, dz) => {
            int x = point.x + dx;
            int y = point.y + dy;
            int z = point.z + dz;
            if (!InBounds(new[] {x, y, z}) || new Vector3Int(dx, dy, dz) == Vector3Int.zero) return;
            if (space[x, y, z].gameObject.activeSelf)
                ++num;
        });

        return num;
    }

    private static bool InBounds(IEnumerable<int> coords) {
        var good = from coord in coords where coord >= 0 && coord < Dim select coord;
        return good.Count() == 3;
    }
}
