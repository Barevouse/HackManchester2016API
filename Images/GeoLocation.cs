using System.Device.Location;

namespace Images
{
    public static class GeoLocation
    {
        private const double MilesInMetres = 1609.344;

        public static bool WithinRadius(GeoCoordinate currentLocation, GeoCoordinate requiredLocation)
        {
            var distanceInMetres = currentLocation.GetDistanceTo(requiredLocation);
            return distanceInMetres / MilesInMetres <= 0.5;
        }
    }
}