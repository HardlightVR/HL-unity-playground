/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NullSpace.SDK.NewLocation;

namespace NullSpace.SDK
{
	public enum ControllerSide
	{
		None = 0,
		Right = 1,
		Left = 2,
		Both = 3
	}
	public class NewGraphEngine
	{
		//public class HapticLocationNode
		//{
		//	private AreaFlag _name;
		//	public AreaFlag Location
		//	{
		//		get { return _name; }
		//	}
		//	public HapticLocationNode(AreaFlag loc)
		//	{
		//		_name = loc;
		//	}
		//	public override string ToString()
		//	{
		//		return _name.ToString();
		//	}
		//}

		#region New Location Approach

		//public abstract class HapticLocationNode : IHapticLocation
		//{
		//	public abstract int Where();
		//	public abstract HapticLocationType GetHapticType();
		//	public virtual AreaFlag ConvertToAreaFlag()
		//	{ return AreaFlag.None; }
		//	public virtual ControllerSide ConvertToControllerSide()
		//	{ return ControllerSide.None; }
		//}

		//public class SuitLocation : HapticLocationNode
		//{
		//	public AreaFlag MyLocation;
		//	public SuitLocation(AreaFlag Location)
		//	{
		//		MyLocation = Location;
		//	}
		//	public override int Where()
		//	{
		//		return (int)MyLocation;
		//	}
		//	public override HapticLocationType GetHapticType()
		//	{
		//		return HapticLocationType.Suit;
		//	}
		//	public override AreaFlag ConvertToAreaFlag()
		//	{
		//		return MyLocation;
		//	}
		//	public override string ToString()
		//	{
		//		return MyLocation.ToStringIncludedAreas();
		//	}
		//}
		//public class ControllerLocation : HapticLocationNode
		//{
		//	public ControllerSide MyLocation = 0;
		//	public ControllerLocation(ControllerSide Location)
		//	{
		//		MyLocation = Location;
		//	}
		//	public override int Where()
		//	{
		//		return (int)MyLocation;
		//	}
		//	public override HapticLocationType GetHapticType()
		//	{
		//		return HapticLocationType.Controller;
		//	}
		//	public override ControllerSide ConvertToControllerSide()
		//	{
		//		return MyLocation;
		//	}
		//	public override string ToString()
		//	{
		//		return "Controller [" + MyLocation.ToString() + "]";
		//	}
		//} 
		#endregion

