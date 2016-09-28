using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFA_DFA_Builder.Model
{
    public class NormalState:BaseState
    { 
        public EndPointState Head { get;private set; }

        public EndPointState Tail { get;private set; }

        public char innerChar { get; set; }

        public NormalState()
        {         
        }

        public void Set_Head_Tail(EndPointState head, EndPointState tail)
        {
            Head = head;
            Tail = tail;
        }
    }
}
