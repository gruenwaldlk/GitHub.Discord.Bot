using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace GitHub.Discord.Bot.Data.Model
{
    [Table("GDB_USER")]
    public class User
    {
        [Column("USER_NR"), Required, Key]
        public long UserId { get; set; }
        
        [Column("DISCORD_USER_NR"), Required]
        public long DiscordUserNr { get; set; }

        [Column("DISCORD_USER_NAME"), Required, Comment("The discord username with discriminator, e.g. My_Name#1234")]
        public string DiscordUserName { get; set; }

        public OptIn OptIn { get; set; }
        
        public User(long discordUserNr, string discordUserName)
        {
            DiscordUserNr = discordUserNr;
            DiscordUserName = discordUserName;
        }
        
        public class UserValidator : AbstractValidator<User>
        {
            public UserValidator()
            {
                RuleFor(user => user.DiscordUserNr).NotEqual(0);
                RuleFor(user => user.DiscordUserName).NotEmpty().Matches(new Regex("^(.*)(#)([0-9]{4})$"));
            }
        }
    }
}