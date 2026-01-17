using System;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public sealed class SalesProbeRow
    {
        public DateTime Date { get; set; }
        public string Variant { get; set; }      // "UTC" or "Local"
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Keys { get; set; }
        public int Count { get; set; }
    }
}
