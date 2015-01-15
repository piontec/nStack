using System;
using Newtonsoft.Json;


namespace NStack.ObjectStorage
{
    public class ObjectInfo
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public long Bytes { get; set; }

        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }
        
        [JsonProperty(PropertyName = "last_modified")]
        public DateTimeOffset LastModified { get; set; }
    }
}