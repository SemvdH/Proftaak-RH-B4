using System;
using System.Collections.Generic;
using System.Text;

namespace ProftaakRH
{
    public interface IHandler
    {
        void setResistance(float percentage);

        void stop();
    }
}
