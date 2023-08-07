using rat.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class MessageLog
    {
        private Queue<string> m_Messages;
        private int m_MaxMessages;

        public MessageLog(int maxMessages = 4)
        {
            m_Messages = new Queue<string>();
            m_MaxMessages = maxMessages;
        }

        public int LogSize => m_Messages.Count;
        public bool Empty => m_Messages.Count <= 0;

        public List<string> Messages => m_Messages.ToList();

        public void AppendMessage(in string message)
        {
            if (message != "") m_Messages.Enqueue(message);
        }

        public void TrimLog()
        {
            while (m_Messages.Count > m_MaxMessages)
                m_Messages.Dequeue();
        }

        public void ClearLog() => m_Messages.Clear();
    }
}
