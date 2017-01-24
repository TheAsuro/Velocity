using Game;
using UI.MenuWindows;
using UnityEngine;

namespace Console
{
    public class FrictionCommand : FloatConsoleCommand
    {
        public FrictionCommand(ConsoleWindow window) : base(window, "move_friction") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.friction = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.friction.ToString();
        }
    }

    public class AcclerationCommand : FloatConsoleCommand
    {
        public AcclerationCommand(ConsoleWindow window) : base(window, "move_accel") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.accel = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.accel.ToString();
        }
    }

    public class AirAcclerationCommand : FloatConsoleCommand
    {
        public AirAcclerationCommand(ConsoleWindow window) : base(window, "move_airaccel") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.airAccel = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.airAccel.ToString();
        }
    }

    public class MaxSpeedCommand : FloatConsoleCommand
    {
        public MaxSpeedCommand(ConsoleWindow window) : base(window, "move_maxspeed") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.maxSpeed = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.maxSpeed.ToString();
        }
    }

    public class MaxAirSpeedCommand : FloatConsoleCommand
    {
        public MaxAirSpeedCommand(ConsoleWindow window) : base(window, "move_maxairspeed") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.maxAirSpeed = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.maxAirSpeed.ToString();
        }
    }

    public class JumpHeightCommand : FloatConsoleCommand
    {
        public JumpHeightCommand(ConsoleWindow window) : base(window, "move_jumpforce") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.jumpForce = value;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.jumpForce.ToString();
        }
    }

    public class GravityCommand : FloatConsoleCommand
    {
        public GravityCommand(ConsoleWindow window) : base(window, "move_gravity") { }

        protected override void RunWithValue(float value)
        {
            if (CheatCheck())
                Physics.gravity = new Vector3(0f, value, 0f);
        }

        protected override string PrintStatus()
        {
            return Physics.gravity.y.ToString();
        }
    }

    public class NoclipCommand : IntConsoleCommand
    {
        public NoclipCommand(ConsoleWindow window) : base(window, "move_gravity") { }

        protected override void RunWithValue(int value)
        {
            if (CheatCheck())
                WorldInfo.info.RaceScript.Movement.Noclip = value != 0;
        }

        protected override string PrintStatus()
        {
            return WorldInfo.info.RaceScript.Movement.Noclip.ToString();
        }
    }
}