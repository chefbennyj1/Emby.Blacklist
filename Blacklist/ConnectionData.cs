using System;
using System.Collections.Generic;
using Blacklist.Api.ReverseLookup;

namespace Blacklist
{
    public class ConnectionData
    {
        public string Ip                          { get; set; }
        public List<DateTime> FailedAuthDateTimes { get; set; }
        public bool IsBanned                      { get; set; }
        public DateTime BannedDateTime            { get; set; }
        public string RuleName                    { get; set; }
        public int LoginAttempts                  { get; set; }
        public string Id                          { get; set; }
        public ReverseLookupData LookupData       { get; set; }
        public string deviceId                    { get; set; }
    }
}