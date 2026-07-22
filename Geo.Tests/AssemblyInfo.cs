using Xunit;

// GeoContext.Current is a mutable, process-wide ambient singleton. Tests that swap
// it (e.g. GeoContextTests toggling LongitudeWrapping) would otherwise race with any
// other test that constructs a Coordinate on a parallel collection, so run the whole
// assembly serially. The suite is small enough that the cost is negligible.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
