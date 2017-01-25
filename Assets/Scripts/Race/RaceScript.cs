﻿using System;

namespace Race
{
    public interface RaceScript
    {
        event EventHandler OnStart;
        event EventHandler OnReset;

        MovementBehaviour Movement { get; }
        bool RunVaild { get; }
        float UnfreezeTime { get; }
        TimeSpan ElapsedTime { get; }

        void Pause();
        void Unpause();
        void PrepareNewRace();
    }
}