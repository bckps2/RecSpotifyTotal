namespace ConsoleApp1.InformationTracks.Dto
{
    public class InformationDeezerTrackDto
    {
        public long id { get; set; }
        public bool readable { get; set; }
        public string title { get; set; }
        public string title_short { get; set; }
        public string title_version { get; set; }
        public string isrc { get; set; }
        public string link { get; set; }
        public string share { get; set; }
        public int duration { get; set; }
        public int track_position { get; set; }
        public int disk_number { get; set; }
        public long rank { get; set; }
        public string release_date { get; set; }
        public bool explicit_lyrics { get; set; }
        public int explicit_content_lyrics { get; set; }
        public int explicit_content_cover { get; set; }
        public string preview { get; set; }
        public double bpm { get; set; }
        public double gain { get; set; }
        public List<InformationDto> contributors { get; set; }
        public string md5_image { get; set; }
        public InformationDto artist { get; set; }
        public AlbumDto album { get; set; }
        public string type { get; set; }
    }
}
