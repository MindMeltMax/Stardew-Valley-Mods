namespace UnbreakableTackles
{
    public class Config
    {
        public bool consumeBait { get; set; } = false;

        public Config() { }

        public Config(bool bait)
        {
            consumeBait = bait;
        }
    }
}
