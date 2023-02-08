using System.Collections.Generic;

namespace Arkayns.Reckon.HM {

    public class HexCellPriorityQueue {

        // -- Variables --
        private List<HexCell> m_list = new ();
        private int m_count = 0;
        private int m_minimum = int.MaxValue;
        
        // -- Properties --
        public int Count => m_count;

        // -- Methods --
        public void Enqueue(HexCell cell) {
            m_count += 1;
            var priority = cell.SearchPriority;
            
            if (priority < m_minimum) {
                m_minimum = priority;
            }
            
            while (priority >= m_list.Count) {
                m_list.Add(null);
            }
            cell.NextWithSamePriority = m_list[priority];
            m_list[priority] = cell;
        } // Enqueue ()

        public HexCell Dequeue () {
            m_count -= 1;
            for (; m_minimum < m_list.Count; m_minimum++) {
                var cell = m_list[m_minimum];
                if (cell != null) {
                    m_list[m_minimum] = cell.NextWithSamePriority;
                    return cell;
                }
            }
            return null;
        } // Dequeue ()

        public void Change(HexCell cell, int oldPriority) {
            var current = m_list[oldPriority];
            var next = current.NextWithSamePriority;
            
            if (current == cell) {
                m_list[oldPriority] = next;
            } else {
                while (next != cell) {
                    current = next;
                    next = current.NextWithSamePriority;
                }
                current.NextWithSamePriority = cell.NextWithSamePriority;
            }
            Enqueue(cell);
            m_count -= 1;
        } // Change ()
	
        public void Clear () {
            m_list.Clear();
            m_count = 0;
            m_minimum = int.MaxValue;
        } // Clear ()
        
    } // Class HexCellPriorityQueue

} // Namespace Arkayns Reckon HM