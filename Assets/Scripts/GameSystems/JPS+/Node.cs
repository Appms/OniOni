using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node> {
	
	public int gridX, gridY;
	
	public int dX, dY;
	
	public Node parent;
	
	
	
	public int gCost;
	public int hCost;
	
	public int fCost{
		get {
			return gCost + hCost;
		}
	}
	
	int heapIndex;
	public int HeapIndex{
		get{
			return heapIndex;
		}
		set{
			heapIndex = value;
		}
	}

	public Node(int _gridX, int _gridY){
		gridX = _gridX;
		gridY = _gridY;
		dX = 0;
		dY = 0;
		parent = null;
		gCost = -1;
	}
	public Node(int _gridX, int _gridY, int _dX, int _dY, Node _parent){
		gridX = _gridX;
		gridY = _gridY;
		dX = _dX;
		dY = _dY;
		parent = _parent;
		gCost = -1;
	}
	public Node(int _gridX, int _gridY, int _dX, int _dY, int _gCost, int _hCost, Node _parent){
		gridX = _gridX;
		gridY = _gridY;
		dX = _dX;
		dY = _dY;
		gCost = _gCost;
		hCost = _hCost;
		parent = _parent;
	}
	
	public int CompareTo(Node nodeToCompare){
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
	
	//int nw, n, ne, e, se, s, sw, w;
	
}
