using System;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    internal class SalesProbeRow
    {
        public DateTime Date { get; set; }
        public string Variant { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Keys { get; set; }
        public int Count { get; set; }
    }
}