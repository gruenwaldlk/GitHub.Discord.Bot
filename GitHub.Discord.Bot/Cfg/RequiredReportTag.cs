using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.Discord.Bot.Cfg
{
    public class RequiredReportTag
    {
        public string Tag { get; set; }
        public IEnumerable<string> AliasList { get; set; }

        public bool IsMatch(string s)
        {
            List<string> possibleTags = new(AliasList.ToList()) {Tag};
            return possibleTags.Any(possibleTag => s.Contains(possibleTag, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}