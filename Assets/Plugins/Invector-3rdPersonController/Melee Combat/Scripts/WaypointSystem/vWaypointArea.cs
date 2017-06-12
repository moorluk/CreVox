using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Invector
{
    public class vWaypointArea : MonoBehaviour
    {
        public List<vWaypoint> waypoints;
        public bool randomWayPoint;

        public vWaypoint GetRandomWayPoint()
        {
            System.Random random = new System.Random(100);
            var _nodes = GetValidPoints();
            var index = random.Next(0, waypoints.Count - 1);
            if (_nodes != null && _nodes.Count > 0 && index < _nodes.Count)
                return _nodes[index];

            return null;
        }

        public List<vWaypoint> GetValidPoints()
        {
            var _nodes = waypoints.FindAll(node => node.isValid);
            return _nodes;
        }

        public List<vPoint> GetValidSubPoints(vWaypoint waipoint)
        {
            var _nodes = waipoint.subPoints.FindAll(node => node.isValid);
            return _nodes;
        }

        public vWaypoint GetWayPoint(int index)
        {
            var _nodes = GetValidPoints();
            if (_nodes != null && _nodes.Count > 0 && index < _nodes.Count) return _nodes[index];

            return null;
        }
    }
}