using System;
using System.Collections.Generic;
using System.Text;

namespace Hardware
{
    class DataConverter : IDataConverter
    {
        public void Bike(byte[] bytes)
        {
            if (bytes == null)
            {
                Console.WriteLine("HEY, didn't get bytes!\n-Bike DataConverter");
            }
            else
            if (bytes.Length == 8)
            {
                
                switch (bytes[0])
                {
                    case 0x10:
                        if (bytes[1] != 25)
                        {
                            Console.WriteLine("WTF this isn't a bike");
                        }
                        Console.WriteLine($"Time since start is: {bytes[2] / 4}s (Rollover every 4s)");
                        Console.WriteLine($"Distance Traveled is : {bytes[3]}m (Rollover every 256m)");

                        int input = bytes[4] | (bytes[5] << 8);
                        Console.WriteLine($"Speed is : {input * 0.01}m/s (Range 65.534m/4)");
                        if (bytes[6] != 0xFF)
                        {
                            Console.WriteLine("Heart rate byte: {0}", Convert.ToString(bytes[6],2));
                        }
                        break;
                    case 0x19:
                        Console.WriteLine($"Event count: {bytes[1]} (Rollover 256)");
                        if (bytes[2] != 0xFF)
                        {
                            Console.WriteLine($"Instantaneous cadence: {bytes[2]} RPM (Range 0-254)");
                            
                        }
                        int accumPower = bytes[3] | (bytes[4] << 8);
                        
                        Console.WriteLine($"Accumulated power: {accumPower} watt (Rollover 65536)");

                        int instantPower = (bytes[5]) | (bytes[6]>>4)<<8;
                        

                        if (instantPower != 0xFFF)
                        Console.WriteLine($"Instant power: {instantPower} watt (Range 0-4094)");

                        int trainerStatus = bytes[6] & 0b00001111; // bit 4-7
                        int flags = bytes[7] >> 4;
                        int FEState = bytes[7] & 0b00001111;

                        break;

                    default:
                        Console.WriteLine("HEY, I never heard of data page {0}\n-DataConverter", bytes[0]);
                        break;
                }
            }
            else
            {
                Console.WriteLine("HEY, I didn't get 8 bytes!\n-DataConverter");
            }
            Console.WriteLine();
        }

        public void BPM(byte[] bytes)
        {
            if (bytes == null)
            {
                Console.WriteLine("HEY, didn't get bytes!\n-BPM DataConverter");
                return;
            }
            if (bytes[0] != 0)
            {
                Console.WriteLine("HOLY SHIT i got flags!!! {0} now i can't do anything\n-BPM DataConverter", bytes[0]);
            }
            else if (bytes.Length != 2)
            {
                Console.WriteLine("bytes length is: {0}", bytes.Length);
            }
            else
            {
                Console.WriteLine("BPM: {0}", bytes[1]);

            }
            Console.WriteLine();
        }
    }

    interface IDataConverter
    {
        void BPM(byte[] bytes);
        void Bike(byte[] bytes);
    }
}
