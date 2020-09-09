using System;
using System.Collections.Generic;
using System.Text;

namespace Hardware
{
    class DataConverter : IDataConverter
    {
        public void Bike(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public void BPM(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }

    interface IDataConverter
    {
        void BPM(byte[] bytes);
        void Bike(byte[] bytes);
    }
}
