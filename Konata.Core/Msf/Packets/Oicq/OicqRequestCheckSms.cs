﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konata.Msf.Packets.Oicq
{
    public class OicqRequestCheckSms : OicqRequest
    {
        public OicqRequestCheckSms()
        {
            _cmd = 0x0810;
            _subCmd = 0x07;
        }
    }
}
