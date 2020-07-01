﻿using System.Collections.Generic;
using TheXDS.MCART.Types.Base;
using TheXDS.Triton.Models.Base;

namespace TheXDS.Triton.Examples.BookstoreExample.Models
{
    public class Author : Model<int>, INameable
    {
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? Picture { get; set; }
        public ICollection<BookAuthor> Books { get; set; } = new List<BookAuthor>();
    }
}
