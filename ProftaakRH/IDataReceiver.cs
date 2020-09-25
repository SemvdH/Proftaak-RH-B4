using System;
using System.Collections.Generic;
using System.Text;

namespace ProftaakRH
{
    public interface IDataReceiver
    {
        void BPM(byte[] bytes);
        void Bike(byte[] bytes);
    }
}
