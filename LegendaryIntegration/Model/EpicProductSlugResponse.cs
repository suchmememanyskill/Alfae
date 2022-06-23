namespace LegendaryMapperV2.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class EpicProductSlugResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("Catalog")]
        public Catalog Catalog { get; set; }
    }

    public class Catalog
    {
        [JsonProperty("catalogOffers")]
        public CatalogOffers CatalogOffers { get; set; }
    }

    public class CatalogOffers
    {
        [JsonProperty("elements")]
        public List<Element> Elements { get; set; }
    }

    public class Element
    {
        [JsonProperty("productSlug")]
        public string ProductSlug { get; set; }
    }
}
