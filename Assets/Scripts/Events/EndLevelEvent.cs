using UI;

namespace Events
{
    public class EndLevelEvent : Event
    {
        public override void Fire(params object[] stuff)
        {
            GameMenu.SingletonInstance.AddWindow(Window.ENDLEVEL);
            GameInfo.info.LevelFinished();
        }
    }
}
