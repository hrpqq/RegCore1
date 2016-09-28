using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFA_DFA_Builder.Model
{
    public class EndPointState : BaseState
    {
        private NormalState _master;
        public NormalState Master
        {
            get { return _master; }
        }

        public EndPointState RelativeEndPoint { get; private set; }

        public StateType? Type { get; private set; }

        public EndPointState(NormalState m,StateType type, char? c)
        {
            _master = m;
            Type = type;
            if (c.HasValue && m != null)
                base.AddOutEdge(c.Value, m);
        }

        /// <summary>
        /// 将两个端节点相互绑定
        /// </summary>
        /// <param name="eps"></param>
        public void SetRelativeEndPoint(EndPointState eps, bool keepBinding = true)
        {
            if (this.Type == eps.Type)
            {
                throw new Exception("同类型的终端节点不能相关");
            }
            else if (this.RelativeEndPoint == null)
            {
                this.RelativeEndPoint = eps;
            }
            if (keepBinding)
                eps.SetRelativeEndPoint(this,false);//反向绑定,形成一对
        }
        

        public void Collapse()
        {
            foreach (var s in this.RefStates)
            {
                _master.AddReference(s);
                s.UpdateOutEdge(this, _master);
            }

            _master.DeleteReference(this);
            
        }
    }
}
