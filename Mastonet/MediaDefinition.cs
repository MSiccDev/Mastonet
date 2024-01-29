using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mastonet;

public class MediaDefinition
{
    //public MediaDefinition(Stream media, string fileName)
    public MediaDefinition(byte[] data, string fileName)
    {
        //this.Media = media ?? throw new ArgumentException("All the params must be defined", nameof(media));
        this.Data = data ?? throw new ArgumentException("All the params must be defined", nameof(data));
        this.FileName = fileName ?? throw new ArgumentException("All the params must be defined", nameof(fileName));
    }

    //public Stream Media { get; set; }
    
    public byte[] Data { get; set; }

    public string FileName { get; set; }

    internal string? ParamName { get; set; }
}
