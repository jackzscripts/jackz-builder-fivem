using System.Collections.Generic;

namespace jackz_builder.Client.JackzBuilder
{
    public class DataEntry
    {
        public readonly string Id;
        public readonly string Name;

        public DataEntry(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    public class Data
    {
        public static readonly List<string> CuratedProps = new List<string>
        {
            {"prop_barriercrash_04"},
            {"prop_barier_conc_01a"},
            {"prop_barier_conc_01b"},
            {"prop_barier_conc_03a"},
            {"prop_barier_conc_02c"},
            {"prop_mc_conc_barrier_01"},
            {"prop_barier_conc_05b"},
            {"prop_metal_plates01"},
            {"prop_metal_plates02"},
            {"prop_woodpile_01a"},
            {"prop_weed_pallet"},
            {"prop_cs_dildo_01"},
            {"prop_water_ramp_03"},
            {"prop_water_ramp_02"},
            {"prop_mp_ramp_02"},
            {"prop_mp_ramp_01_tu"},
            {"prop_roadcone02a"},
            {"prop_sign_road_03b"},
            {"prop_prlg_snowpile"},
            {"prop_logpile_06b"},
            {"prop_windmill_01"},
            {"prop_cactus_01e"},
            {"prop_minigun_01"},
            {"v_ilev_gold"},
            {"bkr_prop_bkr_cashpile_07"},
            {"ex_cash_pile_07"},
            {"prop_cs_dildo_01"},
            {"prop_ld_bomb_01"}
        };

        public static readonly Dictionary<string, string> CuratedVehicles = new Dictionary<string, string>()
        {
            { "t20", "T20" },
            { "vigilante", "Vigilante" },
            { "oppressor", "Oppressor" },
            { "frogger", "Frogger" },
            { "airbus", "Airport Bus" },
            { "pbus2", "Festival Bus" },
            { "hydra", "Hydra" },
            { "blimp", "Blimp" },
            { "rhino", "Rhino Tank" },
            { "cerberus2", "Future Shock Cerberus" },
            { "mule", "Mule" },
            { "bmx", "BMX" },
            { "ambulance", "Ambulance" },
            { "police3", "Police Crusier 3" },
            { "predator", "Police Boat" },
            { "polmav", "Police Maverick Helicopter" },
            { "bati", "Bati" },
            { "airtug", "Airtug" },
            { "armytrailer", "Army Trailer (Flatbed)" },
            { "armytanker", "Army Tanker" },
            { "freightcont2", "Train Freight Car" }
        };

        public static readonly Dictionary<string, string> CuratedPeds = new Dictionary<string, string>()
        {
            { "player_one", "Franklin" },
            { "player_two", "Trevor" },
            { "player_zero", "Michael" },
            { "hc_driver", null },
            { "hc_gunman", null },
            { "hc_hacker", null },
            { "ig_agent", null },
            { "ig_amanda_townley", "Amanda" },
            { "ig_andreas", null },
            { "ig_ashley", null },
            { "ig_avon", "Avon" },
            { "ig_brad", "Brad" },
            { "ig_chef", "Chef" },
            { "ig_devin", "Devin" },
            { "ig_tomcasino", "Tom" },
            { "ig_agatha", "Agtha" },
            { "s_f_y_cop_01", "Female Cop" },
            { "s_m_m_fibsec_01", "Fib Agent (M)" },
            { "s_m_m_movspace_01", "Spacesuit Ped" },
            { "s_m_m_scientist_01", "Scientist" },
            { "s_m_y_clown_01", "Clown" },
            { "ig_nervousron", "Nervous Ron" },
            { "ig_wade", "Wade" },
            { "u_f_y_corpse_01", "Corpse" },
            { "u_m_m_jesus_01", "Jesus" },
            { "u_m_m_streetart_01", "Monkey Mask" },
            { "u_m_y_rsranger_01", "Space Ranger" },
            { "a_c_deer", "Deer" },
            { "s_m_y_prisoner_01", "Prisoner" },
            { "s_m_y_sheriff_01", "Sherrif" },
            { "s_m_y_fireman_01", "Fireman" }
        };

        public static readonly List<DataEntry> CuratedParticles = new List<DataEntry>()
        {
            { new DataEntry("scr_indep_fireworks", "scr_indep_firework_shotburst") },
            { new DataEntry("core", "fire_wrecked_plane_cockpit") },
            { new DataEntry("wpn_flare", "proj_heist_flare_trail") },
            { new DataEntry("weap_xs_vehicle_weapons", "muz_xs_turret_flamethrower_looping") },
            { new DataEntry("weap_xs_vehicle_weapons", "muz_xs_turret_flamethrower_looping_sf") },
            { new DataEntry("weap_sm_tula", "veh_tula_turbulance_water") },
            { new DataEntry("veh_khanjali", "muz_xm_khanjali_railgun_charge") },
            { new DataEntry("scr_xs_props", "scr_xs_oil_jack_fire") },
            { new DataEntry("scr_xs_pits", "scr_xs_sf_pit") },
            { new DataEntry("scr_xs_pits", "scr_xs_fire_pit") },
            { new DataEntry("scr_xs_pits", "scr_xs_sf_pit_long") },
            { new DataEntry("scr_xs_pits", "scr_xs_fire_pit_long") },
            { new DataEntry("xcr_xs_celebration", "scr_xs_money_rain") },
            { new DataEntry("xcr_xs_celebration", "scr_xs_money_rain_celeb") },
            { new DataEntry("xcr_xs_celebration", "scr_xs_champagne_spray") },
            { new DataEntry("xcr_xm_submarine", "scr_xm_stromberg_scanner") },
            { new DataEntry("xcr_xm_spybomb", "scr_xm_spybomb_plane_smoke_trail") },
            { new DataEntry("scr_xm_ht", "scr_xm_ht_package_flare") },
            { new DataEntry("scr_xm_farm", "scr_xm_dst_elec_cracke") },
            { new DataEntry("scr_xm_heat", "scr_xm_heat_camo") },
            { new DataEntry("scr_xm_aq", "scr_xm_aq_final_kill_thruster") },
            { new DataEntry("scr_sr_adversary", "scr_sr_lg_weapon_highlight") },
            { new DataEntry("scr_recrash_rescue", "scr_recrash_rescue") },
            { new DataEntry("scr_reconstructionaccident", "scr_sparking_generator") },
            { new DataEntry("scr_rcnigel2", "scr_rcn2_debris_trail") },
            { new DataEntry("scr_rcbarry1", "scr_alien_charging") },
            { new DataEntry("scr_rcbarry1", "scr_alien_impact") },
            { new DataEntry("scr_jewelheist", "scr_jewel_fog_volume") },
            { new DataEntry("scr_carwash", "ent_amb_car_wash_jet") },
            { new DataEntry("scr_as_trans", "scr_as_trans_smoke") },
            { new DataEntry("cut_amb_tv", "cs_amb_tv_sauna_steam") },
            { new DataEntry("scr_trevor2", "scr_trev2_heli_wreck") },
            { new DataEntry("scr_stunts", "scr_stunts_fire_ring") }
        };

    }
}