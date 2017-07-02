using System;
using System.Collections.Generic;

namespace Astar
{
    /// <summary>
    /// MinHeap from ZeraldotNet (http://zeraldotnet.codeplex.com/)
    /// Modified by Roy Triesscheijn (http://roy-t.nl)    
    /// -Moved method variables to class variables
    /// -Added English Exceptions and comments (instead of Chinese)    
    /// </summary>    
    public class MinHeap
    {        
        private SearchNode listHead;

        public bool HasNext()
        {
            return listHead != null;
        }

        public void Add(SearchNode item)
        {
            if (listHead == null)
            {
                listHead = item;
            }
            else if (listHead.next == null && item.cost <= listHead.cost)
            {
                item.nextListElem = listHead;
                listHead = item;
            }
            else
            {
                SearchNode ptr = listHead;
                while (ptr.nextListElem != null && ptr.nextListElem.cost < item.cost)
                    ptr = ptr.nextListElem;
                item.nextListElem = ptr.nextListElem;
                ptr.nextListElem = item;
            }
        }

        public SearchNode ExtractFirst()
        {
            SearchNode result = listHead;
            listHead = listHead.nextListElem;
            return result;
        }
    }
}