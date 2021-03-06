﻿using System;
using System.Collections.Generic;

namespace Blacklist
{
    public class Connection
    {
        public string Ip                          { get; set; }
        public string Isp                         { get; set; }
        public string DeviceName                  { get; set; }
        public string FlagIconUrl                 { get; set; }
        public string UserAccountName             { get; set; }
        public List<DateTime> FailedAuthDateTimes { get; set; }
        public bool IsBanned                      { get; set; }
        public DateTime BannedDateTime            { get; set; }
        public string RuleName                    { get; set; }
        public int LoginAttempts                  { get; set; }
        public string Id                          { get; set; }
        public bool Proxy                         { get; set; }
        public string ServiceProvider             { get; set; }
        public double Longitude                   { get; set; }
        public double Latitude                    { get; set; }
        public string Region                      { get; set; }

    }
}