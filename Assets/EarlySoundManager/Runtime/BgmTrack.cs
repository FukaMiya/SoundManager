namespace Early.SoundManager
{
    public readonly struct BgmTrackId
    {
        public readonly string Value;
        
        public BgmTrackId(string value) => Value = value;
        
        public static readonly BgmTrackId Main = new("Main");
        public static readonly BgmTrackId Ambient = new("Ambient");
    }

    internal sealed class BgmTrackState
    {
        public IBgmHandle Current { get; set; }
        public IBgmHandle Next { get; set; }

        public BgmTrackState(IBgmHandle current)
        {
            Current = current;
            Next = null;
        }
    }
}