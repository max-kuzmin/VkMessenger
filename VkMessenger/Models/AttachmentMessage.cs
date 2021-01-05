using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public sealed class AttachmentMessage
    {
        public Profile? Profile { get; set; }
        public string Text { get; set; } = default!;
    }
}
