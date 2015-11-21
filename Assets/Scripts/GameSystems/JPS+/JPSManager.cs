using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class JPSManager : MonoBehaviour {

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;

	static JPSManager instance;
	JPSplus pathfinding;

	bool isProcessingPath = false;


	string fileName = "Assets/Resources/JPValues.txt";
	string JPFile = "Assets/Resources/JPValues.txt";
    string GBFile = "Assets/Resources/GBValues.txt";

    public Vector2 worldSize = new Vector2(10, 10);
	public Vector2 gridSize;

	public int[,][] precompute;

	public int[,][] JPValues;
    public Vector2[,][] GBValues;

    void Awake(){
		
		instance = this;
		pathfinding = GetComponent<JPSplus>();
		
		Texture2D map = (Texture2D) Resources.Load("Levelmap");

		gridSize.x = map.width;
		gridSize.y = map.height;

		precompute = new int[(int)gridSize.x,(int)gridSize.y][];

		//if(File.Exists (fileName)){
		//	var sr = File.OpenText(fileName);

		JPValues = new int[(int)gridSize.x, (int)gridSize.y][];
        GBValues = new Vector2[(int)gridSize.x, (int)gridSize.y][];

        if (File.Exists (JPFile)){
			StreamReader sr = File.OpenText(JPFile);


			string nodeInfo;

			for (int i = 0; i < gridSize.x; i++){
				for (int j = 0; j < gridSize.y; j++){
					nodeInfo = sr.ReadLine ();

					int[] nodeInfoArray = new int[8]; // 8 = each possible direction //nodeInfoArray = nodeInfo.Split(","[0]);
					string[] aux = nodeInfo.Split(',');

					// ReOrder
					nodeInfoArray[0] = int.Parse(aux[5]); //NW
					nodeInfoArray[1] = int.Parse(aux[3]); //N
					nodeInfoArray[2] = int.Parse(aux[6]); //NE
					nodeInfoArray[3] = int.Parse(aux[1]); //W
					nodeInfoArray[4] = int.Parse(aux[2]); //E
					nodeInfoArray[5] = int.Parse(aux[7]); //SW
					nodeInfoArray[6] = int.Parse(aux[4]); //S
					nodeInfoArray[7] = int.Parse(aux[8]); //SE


					precompute[j,i] = nodeInfoArray;

					JPValues[j,i] = nodeInfoArray;
					//navmesh[i,j] = map.GetPixel(i * map.width / gridSize.x, (gridSize.y - 1 - j) * map.height / gridSize.y).r >= 0.5f;
				}
			}
		}

        if (File.Exists(GBFile))
        {
            StreamReader sr = File.OpenText(GBFile);
            string goalbound;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    goalbound = sr.ReadLine();

                    Vector2[] gbValues = new Vector2[16];
                    string[] aux = goalbound.Split('/');

                    for (int k = 1; k < 17; k++)
                    {
                        int startInd = aux[k].IndexOf("(") + 1; ;
                        float aXPosition = float.Parse(aux[k].Substring(startInd, aux[k].IndexOf(",") - startInd));
                        startInd = aux[k].IndexOf(",") + 1;
                        float aYPosition = float.Parse(aux[k].Substring(startInd, aux[k].IndexOf(")") - startInd));

                        gbValues[k-1] = new Vector2(aXPosition, aYPosition);
                    }

                    GBValues[j, i] = gbValues;
                }
            }
        }
	}

	public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<List<Vector2>, bool> callback){
		PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}
	
	void TryProcessNext(){
		if (!isProcessingPath && pathRequestQueue.Count > 0){
			currentPathRequest = pathRequestQueue.Dequeue();
			isProcessingPath = true;
			pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
		}
	}
	
	public void FinishedProcessingPath(List<Vector2> path, bool success){
		currentPathRequest.callback(path,success);
		isProcessingPath = false;
		TryProcessNext();
	}


	struct PathRequest {
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public Action<List<Vector2>, bool> callback;
		
		public PathRequest(Vector3 _start, Vector3 _end, Action<List<Vector2>, bool> _callback) {
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
		}
	}
}