		private Dictionary<int, HapticLocationNode> _hapticNodes;
		private Dictionary<AreaFlag, HapticLocationNode> _nodes;
		private Dictionary<HapticLocationNode, List<HapticLocationNode>> _graph;
		private Dictionary<HapticLocationNode, Dictionary<HapticLocationNode, int>> _weights;
		public NewGraphEngine()
		{
			_weights = new Dictionary<HapticLocationNode, Dictionary<HapticLocationNode, int>>();
			_graph = new Dictionary<HapticLocationNode, List<HapticLocationNode>>();
			_nodes = new Dictionary<AreaFlag, HapticLocationNode>();
			_hapticNodes = new Dictionary<int, HapticLocationNode>();
			SuitLocation leftShoulder = new SuitLocation(AreaFlag.Shoulder_Left);
			SuitLocation leftChest = new SuitLocation(AreaFlag.Chest_Left);
			SuitLocation leftBack = new SuitLocation(AreaFlag.Back_Left);
			SuitLocation leftUpperArm = new SuitLocation(AreaFlag.Upper_Arm_Left);
			SuitLocation leftForearm = new SuitLocation(AreaFlag.Forearm_Left);
			SuitLocation leftUpperAbs = new SuitLocation(AreaFlag.Upper_Ab_Left);
			SuitLocation leftMidAbs = new SuitLocation(AreaFlag.Mid_Ab_Left);
			SuitLocation leftLowerAbs = new SuitLocation(AreaFlag.Lower_Ab_Left);

			SuitLocation rightShoulder = new SuitLocation(AreaFlag.Shoulder_Right);
			SuitLocation rightChest = new SuitLocation(AreaFlag.Chest_Right);
			SuitLocation rightBack = new SuitLocation(AreaFlag.Back_Right);
			SuitLocation rightUpperArm = new SuitLocation(AreaFlag.Upper_Arm_Right);
			SuitLocation rightForearm = new SuitLocation(AreaFlag.Forearm_Right);
			SuitLocation rightUpperAbs = new SuitLocation(AreaFlag.Upper_Ab_Right);
			SuitLocation rightMidAbs = new SuitLocation(AreaFlag.Mid_Ab_Right);
			SuitLocation rightLowerAbs = new SuitLocation(AreaFlag.Lower_Ab_Right);

			ControllerLocation rightController = new ControllerLocation(ControllerSide.Right);
			ControllerLocation leftController = new ControllerLocation(ControllerSide.Left);

			List<HapticLocationNode> nodes = new List<HapticLocationNode> { rightShoulder, rightChest, rightBack, rightUpperArm, rightForearm, rightUpperAbs, rightMidAbs, rightLowerAbs, leftShoulder, leftChest, leftBack, leftUpperArm, leftForearm, leftUpperAbs, leftMidAbs, leftLowerAbs };
			foreach (var node in nodes)
			{
				_graph[node] = new List<HapticLocationNode>();
				_weights[node] = new Dictionary<HapticLocationNode, int>();

				if (node.GetHapticType() == HapticLocationType.Suit)
				{
					_nodes[((SuitLocation)node).MyLocation] = node;
				}
				if (node.GetHapticType() == HapticLocationType.Controller)
				{
					_hapticNodes[((ControllerLocation)node).Where()] = node;
				}
			}

			InsertEdge(rightController, rightForearm, 6);
			InsertEdge(leftController, leftForearm, 6);

			InsertEdge(leftChest, rightChest, 10);
			InsertEdge(leftChest, leftUpperAbs, 10);
			InsertEdge(leftChest, rightUpperAbs, 17);
			InsertEdge(leftChest, leftShoulder, 10);

			//Right chest
			InsertEdge(rightChest, leftChest, 10);
			InsertEdge(rightChest, rightUpperAbs, 10);
			InsertEdge(rightChest, leftUpperAbs, 17);
			InsertEdge(rightChest, rightShoulder, 10);

			//Left Upper Abs
			InsertEdge(leftUpperAbs, leftChest, 10);
			InsertEdge(leftUpperAbs, leftMidAbs, 10);
			InsertEdge(leftUpperAbs, rightUpperAbs, 10);
			InsertEdge(leftUpperAbs, rightMidAbs, 14);
			InsertEdge(leftUpperAbs, rightChest, 17);

			//Right Upper Abs
			InsertEdge(rightUpperAbs, rightChest, 10);
			InsertEdge(rightUpperAbs, rightMidAbs, 10);
			InsertEdge(rightUpperAbs, leftUpperAbs, 10);
			InsertEdge(rightUpperAbs, leftMidAbs, 14);
			InsertEdge(rightUpperAbs, leftChest, 17);

			//Left Mid Abs 
			InsertEdge(leftMidAbs, leftUpperAbs, 10);
			InsertEdge(leftMidAbs, leftLowerAbs, 10);
			InsertEdge(leftMidAbs, rightUpperAbs, 14);
			InsertEdge(leftMidAbs, rightMidAbs, 10);
			InsertEdge(leftMidAbs, rightLowerAbs, 14);

			//Right Mid Abs 
			InsertEdge(rightMidAbs, rightUpperAbs, 10);
			InsertEdge(rightMidAbs, rightLowerAbs, 10);
			InsertEdge(rightMidAbs, leftUpperAbs, 14);
			InsertEdge(rightMidAbs, leftMidAbs, 10);
			InsertEdge(rightMidAbs, leftLowerAbs, 14);

			//Left Lower Abs
			InsertEdge(leftLowerAbs, rightLowerAbs, 10);
			InsertEdge(leftLowerAbs, rightMidAbs, 14);
			InsertEdge(leftLowerAbs, leftMidAbs, 10);

			//Right Lower Abs   
			InsertEdge(rightLowerAbs, leftLowerAbs, 10);
			InsertEdge(rightLowerAbs, leftMidAbs, 14);
			InsertEdge(rightLowerAbs, rightMidAbs, 10);

			//Left shoulder
			InsertEdge(leftShoulder, leftChest, 10);
			InsertEdge(leftShoulder, leftBack, 20);
			InsertEdge(leftShoulder, leftUpperArm, 10);

			//Right shoulder
			InsertEdge(rightShoulder, rightChest, 10);
			InsertEdge(rightShoulder, rightBack, 20);
			InsertEdge(rightShoulder, rightUpperArm, 10);

			//Left upper arm
			InsertEdge(leftUpperArm, leftShoulder, 10);
			InsertEdge(leftUpperArm, leftForearm, 10);

			//Right upper arm
			InsertEdge(rightUpperArm, rightShoulder, 10);
			InsertEdge(rightUpperArm, rightForearm, 10);

			//Left forearm
			InsertEdge(leftForearm, leftUpperArm, 10);
			InsertEdge(leftForearm, leftController, 6);

			//Right forearm
			InsertEdge(rightForearm, rightUpperArm, 10);
			InsertEdge(rightForearm, rightController, 6);

			//Left back
			InsertEdge(leftBack, leftShoulder, 20);
			InsertEdge(leftBack, rightBack, 20);

			//Right back
			InsertEdge(rightBack, rightShoulder, 20);
			InsertEdge(rightBack, leftBack, 20);
		}
		public List<HapticLocationNode> Dijkstras(AreaFlag beginNode, AreaFlag endNode)
		{
			Dictionary<HapticLocationNode, float> dist = new Dictionary<HapticLocationNode, float>();
			Dictionary<HapticLocationNode, HapticLocationNode> prev = new Dictionary<HapticLocationNode, HapticLocationNode>();

			var stack = new List<HapticLocationNode>();
			foreach (var n in _nodes)
			{
				dist[n.Value] = float.PositiveInfinity;
				prev[n.Value] = null;
				stack.Add(n.Value);
			}
			var source = _nodes[beginNode];
			var sink = _nodes[endNode];
			dist[source] = 0;

			while (stack.Count > 0)
			{
				var u = minDist(stack, dist);
				if (u.Where() == _nodes[endNode].Where())
				{
					break;
				}
				stack.Remove(u);
				foreach (var neighbor in Neighbors(u))
				{
					var alt = dist[u] + _weights[u][neighbor];
					if (alt < dist[neighbor])
					{
						dist[neighbor] = alt;
						prev[neighbor] = u;
					}
				}
			}

			List<HapticLocationNode> s = new List<HapticLocationNode>();
			var u2 = sink;
			while (prev[u2] != null)
			{
				s.Add(u2);
				u2 = prev[u2];
			}
			s.Add(source);
			s.Reverse();
			return s;
		}

