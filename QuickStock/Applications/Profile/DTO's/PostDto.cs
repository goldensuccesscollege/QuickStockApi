using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace QuickStock.Applications.Profile.DTO_s
{
    public class CreatePostDto
    {
        public string? Content { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string AuthorProfileImage { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string AuthorProfileImage { get; set; } = string.Empty;
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Images { get; set; } = new();
        
        // Social metadata
        public int ReactionsCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<CommentDto> Comments { get; set; } = new();
    }
}
