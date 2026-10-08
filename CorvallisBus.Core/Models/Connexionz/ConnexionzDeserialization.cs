using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CorvallisBus.Core.Models.Connexionz;

/// <summary></summary>
public class RoutePositionET
{
    /// <summary></summary>
    [XmlElement("Content", typeof(RoutePositionContent))]
    [XmlElement("Platform", typeof(RoutePositionPlatform))]
    public required object[] Items { get; init; }
}

/// <summary></summary>
public class RoutePositionPlatform
{
    /// <summary></summary>
    [XmlElement("Route")]
    public required RoutePositionPlatformRoute[] Route { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string PlatformTag { get; init; }
}

/// <summary></summary>
public class RoutePositionPlatformRouteDestination
{
    /// <summary></summary>
    [XmlElement("Trip")]
    public required RoutePositionPlatformRouteDestinationTrip[] Trip { get; init; }
}

/// <summary></summary>
public class RoutePositionPlatformRouteDestinationTrip
{
    /// <summary></summary>
    [XmlAttribute]
    public int ETA { get; init; }
}

/// <summary></summary>
public class RoutePositionPlatformRoute
{
    /// <summary></summary>
    [XmlElement("Destination")]
    public required RoutePositionPlatformRouteDestination[] Destination { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string RouteNo { get; init; }
}

/// <summary></summary>
public class RoutePositionContent
{
    /// <summary></summary>
    [XmlAttribute]
    public required DateTimeOffset Expires { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required int MaxArrivalScope { get; init; }
}

/// <summary></summary>
public class RoutePattern
{
    /// <summary></summary>
    [XmlElement(elementName: "Content", typeof(RoutePatternContent))]
    [XmlElement(elementName: "Project", typeof(RoutePatternProject))]
    public required object[] Items { get; init; }
}

/// <summary></summary>
public class RoutePatternContent
{
}

/// <summary></summary>
public class RoutePatternProject
{
    /// <summary></summary>
    [XmlElement("Route")]
    public required RoutePatternProjectRoute[] Route { get; init; }
}

/// <summary></summary>
public class RoutePatternProjectRoute
{
    /// <summary></summary>
    [XmlElement("Destination")]
    public required RoutePatternProjectRouteDestination[] Destination { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string RouteNo { get; init; }
}

/// <summary></summary>
public class RoutePatternProjectRouteDestination
{
    /// <summary></summary>
    public required RoutePatternProjectRouteDestinationPattern Pattern { get; init; }
}

/// <summary></summary>
public class RoutePatternProjectRouteDestinationPattern
{
    /// <summary></summary>
    public required string Coordinates { get; init; }

    /// <summary></summary>
    [XmlElement("Platform")]
    public required RoutePatternProjectRouteDestinationPatternPlatform[] Platform { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string Schedule { get; init; }
}

/// <summary></summary>
public class RoutePatternProjectRouteDestinationPatternPlatform
{
    /// <summary></summary>
    [XmlAttribute]
    public required string PlatformTag { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string ScheduleAdheranceTimepoint { get; init; }

    /// <summary></summary>
    [XmlAttribute]
    public required string PlatformNo { get; init; }
}