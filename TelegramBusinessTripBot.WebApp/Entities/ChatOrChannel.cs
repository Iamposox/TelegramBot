using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace TelegramBusinessTripBot.WebApp.Entities;

public class ChatOrChannel
{
    public long ChatOrChannelId { get; set; }
    public string? Title { get; set; }
    public long? Hash { get; set; }
    public virtual ICollection<Users> Users { get; set; }
    public class ETC : IEntityTypeConfiguration<ChatOrChannel>
    {
        public void Configure(EntityTypeBuilder<ChatOrChannel> builder)
        {
            builder.HasKey(x => new { x.ChatOrChannelId, x.Hash });
        }
    }
}
