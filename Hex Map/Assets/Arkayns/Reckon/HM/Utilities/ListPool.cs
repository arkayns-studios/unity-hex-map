using System.Collections.Generic;

namespace Arkayns.Reckon.HM {

    public static class ListPool<T> {

        // -- Variables --
        private static Stack<List<T>> m_stack = new ();

        // -- Methods --
        public static List<T> Get() {
            return m_stack.Count > 0 ? m_stack.Pop() : new List<T>();
        } // Get ()

        public static void Add(List<T> list) {
            list.Clear();
            m_stack.Push(list);
        } // Add ()
        
    } // Class ListPool

} // Namespace Arkayns Reckon HM