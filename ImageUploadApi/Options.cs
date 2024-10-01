using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUploadApi;

public class Options
{
    public string StorageConnectionString { get; set; }

    public string FullImageContainerName { get; set; }

    public string ThumbnailImageContainerName { get; set; }
}