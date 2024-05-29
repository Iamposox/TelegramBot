using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelegramBusinessTripBot.WebApp.Entities;

public class Users
{
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public long? UserHash { get; set; }
    public string? Phone { get; set; }
    public string? FIO { get; set; }
    public bool? AdminAccount { get; set; }
    public bool? TrevelAccount { get; set; }
    public virtual ICollection<ChatOrChannel> ChatOrChannel { get; set; }
    public class ETC : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(x => new { x.UserId, x.UserHash });
        }
    }
}