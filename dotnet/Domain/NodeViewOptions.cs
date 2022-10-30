using System;

namespace NodeView.Domain;

public class NodeViewOptions
{
    public static readonly string ConfigSectionName = "NodeView";

    public string ApiEndpoint { get; set; }
    public string ApiToken { get; set; }
}
