using GC = System.GC;
using Math = System.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrevoxExtend {
	public class AStar {
		//show node state.
		public enum nodeType {
			wall = 0,
			none = 1,
			open = 2,
			start = 3,
			end = 4
		}

		//using node to show position state for A*.
		public class Node {
			public nodeType type {
				get;
				set;
			}

			public Vector3 position3 {
				get;
				private set;
			}

			public int X {
				get;
				set;
			}
			public int Y {
				get;
				set;
			}

			public int Z {
				get;
				set;
			}
			public float fScore {
				get { return hScore + gScore; }
			}

			public float hScore {
				get;
				private set;
			}

			public int gScore {
				get;
				private set;
			}

			//constructor.
			public Node(int X, int Y, int Z, nodeType type) {
				this.position3 = new Vector3(X * 3, Y * 2, Z * 3);
				this.type = type;
				this.hScore = float.MaxValue;
				this.gScore = int.MaxValue;
				this.X = X;
				this.Y = Y;
				this.Z = Z;
			}

			//update the gscore and hscore,if gscore + hscore < current fscore.
			public void distanceToEnd(Node end, int time) {
				//if thie node is end, and then make this node fscore to  zero, and select it.
				if (this.type == nodeType.end) {
					this.gScore = 0;
					this.hScore = 0;
				}

				var temp = Math.Max(Math.Abs(this.X - end.X), Math.Max(Math.Abs(this.Y - end.Y), Math.Abs(this.Z - end.Z)));
				if (temp + time < this.fScore) {
					this.hScore = temp;
					this.gScore = time;
				}
			}

			public override string ToString() {
				return (position3.ToString());
			}
		}

		//use nodeMap to show map of volume state.
		public Node[,,] nodeMap {
			get;
			private set;
		}

		public List<Node> theShortestPath {
			get;
			private set;
		}

		public int time {
			get;
			private set;
		}

		//constructor
		//when new it, it will do A* algorithm, and then you can call this object.theShortestPath to get the
		//shortest path.
		public AStar(int[,,] map, Vector3 start, Vector3 end) {
			this.nodeMap = mapToNodeMap(map);
			this.nodeMap[(int)start.x / 3, (int)start.y / 2, (int)start.z / 3].type = nodeType.start;
			this.nodeMap[(int)end.x / 3, (int)end.y / 2, (int)end.z / 3].type = nodeType.end;
			this.time = 0;
			this.theShortestPath = findThePath(this.nodeMap[(int)start.x / 3, (int)start.y / 2, (int)start.z / 3], this.nodeMap[(int)end.x / 3, (int)end.y / 2, (int)end.z / 3]);
			this.theShortestPath.Reverse();
			GC.Collect();
		}

		//int map to nodeMap for A*.
		public Node[,,] mapToNodeMap(int[,,] map) {
			Node[,,] nodeMap;
			nodeMap = new Node[map.GetLength(0), map.GetLength(1), map.GetLength(2)];
			for (var X = 0; X < map.GetLength(0); ++X) {
				for (var Y = 0; Y < map.GetLength(1); ++Y) {
					for (var Z = 0; Z < map.GetLength(2); ++Z)
						nodeMap[X, Y, Z] = new Node(X, Y, Z, (nodeType)map[X, Y, Z]);
				}
			}
			return nodeMap;
		}

		//find the shortest path.
		public List<Node> findThePath(Node start, Node end) {
			bool finish = false;
			Stack<Node> answer = new Stack<Node>();
			answer.Push(start);
			while (!finish) {
				var aroundPositions = findAround(answer.Peek());
				var temp = answer.Peek();
				for (var i = 0; i < aroundPositions.Count; ++i) {
					aroundPositions[i].distanceToEnd(end, time);
					//if fscore is same then select node by using hscore.
					if (aroundPositions[i].fScore == temp.fScore && aroundPositions[i].hScore < temp.hScore) {
						temp = aroundPositions[i];
						continue;
					}
					//find the minimum fscore in the round.
					if (aroundPositions[i].fScore < temp.fScore)
						temp = aroundPositions[i];
				}
				answer.Push(temp);
				time++;
				if (time > 1000)
					break;
				if (temp.type == nodeType.end)
					finish = true;
			}
			return answer.ToList();
		}

		//find the around node of input node.
		public List<Node> findAround(Node node) {
			List<Node> temp = new List<Node>();
			for (var i = -1; i < 2; ++i) {
				for (var j = -1; j < 2; ++j) {
					for (var k = -1; k < 2; ++k) {
						if (//skip out of range.
							node.X + i < 0 || node.Y + j < 0 || node.Z + k < 0 ||
							node.X + i >= nodeMap.GetLength(0) ||
							node.Y + j >= nodeMap.GetLength(1) ||
							node.Z + k >= nodeMap.GetLength(2) ||
							//skip the origin node.
							(i == 0 && j == 0 && k == 0))
							continue;
						if (nodeMap[node.X + i, node.Y + j, node.Z + k].type == nodeType.wall)
							continue;
						temp.Add(nodeMap[node.X + i, node.Y + j, node.Z + k]);
					}
				}
			}
			return temp;
		}
	}
}