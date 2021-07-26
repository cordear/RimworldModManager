using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModManager
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Tag
    {
        public string tag { get; set; }
        public bool adminonly { get; set; }
    }

    public class VoteData
    {
        public int result { get; set; }
        public int votes_up { get; set; }
        public int votes_down { get; set; }
    }

    public class Root
    {
        public int result { get; set; }
        public string publishedfileid { get; set; }
        public string creator { get; set; }
        public int creator_appid { get; set; }
        public int consumer_appid { get; set; }
        public int consumer_shortcutid { get; set; }
        public string filename { get; set; }
        public string file_size { get; set; }
        public string preview_file_size { get; set; }
        public string file_url { get; set; }
        public string preview_url { get; set; }
        public string url { get; set; }
        public string hcontent_file { get; set; }
        public string hcontent_preview { get; set; }
        public string title { get; set; }
        public string title_disk_safe { get; set; }
        public string file_description { get; set; }
        public int time_created { get; set; }
        public int time_updated { get; set; }
        public int visibility { get; set; }
        public int flags { get; set; }
        public bool workshop_file { get; set; }
        public bool workshop_accepted { get; set; }
        public bool show_subscribe_all { get; set; }
        public int num_comments_developer { get; set; }
        public int num_comments_public { get; set; }
        public bool banned { get; set; }
        public string ban_reason { get; set; }
        public string banner { get; set; }
        public bool can_be_deleted { get; set; }
        public bool incompatible { get; set; }
        public string app_name { get; set; }
        public int file_type { get; set; }
        public bool can_subscribe { get; set; }
        public int subscriptions { get; set; }
        public int favorited { get; set; }
        public int followers { get; set; }
        public int lifetime_subscriptions { get; set; }
        public int lifetime_favorited { get; set; }
        public int lifetime_followers { get; set; }
        public string lifetime_playtime { get; set; }
        public string lifetime_playtime_sessions { get; set; }
        public int views { get; set; }
        public bool spoiler_tag { get; set; }
        public int num_children { get; set; }
        public object children { get; set; }
        public int num_reports { get; set; }
        public object previews { get; set; }
        public List<Tag> tags { get; set; }
        public VoteData vote_data { get; set; }
        public int language { get; set; }
    }
}
