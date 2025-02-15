namespace CS2Tags_VipTag;
public partial class CS2Tags_VipTag
{
    public class Player
    {
        public required ulong steamid { get; set; }
        public required string tag { get; set; }
        public string? tagcolor { get; set; }
        public string? namecolor { get; set; }
        public string? chatcolor { get; set; }
        public bool? visibility {get; set; }
    }
}