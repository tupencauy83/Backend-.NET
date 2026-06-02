using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TuPenca.Application.DTOs.SportsApi
{

    public class TheSportsDbEvent
    {
        [JsonPropertyName("intHomeScore")]
        public string? IntHomeScore { get; set; }

        [JsonPropertyName("intAwayScore")]
        public string? IntAwayScore { get; set; }

        [JsonPropertyName("strStatus")]
        public string? StrStatus { get; set; }
    }
}
