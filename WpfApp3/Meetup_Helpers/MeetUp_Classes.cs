using MeetupToRTM.MeetupHelpers;

namespace MeetupToRTM.Meetup_Helpers
{
    /// <summary>
    /// Create a main MeetUp JSON class that we can map our JSON to.
    /// </summary>
    public class MeetupJSONEventResults : MeetUp
    {
        public string How_to_find_us { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Rsvp_limit { get; set; }
        public string Status { get; set; }
        // The local date of the Meetup in ISO 8601 format
        public string Local_date { get; set; }
        // The local time of the Meetup in ISO 8601 format
        public string Local_time { get; set; }
        public long Updated { get; set; }
        public int UTC_offset { get; set; }
        public int Waitlist_count { get; set; }
        public int Yes_rsvp_count { get; set; }
        public MeetupJSONVenueResults Venue { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public bool saved { get; set; }
        public bool Pro_is_email_shared { get; set; }
    }

    /// <summary>
    /// For Venue/Event Locations
    /// </summary>
    public class MeetupJSONVenueResults : MeetUp
    {
        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Address_1 { get; set; }
        public string City { get; set; }
    }
}
