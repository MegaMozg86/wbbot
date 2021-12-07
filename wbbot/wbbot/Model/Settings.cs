using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbbot.Model
{
    public class Settings
    {
        public string PhoneNumber { get; set; } = "9511929402";
        public string Link { get; set; } = "https://seller.wildberries.ru/cmp/campaigns/list/pause/edit/search/505131";
        public int Limit { get; set; } = 10000;
        public int Freq { get; set; } = 60;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;

        public bool IsHeadless { get; set; } = false;
    }
}
