using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TuPenca.Application.DTOs.SportsApi
{


    public class TheSportsDbResponse
    {
        [JsonPropertyName("events")]
        public List<TheSportsDbEvent>? Events { get; set; }
    }
}
