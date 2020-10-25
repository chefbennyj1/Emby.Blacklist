using System;
using System.Collections.Generic;

namespace Blacklist.Api.ReverseLookup
{
    public class Language
    {
        public string code { get; set; }
        public string name { get; set; }
        public string native { get; set; }
    }

    public class Location
    {
        public int geoname_id { get; set; }
        public string capital { get; set; }
        public List<Language> languages { get; set; }
        public string country_flag { get; set; }
        public string country_flag_emoji { get; set; }
        public string country_flag_emoji_unicode { get; set; }
        public string calling_code { get; set; }
        public bool is_eu { get; set; }
    }

    public class TimeZone
    {
        public string id { get; set; }
        public DateTime current_time { get; set; }
        public int gmt_offset { get; set; }
        public string code { get; set; }
        public bool is_daylight_saving { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
        public string plural { get; set; }
        public string symbol { get; set; }
        public string symbol_native { get; set; }
    }

    public class Connection
    {
        public int asn { get; set; }
        public string isp { get; set; }
    }

    public class Security
    {
        public bool is_proxy { get; set; }
        public object proxy_type { get; set; }
        public bool is_crawler { get; set; }
        public object crawler_name { get; set; }
        public object crawler_type { get; set; }
        public bool is_tor { get; set; }
        public string threat_level { get; set; }
        public object threat_types { get; set; }
    }

    public class ReverseLookupData
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string type { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_code { get; set; }
        public string region_name { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public Location location { get; set; }
        public TimeZone time_zone { get; set; }
        public Currency currency { get; set; }
        public Connection connection { get; set; }
        public Security security { get; set; }
    }
}