using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JPSplus : MonoBehaviour {

	public JPSManager manager;

	Node targetNode;

	void Awake(){
		manager = GameObject.Find ("JPSManager").GetComponent<JPSManager>();
	}


	public void StartFindPath(Vector3 startPos, Vector3 targetPos){
		StartCoroutine(FindPath(startPos, targetPos));
	}

	public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) {
		Vector2 startPosGrid = Coords_GetGridFromWorld(startPos);
		Vector2 targetPosGrid = Coords_GetGridFromWorld(targetPos);

		Node startNode = new Node((int)startPosGrid.x, (int)startPosGrid.y);//getNodeFromPos(startPos);
		/*Node */targetNode = new Node((int)targetPosGrid.x, (int)targetPosGrid.y);

		Heap<Node> openSet = new Heap<Node>((int)(manager.gridSize.x * manager.gridSize.y));
		Heap<Node> closedSet = new Heap<Node>((int)(manager.gridSize.x * manager.gridSize.y));

		// OpenSet init
		for (int j = -1; j <= 1; j++){
			for (int i = -1; i <= 1; i++){
				if(i != 0 || j != 0){ // if not actual Starting Node
					if(startNode.gridX + i >= 0 && startNode.gridX + i < manager.gridSize.x && // Check boundaries
					   startNode.gridY + j >= 0 && startNode.gridY + j < manager.gridSize.y){

					    Node addNode = GetNeighbourAtDirection(startNode, i, j);
					    if (addNode != null) {
                            if(NodeInGoalBound(startNode, i, j)) openSet.Add(addNode);
                        } 
					}
				}
			}
		}

		bool success = false;

		while(openSet.Count > 0){
			Node currentNode = openSet.RemoveFirst();

			if(currentNode.hCost == 0){ success = true; break;}

			// Natural Primary Neighbour
			Node addNode = GetNeighbourAtDirection(currentNode, currentNode.dX, currentNode.dY);
			if(addNode != null && NodeInGoalBound(currentNode, currentNode.dX, currentNode.dY))openSet.Add(addNode);

			if(currentNode.dX != 0 && currentNode.dY != 0){ // Diagonal
				// Natural Secondary Neighbours
				Node nsn1 = GetNeighbourAtDirection(currentNode, currentNode.dX, 0);
				if(nsn1 != null && NodeInGoalBound(currentNode, currentNode.dX, 0)) openSet.Add(nsn1);
				Node nsn2 = GetNeighbourAtDirection(currentNode, 0, currentNode.dY);
				if(nsn2 != null && NodeInGoalBound(currentNode, 0, currentNode.dY)) openSet.Add(nsn2);
			}
			else{ //Straight
                  // Forced Neighbours
                  // Left
                //Anterior Pared?
                int check_dX = -currentNode.dX + currentNode.dY;
                int check_dY = -currentNode.dY + currentNode.dX;
                int checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                if (checkDirectionIndex > 4) checkDirectionIndex--;
                int checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                if (checkValue <= 0)
                {
                    //Lateral Pared?
                    check_dX = currentNode.dY;
                    check_dY = currentNode.dX;
                    checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                    if (checkDirectionIndex > 4) checkDirectionIndex--;
                    checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                    if (checkValue != 0)
                    {
                        Node leftFN = GetNeighbourAtDirection(currentNode, currentNode.dY, currentNode.dX);
                        if (leftFN != null && NodeInGoalBound(currentNode, currentNode.dY, currentNode.dX)) openSet.Add(leftFN);

                        //Diagonal Pared?
                        check_dX = currentNode.dX + currentNode.dY;
                        check_dY = currentNode.dY + currentNode.dX;
                        checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                        if (checkDirectionIndex > 4) checkDirectionIndex--;
                        checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                        if (checkValue != 0)
                        {
                            Node leftDFN = GetNeighbourAtDirection(currentNode, currentNode.dX + currentNode.dY, currentNode.dY + currentNode.dX);
                            if (leftDFN != null && NodeInGoalBound(currentNode, currentNode.dX + currentNode.dY, currentNode.dY + currentNode.dX)) openSet.Add(leftDFN);
                        }
                    }
                }


                    //Right
                //Anterior Pared?
                check_dX = -currentNode.dY - currentNode.dX;
                check_dY = -currentNode.dX - currentNode.dY;
                checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                if (checkDirectionIndex > 4) checkDirectionIndex--;
                checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                if (checkValue <= 0)
                {

                    //Lateral Pared?
                    check_dX = -currentNode.dY;
                    check_dY = -currentNode.dX;
                    checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                    if (checkDirectionIndex > 4) checkDirectionIndex--;
                    checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                    if (checkValue != 0)
                    {
                        Node rightFN = GetNeighbourAtDirection(currentNode, -currentNode.dY, -currentNode.dX);
                        if (rightFN != null && NodeInGoalBound(currentNode, -currentNode.dY, -currentNode.dX)) openSet.Add(rightFN);

                        //Diagonal Pared?
                        check_dX = currentNode.dX - currentNode.dY;
                        check_dY = currentNode.dY - currentNode.dX;
                        checkDirectionIndex = (check_dY + 1) * 3 + (check_dX + 1);
                        if (checkDirectionIndex > 4) checkDirectionIndex--;
                        checkValue = manager.JPValues[currentNode.gridX, currentNode.gridY][checkDirectionIndex];
                        if (checkValue != 0)
                        {
                            Node rightDFN = GetNeighbourAtDirection(currentNode, currentNode.dX - currentNode.dY, currentNode.dY - currentNode.dX);
                            if (rightDFN != null && NodeInGoalBound(currentNode, currentNode.dX - currentNode.dY, currentNode.dY - currentNode.dX)) openSet.Add(rightDFN);
                        }
                    }
                }
            }
			closedSet.Add(currentNode);
		}

		//if (success) return RetracePath(startNode, targetNode);
		//else return RetracePath(startNode, closedSet.RemoveFirst());
		yield return null;
		List<Vector2> path;
		if (success){
			path = RetracePath(startNode, targetNode);
		}
		else if(closedSet.Count > 0){
			path = RetracePath(startNode, closedSet.RemoveFirst());
		}
        else
        {
            path = new List<Vector2>();
            path.Add(new Vector2(targetPos.x, targetPos.z));
        }
		manager.FinishedProcessingPath(path, success);
	}

	

	private List<Vector2> RetracePath(Node sN, Node tN){
		List<Vector2> path = new List<Vector2>();
		Node currentNode = tN;
		while(currentNode != sN){
			path.Add(Coords_GetWorldFromGrid(currentNode.gridX, currentNode.gridY));
			currentNode = currentNode.parent;
		}
		path.Reverse();
		return path;
	}

	private Vector2 Coords_GetGridFromWorld(Vector3 position){
		int x = Mathf.FloorToInt((position.x + manager.worldSize.x/2) / manager.worldSize.x * manager.gridSize.x);
		int y = Mathf.FloorToInt((position.z - manager.worldSize.y/2) / -manager.worldSize.y * manager.gridSize.y);
		
		if (x >= manager.gridSize.x) x = (int)manager.gridSize.x -1;
		if (y >= manager.gridSize.y) y = (int)manager.gridSize.y -1;
		if (x < 0) x = 0;
		if (y < 0) y = 0;

		return new Vector2(x,y);
	}

	private Vector2 Coords_GetWorldFromGrid(int x, int y){
		return new Vector2(x / manager.gridSize.x * manager.worldSize.x - manager.worldSize.x/2f + manager.worldSize.x/manager.gridSize.x/2f, - y / manager.gridSize.y * manager.worldSize.y + manager.worldSize.y / 2f - manager.worldSize.y/manager.gridSize.y/2f); // Last term in each coordinate is to set the node position to the "center" of the node
	}



	private int GetDistance(Node nodeA, Node nodeB){
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		
		if (distX > distY)
			return 1414*distY + 1000*(distX-distY);
		return 1414*distX + 1000*(distY-distX);
	}
	private int GetGridDistanceFromTo(int aX, int aY, int bX, int bY){
		int distX = Mathf.Abs (aX - bX);
		int distY = Mathf.Abs (aY - bY);
		
		if (distX > distY)
			return distY + (distX-distY);
		return distX + (distY-distX);
	}

	private int GetHCost(int i, int j){
		int distX = Mathf.Abs (i - targetNode.gridX);
		int distY = Mathf.Abs (j - targetNode.gridY);
		
		if (distX > distY)
			return 1414*distY + 1000*(distX-distY);
		return 1414*distX + 1000*(distY-distX);
	}


	
	private Node GetNeighbourAtDirection(Node parentNode, int dX, int dY){
		if(parentNode.gridX + dX >= 0 && parentNode.gridX + dX < manager.gridSize.x && // Check boundaries
		   parentNode.gridY + dY >= 0 && parentNode.gridY + dY < manager.gridSize.y){
			int neighbourDirectionIndex = (dY+1)*3 + (dX+1);
			if(neighbourDirectionIndex > 4) neighbourDirectionIndex--;
			int neighbourValue = manager.JPValues[parentNode.gridX,parentNode.gridY][neighbourDirectionIndex];


			//Target Reachable
			int distanceToTarget = GetGridDistanceFromTo(parentNode.gridX, parentNode.gridY, targetNode.gridX, targetNode.gridY);
			if(parentNode.gridX + distanceToTarget * dX == targetNode.gridX && parentNode.gridY + distanceToTarget * dY == targetNode.gridY){
				if (distanceToTarget <= Mathf.Abs(neighbourValue)){
					targetNode.dX = dX;
					targetNode.dY = dY;
					targetNode.gCost = parentNode.gCost + (dX != 0 && dY != 0 ? 1414 : 1000) * distanceToTarget;
					targetNode.parent = parentNode;
					return targetNode;
				}
			}

			//Search Target JumpPoint
			if(dX!=0 && dY!=0){
				int targetJumpPointX, targetJumpPointY;
				if(Mathf.Abs(parentNode.gridX - targetNode.gridX) > Mathf.Abs(parentNode.gridY - targetNode.gridY)){
					targetJumpPointX = parentNode.gridX + (targetNode.gridY - parentNode.gridY)*dX*dY;
					targetJumpPointY = targetNode.gridY;
				}
				else{
					targetJumpPointX = targetNode.gridX;
					targetJumpPointY = parentNode.gridY + (targetNode.gridX - parentNode.gridX)*dY*dX;
				}
				// Check if appropiate direction
				if((targetJumpPointX - parentNode.gridX) * dX > 0 && (targetJumpPointY - parentNode.gridY) * dY > 0){
					if(Mathf.Abs(targetJumpPointX - parentNode.gridX) <= Mathf.Abs(neighbourValue)){
						return new Node(targetJumpPointX, targetJumpPointY,
						                dX, dY,
						                parentNode.gCost + 1414 * Mathf.Abs(targetJumpPointX - parentNode.gridX), // Always Diagonal!
						                GetHCost(targetJumpPointX, targetJumpPointY),
						                parentNode);
					}
				}
			}

			// if no Target JumpPoint, do a normal step
			if(neighbourValue > 0){
				int newX = parentNode.gridX + dX * neighbourValue;
				int newY = parentNode.gridY + dY * neighbourValue;

				
				return new Node(newX, newY,
				                dX, dY,
				                parentNode.gCost + (dX != 0 && dY != 0 ? 1414 : 1000) * neighbourValue,
				                GetHCost(newX, newY),
				                parentNode);
			}
		}
		return null;
	}
	
	public bool NodeInGoalBound(Node node, int dirx, int diry) {
        bool result = false;

        switch (dirx) {
            case -1:
                switch (diry)
                {
                    case 0:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][0].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][1].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][0].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][1].y;
                        break;
                    case -1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][2].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][3].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][2].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][3].y;
                        break;
                    case 1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][4].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][5].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][4].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][5].y;
                        break;
                }
                break;
            case 1:
                switch (diry)
                {
                    case 0:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][6].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][7].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][6].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][7].y;
                        break;
                    case -1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][8].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][9].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][8].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][9].y;
                        break;
                    case 1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][10].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][11].x
                            && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][10].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][11].y;
                        break;
                }
                break;
            case 0:
                switch (diry)
                {
                    case -1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][12].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][13].x
                        && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][12].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][13].y;
                        break;

                    case 1:
                        result = targetNode.gridX <= manager.GBValues[node.gridX, node.gridY][14].x && targetNode.gridX >= manager.GBValues[node.gridX, node.gridY][15].x
                        && targetNode.gridY <= manager.GBValues[node.gridX, node.gridY][14].y && targetNode.gridY >= manager.GBValues[node.gridX, node.gridY][15].y;
                        break;
                }
                break;
        }
        return result;
    }
}