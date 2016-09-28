using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace NFA_DFA_Builder.Model
{
    public class Graph
    {
        /// <summary>
        /// 静态定义
        /// </summary>

        //全局字符索引
        public static int GIndex = 0;
        //待解析的正则表达式
        public static string Reg = null;

        public static string MatchStr ;

        public static char[] Words = null;

        private static Stack<Graph> GraphStack;


        static Graph()
        {
            GraphStack = new Stack<Graph>();
            Words = new Char[62];
            for (byte i = 0; i < 10; i++)
            {
                Words[i] = i.ToString().ElementAt(0);
            }
            int s = 'a';
            for (int j = 0; j< 26; j++)
            {
                Words[j+10] = (char)(s + j);
            }
            int s1 = 'A';
            for (int k = 0; k < 26; k++)
            {
                Words[k + 36] = (char)(s1 + k);
            }
        }


       /// <summary>
       /// 实例定义
       /// </summary>

        private List<object> Temp_OR_SET { get; set; }

        public EndPointState HeaderState { get; set; }

        public EndPointState TailState { get; set; }

        private EndPointState LastEndPoint { get; set; }


        public Graph()
        {
            HeaderState = new EndPointState(null,StateType.HeadState,'_');
            TailState = new EndPointState(null, StateType.TailState, '_');
            HeaderState.SetRelativeEndPoint(TailState, true);
            LastEndPoint = null;
        }

        /// <summary>
        /// 使路由直接忽略当前NFA子图,这样当前模式将允许出现0次匹配
        /// </summary>
        public void DirectPass()
        {
            //让首尾节点直接连接
            this.HeaderState.AddOutEdge('_', TailState);
        }




        public void Concat(EndPointState s)
        {
            if (LastEndPoint == null)
                this.HeaderState.AddOutEdge('_', s);
            else
                LastEndPoint.AddOutEdge('_', s);

            LastEndPoint = s.RelativeEndPoint;
        }

       


        public void read()
        {
            Graph.GraphStack.Push(this);
            Func<char> f = () => Graph.Reg.ElementAt(GIndex);

            while (true)
            {
               
                if (IsWord(f))
                {
                    var s = this.GetSingalCharGraph(f());
                    if (this.Temp_OR_SET != null)
                        Build_OR_ChildrenGraph();
                    this.Concat(s);
                }
                else if (IsLeftBra(f))
                {
                    if (this.Temp_OR_SET != null)
                        Build_OR_ChildrenGraph();

                    var g = new Graph();
                    g.read();
                    this.Concat(g.HeaderState);
                }
                else if (IsRightBra(f))
                {
                    if (this.Temp_OR_SET != null)
                        Build_OR_ChildrenGraph();

                    this.LastEndPoint.AddOutEdge('_', this.TailState);
                    GraphStack.Pop();

                    return;
                }
                else if (IsOr(f))
                {
                    
                    //使用临时“或”集合
                    if (this.Temp_OR_SET == null)
                    {
                        this.Temp_OR_SET = new List<object>();
                        this.Temp_OR_SET.Add(LastEndPoint.RelativeEndPoint);//or子图中的第一个结构
                        LastEndPoint.RelativeEndPoint.RefStates.ElementAt(0).OutEdges['_'].Remove(LastEndPoint.RelativeEndPoint);
                        LastEndPoint = LastEndPoint.RelativeEndPoint.RefStates.ElementAt(0) as EndPointState;
                    }

                    if (IsWord(f))
                    {
                        var s = this.GetSingalCharGraph(f());
                        if(IsRepeat(f))
                            s.RelativeEndPoint.AddOutEdge('_', s);
                        this.Temp_OR_SET.Add(s);
                    }
                    else if (IsLeftBra(f))
                    {
                        var g = new Graph();
                        GraphStack.Push(g);
                        g.read();
                        if (IsRepeat(f))
                            g.TailState.AddOutEdge('_', g.TailState.RelativeEndPoint);
                        this.Temp_OR_SET.Add(g);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (IsRepeat(f))
                {
                    this.LastEndPoint.AddOutEdge('_', this.LastEndPoint.RelativeEndPoint);
                }
                else if (IsEOF(f))
                {
                    GraphStack.Pop();
                    return;
                }

            }
            
        }

        private void Build_OR_ChildrenGraph()
        {
            var start = new EndPointState(null, StateType.HeadState, null);
            var end = new EndPointState(null, StateType.TailState, null);
            start.SetRelativeEndPoint(end);

            foreach (var unit in this.Temp_OR_SET)
            {
                if (unit is Graph)
                {
                    var u = unit as Graph;
                    start.AddOutEdge('_', u.HeaderState);
                    u.TailState.AddOutEdge('_', end);
                }
                else if (unit is NormalState)
                {
                    var u = unit as NormalState;
                    start.AddOutEdge('_', u.Head);
                    u.Tail.AddOutEdge('_', end);
                }
                else if (unit is EndPointState)
                {
                    var u = unit as EndPointState;
                    start.AddOutEdge('_', u);
                    u.RelativeEndPoint.AddOutEdge('_', end);
                }
            }
            this.LastEndPoint.AddOutEdge('_', start);
            this.LastEndPoint = end;

            //清理“或”集合，为下次处理“或”结构作准备
            this.Temp_OR_SET = null;

        }

        public void Traverse()
        {
            List<BaseState> StepStates = new List<BaseState>();
            StepStates.Add(this.HeaderState);


            int mcount = 0;

            for (int i = 0; ; i++)
            {
                var nextTempStates = new List<BaseState>();
                foreach (var s in StepStates)
                {
                    var nexts = new List<BaseState>();
                    GetEdge(s, nexts);
                    
                    nexts.ForEach(q => { if (!nextTempStates.Contains(q)) { nextTempStates.Add(q); } });
                }
                StepStates = nextTempStates;

                //string output = "";
                //nextTempStates.ForEach(q => { output += (q as NormalState).innerChar; });
                //Console.WriteLine(output);

                StepStates = StepStates.Where(q=> { return (q as NormalState).innerChar == Graph.MatchStr.ElementAt(i); }).ToList();
                if (StepStates.Count > 0)
                {
                    mcount++;
                    if (mcount == Graph.MatchStr.Length)
                    {

                    }
                }
                else
                {

                }


            }
        }

        private void GetEdge(BaseState s,List<BaseState> states)
        {
           
            foreach(var edges in s.OutEdges)
            {
                if (edges.Key != '_')
                {
                    edges.Value.ForEach(q => { if (!states.Contains(q)) { states.Add(q); } }); 
                }
                else
                {
                    var nexts = new List<BaseState>();
                    edges.Value.ForEach(q => GetEdge(q, nexts));
                    nexts.ForEach(q => { if (!states.Contains(q)) { states.Add(q); } });
                }
            }


        }


        /// <summary>
        /// 无用的endpoint被定义为，出边和入边都是'_'的端节点
        /// </summary>
        public void deleteUselessEndPoint()
        {
            
        }


        private bool IsWord(Func<char> f)
        {
            var c = f();
            if (Words.Contains(c))
                return true;
            else
                return false;
        }

        private bool IsOr(Func<char> f)
        {
            var c = f();
            if (c == '|')
            {
                GIndex++;
                return true;
            }
            else
                return false;
        }

        private bool IsRepeat(Func<char> f)
        {
            var c = f();
            if (c == '*')
            {
                GIndex++;
                return true;
            }
            else
                return false;
        }

        private bool IsLeftBra(Func<char> f)
        {
            var c = f();
            if (c == '(')
            {
                GIndex++;
                return true;
            }
            else
                return false;
        }

        private bool IsRightBra(Func<char> f)
        {
            var c = f();
            if (c == ')')
            {
                GIndex++;
                return true;
            }
            else
                return false;
        }

        private bool IsEscape(Func<char> f)
        {
            var c = f();
            if (c == '\\')
            {
                GIndex++;
                return true;
            }

            else
                return false;
        }
        private bool IsEOF(Func<char> f)
        {
            var c = f();
            if (c == '\n')
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 获取关于一个字符的NFA图
        /// 这个图包括一个头节点，一个
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public EndPointState GetSingalCharGraph(char c)
        {
            var mid = new NormalState();
            var start = new EndPointState(mid,StateType.HeadState, c);
            var end = new EndPointState(mid,StateType.TailState, null);
            

            start.AddOutEdge(c, mid);
            start.SetRelativeEndPoint(end);
            mid.AddOutEdge('_', end);
            mid.Set_Head_Tail(start, end);
            mid.innerChar = c;
            GIndex++;

            return start;
        }
    }
}
