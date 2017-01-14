namespace Events
{
    public class EndLevelEvent : Event
    {
        public override void Fire(params object[] stuff)
        {
            GameInfo.info.LevelFinished();
        }
    }
}
