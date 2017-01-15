using UI;

namespace Events
{
    public class EndLevelEvent : Event
    {
        public override void Fire(params object[] stuff)
        {
            GameMenu.SingletonInstance.AddWindow(Window.END_LEVEL);
            GameInfo.info.LevelFinished();
        }
    }
}
