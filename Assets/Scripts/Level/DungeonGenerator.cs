using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y)
        {
            bool noLimits = (minPosition == Vector2Int.zero && maxPosition == Vector2Int.zero);

            if (noLimits || (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y))
            {
                return obligatory ? 2 : 1;
            }
            return 0;
        }
    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public GameObject startRoomPrefab;
    public GameObject firstRoomPrefab;
    public GameObject bossRoomPrefab;
    public int minRooms = 10;
    public int maxRooms = 30;
    public Vector2 offset;

    List<Cell> board;

    private int bossCellIndex;
    private int firstRoomAfterStart;

    void Start()
    {
        MazeGenerator();
    }

    void GenerateDungeon()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Cell currentCell = board[i + j * size.x];
                if (currentCell.visited)
                {
                    int currentIndex = i + j * size.x;
                    GameObject roomPrefab = null;

                    if (currentIndex == startPos && startRoomPrefab != null)
                    {
                        roomPrefab = startRoomPrefab;
                    }
                    else if (currentIndex == firstRoomAfterStart && firstRoomPrefab != null)
                    {
                        roomPrefab = firstRoomPrefab;
                    }
                    else if (currentIndex == bossCellIndex && bossRoomPrefab != null)
                    {
                        roomPrefab = bossRoomPrefab;
                    }
                    else
                    {
                        int randomRoom = -1;
                        List<int> availableRooms = new List<int>();

                        for (int k = 0; k < rooms.Length; k++)
                        {
                            int p = rooms[k].ProbabilityOfSpawning(i, j);
                            if (p == 2)
                            {
                                randomRoom = k;
                                break;
                            }
                            else if (p == 1)
                            {
                                availableRooms.Add(k);
                            }
                        }

                        if (randomRoom == -1)
                        {
                            if (availableRooms.Count > 0)
                                randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                            else
                                randomRoom = 0;
                        }

                        roomPrefab = rooms[randomRoom].room;
                    }

                    var newRoom = Instantiate(roomPrefab,
                        new Vector3(i * offset.x, 0, -j * offset.y),
                        Quaternion.identity,
                        transform).GetComponent<RoomBehaviour>();

                    newRoom.UpdateRoom(currentCell.status);
                    newRoom.name += " " + i + "-" + j;

                    if (navMeshSurface != null)
                        navMeshSurface.BuildNavMesh();
                }
            }
        }
    }

    void MazeGenerator()
    {
        bool generationSuccessful = false;
        int attempts = 0;
        const int maxAttempts = 100;

        int firstRoomIndex = -1;
        bool firstStepRecorded = false;

        while (!generationSuccessful && attempts < maxAttempts)
        {
            attempts++;
            board = new List<Cell>();
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    board.Add(new Cell());

            int currentCell = startPos;
            Stack<int> path = new Stack<int>();
            int visitedCount = 0;

            int k = 0;
            while (k < 1000)
            {
                k++;
                if (!board[currentCell].visited)
                {
                    board[currentCell].visited = true;
                    visitedCount++;
                }

                if (visitedCount >= maxRooms)
                {
                    break;
                }

                if (currentCell == board.Count - 1)
                    break;

                List<int> neighbors = CheckNeighbors(currentCell);

                if (neighbors.Count == 0)
                {
                    if (path.Count == 0)
                        break;
                    else
                        currentCell = path.Pop();
                }
                else
                {
                    path.Push(currentCell);
                    int newCell = neighbors[Random.Range(0, neighbors.Count)];

                    if (!firstStepRecorded && currentCell == startPos)
                    {
                        firstRoomIndex = newCell;
                        firstStepRecorded = true;
                    }

                    if (newCell > currentCell)
                    {
                        if (newCell - 1 == currentCell)
                        {
                            board[currentCell].status[2] = true;
                            currentCell = newCell;
                            board[currentCell].status[3] = true;
                        }
                        else
                        {
                            board[currentCell].status[1] = true;
                            currentCell = newCell;
                            board[currentCell].status[0] = true;
                        }
                    }
                    else
                    {
                        if (newCell + 1 == currentCell)
                        {
                            board[currentCell].status[3] = true;
                            currentCell = newCell;
                            board[currentCell].status[2] = true;
                        }
                        else
                        {
                            board[currentCell].status[0] = true;
                            currentCell = newCell;
                            board[currentCell].status[1] = true;
                        }
                    }
                }
            }

            if (visitedCount >= minRooms)
            {
                generationSuccessful = true;
                bossCellIndex = currentCell;
                firstRoomAfterStart = firstRoomIndex;
            }
            else
            {
                Debug.LogWarning($"Сгенерировано только {visitedCount} комнат. Требуется минимум {minRooms}. Попытка {attempts}...");
            }
        }

        if (!generationSuccessful)
        {
            Debug.LogError($"Не удалось сгенерировать подземелье с минимум {minRooms} комнат за {maxAttempts} попыток.");
        }

        GenerateDungeon();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell-size.x)].visited)
        {
            neighbors.Add((cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited)
        {
            neighbors.Add((cell + size.x));
        }

        //check right neighbor
        if ((cell+1) % size.x != 0 && !board[(cell +1)].visited)
        {
            neighbors.Add((cell +1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited)
        {
            neighbors.Add((cell -1));
        }

        return neighbors;
    }

    //private void OnDrawGizmos()
    //{
    //    // Если генерация ещё не выполнялась — выходим
    //    if (board == null || board.Count == 0)
    //        return;

    //    // Размер одной клетки (можно заменить на offset из твоего скрипта)
    //    Vector3 cellSize = new Vector3(offset.x, 0.1f, offset.y);

    //    // Рисуем все клетки сетки
    //    for (int i = 0; i < size.x; i++)
    //    {
    //        for (int j = 0; j < size.y; j++)
    //        {
    //            int index = i + j * size.x;
    //            Cell cell = board[index];
    //            Vector3 center = new Vector3(i * offset.x, 0, -j * offset.y);

    //            // Посещённые клетки — зелёный полупрозрачный куб
    //            if (cell.visited)
    //            {
    //                Gizmos.color = new Color(0, 1, 0, 0.3f);
    //                Gizmos.DrawCube(center, cellSize * 0.9f);
    //            }
    //            // Непосещённые — серый каркас
    //            else
    //            {
    //                Gizmos.color = Color.gray;
    //                Gizmos.DrawWireCube(center, cellSize);
    //            }

    //            // Рисуем проходы (двери) между посещёнными клетками
    //            if (cell.visited)
    //            {
    //                Gizmos.color = Color.yellow;
    //                float halfW = offset.x / 2f;
    //                float halfH = offset.y / 2f;

    //                if (cell.status[0]) // вверх (по массиву)
    //                {
    //                    Vector3 from = center + Vector3.forward * halfH;
    //                    Vector3 to = new Vector3(i * offset.x, 0, -j * offset.y + offset.y);
    //                    Gizmos.DrawLine(from, to);
    //                }
    //                if (cell.status[1]) // вниз
    //                {
    //                    Vector3 from = center - Vector3.forward * halfH;
    //                    Vector3 to = new Vector3(i * offset.x, 0, -j * offset.y - offset.y);
    //                    Gizmos.DrawLine(from, to);
    //                }
    //                if (cell.status[2]) // вправо
    //                {
    //                    Vector3 from = center + Vector3.right * halfW;
    //                    Vector3 to = new Vector3((i + 1) * offset.x, 0, -j * offset.y);
    //                    Gizmos.DrawLine(from, to);
    //                }
    //                if (cell.status[3]) // влево
    //                {
    //                    Vector3 from = center - Vector3.right * halfW;
    //                    Vector3 to = new Vector3((i - 1) * offset.x, 0, -j * offset.y);
    //                    Gizmos.DrawLine(from, to);
    //                }
    //            }
    //        }
    //    }

    //    // Отмечаем стартовую комнату и босса
    //    if (startPos >= 0 && startPos < board.Count)
    //    {
    //        Gizmos.color = Color.cyan;
    //        Vector3 startCenter = new Vector3((startPos % size.x) * offset.x, 0.5f, -(startPos / size.x) * offset.y);
    //        Gizmos.DrawSphere(startCenter, 0.5f);
    //    }
    //}
}
