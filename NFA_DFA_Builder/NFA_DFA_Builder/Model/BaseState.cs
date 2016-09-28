using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFA_DFA_Builder.Model
{
    public class BaseState
    {
        private static int _index = 0;

        private static int GenID()
        {
            return _index++;
        }

        public int ID { get; private set; }

        public Dictionary<char, List<BaseState>> OutEdges;

        /// <summary>
        /// 出边路由次数限制，key为char+sId，value为一个二元元组，其中item1为下限，item2为上限
        /// </summary>
        public Dictionary<string, Tuple<int,int>> OutEdgesRepeatRestrict;

        //引用当前状态的其它状态集合
        public HashSet<BaseState> RefStates;


        public BaseState()
        {
            ID = GenID();
            OutEdges = new Dictionary<char, List<BaseState>>();
            OutEdgesRepeatRestrict = new Dictionary<string, Tuple<int, int>>();
            RefStates = new HashSet<BaseState>();
        }

        public virtual void AddOutEdge(char c, BaseState s, int repS = -1, int repE = -1)
        {
            if (!OutEdges.ContainsKey(c))
                OutEdges[c] = new List<BaseState>();
            if (!OutEdges[c].Contains(s))
                OutEdges[c].Add(s);
            s.AddReference(this);
            

            //注意：当repS等于repE时，代表该出边被强制执行repS（或repE）次
            //且一个state只能有一个Repeat出边，当强制执行时，该state只有repeat出边可以路由，其他出边不可路由
            if (repS > -1 && repE >= repS)
            {
                OutEdgesRepeatRestrict.Add(c +"_" + s.ID.ToString(), new Tuple<int, int>(repS, repE));
            }

        }

        public virtual void UpdateOutEdge(BaseState old_S, BaseState new_S)
        {
            foreach (var edges in OutEdges)
            {
                edges.Value.ForEach(s => { s = s == old_S ? new_S : s; });
            }
        }
       

        public void AddReference(BaseState s)
        {
            if(!this.RefStates.Contains(s))
                this.RefStates.Add(s);
        }

        public void UpdateReference(BaseState old_S, BaseState new_S)
        {
            if (this.RefStates.Contains(old_S) && !this.RefStates.Contains(new_S))
            {
                this.RefStates.Remove(old_S);
                this.RefStates.Add(new_S);
            }
        }

        public void DeleteReference(BaseState s)
        {
            if (this.RefStates.Contains(s))
                this.RefStates.Remove(s);
        }
    }
}
