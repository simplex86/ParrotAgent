using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.NUI
{
    internal interface IState
    {
        Task Enter();
        Task Exit();
    }
}
