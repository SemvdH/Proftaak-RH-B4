﻿using DoctorApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Util;

namespace DoctorApp.Models
{
    class Info : ObservableObject
    {
        public bool ConnectedToServer { get; set; }
        public bool ConnectedToVREngine { get; set; }
        public bool DoctorConnected { get; set; }

        public bool CanConnectToVR { get; set; }
    }
}
