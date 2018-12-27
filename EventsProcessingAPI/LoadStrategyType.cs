using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public enum LoadStrategyType
    {
        LoadEventsAndPayloadsForChart,
        LoadEventsForChart,
        LoadOnlyEvents,
        LoadEventsAndPayloads
    }
}
