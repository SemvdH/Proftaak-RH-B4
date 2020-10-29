using System.Collections.ObjectModel;
using Util;

namespace DoctorApp.Models
{
    class PatientInfo : ObservableObject
    {
        public ObservableCollection<string> ChatLog { get; set; }
        public string Username { get; set; }

        public string Status { get; set; }

        public double Speed { get; set; }

        public int BPM { get; set; }

        public float Resistance { get; set; }
        public int Acc_Power { get; set; }

        public int Curr_Power { get; set; }

        public int Distance { get; set; }

    }
}