		private HapticLocationNode minDist(IList<HapticLocationNode> stack, Dictionary<HapticLocationNode, float> dist)
		{
			HapticLocationNode best = null;
			float bestDist = float.PositiveInfinity;
			foreach (var node in stack)
			{
				if (dist[node] < bestDist)
				{
					best = node;
					bestDist = dist[node];
				}
			}

			return best;
		}

		public List<HapticLocationNode> DFS(AreaFlag beginNode, AreaFlag endNode)
		{
			var stages = new List<HapticLocationNode>();
			var stack = new Stack<HapticLocationNode>();
			Dictionary<HapticLocationNode, HapticLocationNode> cameFrom = new Dictionary<HapticLocationNode, HapticLocationNode>();
			var source = _nodes[beginNode];
			//var sink = _nodes[endNode];
			Dictionary<HapticLocationNode, bool> visited = new Dictionary<HapticLocationNode, bool>();
			foreach (var n in _nodes)
			{
				visited[n.Value] = false;
			}
			stack.Push(source);
			while (stack.Count != 0)
			{
				var v = stack.Pop();
				if (!visited[v])
				{
					visited[v] = true;
					var neighbors = Neighbors(v);
					foreach (var neigh in neighbors)
					{
						stack.Push(neigh);
						cameFrom[neigh] = v;
					}
					stages.Add(v);


				}
			}

			return stages;

		}
		public List<List<HapticLocationNode>> BFS(AreaFlag beginNode, int maxDepth)
		{
			maxDepth = Math.Max(0, maxDepth);
			List<List<HapticLocationNode>> stages = new List<List<HapticLocationNode>>();
			HapticLocationNode source = _nodes[beginNode];
			Dictionary<HapticLocationNode, bool> visited = new Dictionary<HapticLocationNode, bool>();
			foreach (var n in _nodes)
			{
				visited[n.Value] = false;
			}
			Queue<HapticLocationNode> queue = new Queue<HapticLocationNode>();
			int currentDepth = 0;
			int elementsToDepthIncrease = 1;
			int nextElementsToDepthIncrease = 0;
			visited[source] = true;
			queue.Enqueue(source);
			stages.Add(new List<HapticLocationNode>() { source });
			List<HapticLocationNode> potentialNextStage = new List<HapticLocationNode>();

			while (queue.Count != 0)
			{
				source = queue.Dequeue();
				var neighbors = Neighbors(source);



				foreach (var neighbor in neighbors)
				{
					if (!visited[neighbor])
					{
						visited[neighbor] = true;
						queue.Enqueue(neighbor);
						potentialNextStage.Add(neighbor);
						nextElementsToDepthIncrease++;
					}
				}

				if (--elementsToDepthIncrease == 0)
				{
					if (potentialNextStage.Count > 0) { stages.Add(new List<HapticLocationNode>(potentialNextStage)); }
					if (++currentDepth == maxDepth) return stages;
					elementsToDepthIncrease = nextElementsToDepthIncrease;
					nextElementsToDepthIncrease = 0;
					potentialNextStage.Clear();
				}

			}

			return stages;
		}


		private int Cost(HapticLocationNode a, HapticLocationNode b)
		{
			return _weights[a][b];
		}


		private List<HapticLocationNode> Neighbors(HapticLocationNode node)
		{
			return _graph[node];
		}
		private void InsertEdge(HapticLocationNode nodeA, HapticLocationNode nodeB, int weight)
		{
			if (!_graph[nodeA].Contains(nodeB))
			{
				_graph[nodeA].Add(nodeB);
			}
			if (!_graph[nodeB].Contains(nodeA))
			{
				_graph[nodeB].Add(nodeA);
			}

			_weights[nodeA][nodeB] = weight;
			_weights[nodeB][nodeA] = weight;

		}
	}
}
