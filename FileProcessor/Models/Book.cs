using Newtonsoft.Json;
using System;

namespace FileProcessor.Models;

public class Book
{
    [JsonProperty("@id")]
    public string Id { get; set; }
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("genre")]
    public string Genre { get; set; }
    [JsonProperty("price")]
    public decimal Price { get; set; }
    [JsonProperty("publish_date")]
    public DateTime PublishDate { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
}