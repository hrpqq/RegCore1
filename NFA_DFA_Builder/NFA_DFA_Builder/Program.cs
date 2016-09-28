using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFA_DFA_Builder.Model;


namespace NFA_DFA_Builder
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph.Reg = "(ad|c*b)*abb\n";
            Graph.MatchStr = "acbbadbabb";
            var g = new Graph();
            //g.Concat(new EndPointState(null, StateType.AcceptState, null));
            g.read();
            g.Traverse();
        }
    }
}
