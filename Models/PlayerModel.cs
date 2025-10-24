using System.Text.Json.Serialization;

namespace CS2Tags_VipTag.Models
{
    public class PlayerModel
    {
        public required ulong steamid { get; set; }
        public required string tag { get; set; }
        public string? tagcolor { get; set; }
        public string? namecolor { get; set; }
        public string? chatcolor { get; set; }
        public bool? visibility { get; set; }
        public bool? chatvisibility { get; set; }
        public bool? scorevisibility { get; set; }
    }
}