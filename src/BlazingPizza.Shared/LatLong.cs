namespace BlazingPizza
{
    public class LatLong
    {
        public LatLong()
        {
        }

        public LatLong(double latitude, double longitude) : this()
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
