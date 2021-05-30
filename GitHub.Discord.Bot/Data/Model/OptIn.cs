using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace GitHub.Discord.Bot.Data.Model
{
    [Table("GDB_OPT_IN")]
    public class OptIn
    {
        [Column("OPT_IN_NR"), Required, Key]
        public long OptInId { get; set; }
        
        [Column("EVT_CREATED"), Required]
        public DateTime EvtCreated { get; set; }
        
        [Column("OPT_IN_STATUS"), Required]
        public OptInStatus OptInStatus { get; set; }
        
        [Column("USER_NR"), Required]
        public long UserId { get; set; }
        
        public User User { get; set; }

        public OptIn(DateTime evtCreated, OptInStatus optInStatus, long userId)
        {
            EvtCreated = evtCreated;
            OptInStatus = optInStatus;
            UserId = userId;
        }

        public class OptInValidator : AbstractValidator<OptIn>
        {
            public OptInValidator()
            {
                RuleFor(optIn => optIn.EvtCreated).NotNull().NotEmpty();
                RuleFor(optIn => optIn.OptInStatus).NotNull().IsInEnum();
                RuleFor(optIn => optIn.UserId).NotEqual(0);
            }
        }
    }
}