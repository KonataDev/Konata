﻿using System;

namespace Konata.Core.Event
{
    public class PullTroopListEvent : ProtocolEvent
    {
        /// <summary>
        /// <b>[In]</b>          <br/>
        ///   Self uin.          <br/>
        /// </summary>
        public uint SelfUin { get; set; }
    }
}
