using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astar
{
    /// <summary>
    /// Author: Roy Triesscheijn (http://www.roy-t.nl)
    /// Class defining BreadCrumbs used in path finding to mark our routes
    /// </summary>
    public class SearchNode
    {
        public Point3D position;
        public int cost;
        public int pathCost;
        public SearchNode next;
        public SearchNode nextListElem;

        public SearchNode(Point3D position, int cost, int pathCost, SearchNode next)
        {
            this.position = position;
            this.cost = cost;
            this.pathCost = pathCost;
            this.next = next;
        }
    }
}
