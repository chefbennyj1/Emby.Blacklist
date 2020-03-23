using System;
using System.Collections.Generic;

namespace Blacklist
{
    public class Connection
    {

        public string Ip                                         { get; set; }
        public List<DateTime> FailAuthenticationRequestDateTimes { get; set; }
        public bool IsBanned                                     { get; set; }
        public DateTime BannedDateTime                           { get; set; }
        public string RuleName                                   { get; set; }
        public int LoginAttempts                                 { get; set; }
        public string Id                                         { get; set; }
        
    }
}
