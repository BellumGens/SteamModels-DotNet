﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamModels.CSGO
{
    /// <summary>
    /// View Model describing player statistics in CS:GO
    /// GET: http://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v0002/?appid=730&key=<API_KEY>&steamid=<STEAMID64>&format=json
    /// </summary>
    public class CSGOPlayerStats : SteamUserStats
    {
        private decimal _killDeathRatio = 0;
        private decimal _headshotPercentage = 0;
        private decimal _accuracy = 0;
        private List<WeaponDescriptor> _weapons;
		private WeaponDescriptor _favWeapon;

        /// <summary>
        /// The stat names
        /// </summary>
        //private static Dictionary<string, string> _statNames = new Dictionary<string, string>()
        //{
        //    { "total_kills", "Total Kills" },
        //    { "total_deaths", "Total Deaths" },
        //    { "total_kills_headshot", "Total Headshots" },
        //    { "total_shots_fired", "Total Shots" },
        //    { "total_shots_hit", "Total Hits" }
        //};

        /// <summary>
        /// Gets the player kill death ratio, if total kills and total deaths have been populated. Otherwise -1 is returned.
        /// </summary>
        /// <value>
        /// The kill death ratio, if total kills and total deaths have been populated. Otherwise -1 is returned.
        /// </value>
        public decimal killDeathRatio
        {
            get
            {
                decimal kills = 0, deaths = 1;
                if (_killDeathRatio == 0)
                {
                    foreach (StatDescriptor stat in playerstats.stats)
                    {
                        if (stat.name == "total_kills")
                            kills = stat.value;
                        if (stat.name == "total_deaths")
                            deaths = stat.value;
                    }
                    _killDeathRatio = kills / deaths;
                }
                return Math.Round(_killDeathRatio, 2);
            }
        }

        /// <summary>
        /// Gets or sets the headshot percentage.
        /// </summary>
        /// <value>
        /// The headshot percentage.
        /// </value>
        public decimal headshotPercentage
        {
            get
            {
                decimal kills = 1, headshots = 0;
                if (_headshotPercentage == 0)
                {
                    foreach (StatDescriptor stat in playerstats.stats)
                    {
                        if (stat.name == "total_kills")
                            kills = stat.value;
                        if (stat.name == "total_kills_headshot")
                            headshots = stat.value;
                    }
                    _headshotPercentage = headshots / kills * 100;
                }
                return Math.Round(_headshotPercentage, 2);
            }    
        }

        /// <summary>
        /// Gets the overal accuracy percentage.
        /// </summary>
        /// <value>
        /// The overal accuracy percentage.
        /// </value>
        public decimal accuracy
        {
            get
            {
                decimal shots = 1, hits = 0;
                if (_accuracy == 0)
                {
                    foreach (StatDescriptor stat in playerstats.stats)
                    {
                        if (stat.name == "total_shots_fired")
                            shots = stat.value;
                        if (stat.name == "total_shots_hit")
                            hits = stat.value;
                    }
                    _accuracy = hits / shots * 100;
                }
                return Math.Round(_accuracy, 2);
            }
        }

        public List<WeaponDescriptor> weapons {
            get
            {
                if (_weapons == null)
                {
                    _weapons = new List<WeaponDescriptor>();
                    List<StatDescriptor> stats = playerstats.stats.Where(s => s.name.StartsWith("total_kills_")).ToList();
                    foreach (StatDescriptor stat in stats)
                    {
                        StatDescriptor shots = playerstats.stats.Where(s => s.name == stat.name.Replace("kills", "shots")).SingleOrDefault();
                        StatDescriptor hits = playerstats.stats.Where(s => s.name == stat.name.Replace("kills", "hits")).SingleOrDefault();
                        _weapons.Add(new WeaponDescriptor()
                        {
                            name = stat.name.Replace("total_kills_", ""),
                            kills = stat.value,
                            shots = shots != null ? shots.value : 1,
                            hits = hits != null ? hits.value : 0
                        });
                    }
                }
                return _weapons;
            }
        }

        /// <summary>
        /// Gets the favourite weapon.
        /// </summary>
        /// <value>
        /// The favourite weapon.
        /// </value>
        public WeaponDescriptor favouriteWeapon
        {
            get
            {
				if (_favWeapon == null && weapons != null)
				{
					_favWeapon = weapons.Where(w => !w.name.Contains("headshot") &&
													!w.name.Contains("enemy_weapon") &&
													!w.name.Contains("zoomed_sniper") &&
													!w.name.Contains("enemy_blinded") &&
													!w.name.Contains("knife_fight")).OrderByDescending(w => w.kills).FirstOrDefault();
				}
                return _favWeapon;
            }
        }
    }

    public class WeaponDescriptor
    {
        public string name { get; set; }
        public int kills { get; set; }
        public int shots { get; set; }
        public int hits { get; set; }

        private decimal _accuracy = 0;

        public decimal accuracy
        {
            get
            {
                if (_accuracy == 0)
                {
                    _accuracy = (decimal)hits / shots * 100;
                }
                return Math.Round(_accuracy, 2);
            }
        }
    }
}
