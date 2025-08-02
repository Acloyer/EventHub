// using System.ComponentModel.DataAnnotations;

// namespace EventHub.Models.DTOs
// {
//     public class CommentDto
//     {
//         public int      Id       { get; set; }
//         public int      UserId   { get; set; }
//         [Required, MaxLength(200)]
//         public string Comment { get; set; } = null!;
//         public DateTime PostDate { get; set; }
//         public bool     IsEdited { get; set; }
//         public DateTime? EditDate{ get; set; }
//         public bool     IsPinned { get; set; }
//     }
    
//     public class CreateCommentDto
//     {
//         [Required, MaxLength(200)]
//         public string Comment { get; set; } = null!;
//     }

//     public class UpdateCommentDto
//     {
//         [Required, MaxLength(200)]
//         public string Comment { get; set; } = null!;
//     }
// }

using System;
using System.ComponentModel.DataAnnotations;
using EventHub.Models;

namespace EventHub.Models.DTOs
{
    public class CommentDto
    {
        public CommentDto() {}

        // Конструктор, если кому удобно
        // public CommentDto(EventComment c)
        // {
        //     Id         = c.Id;
        //     EventId    = c.EventId;
        //     EventTitle = c.Event?.Title ?? "";
        //     UserId     = c.UserId;
        //     UserName   = c.User?.Name ?? "";
        //     UserRole   = c.User?.Role?.Name ?? "";
        //     Comment    = c.Comment;
        //     PostDate   = c.PostDate;
        //     IsEdited   = c.IsEdited;
        //     EditDate   = c.EditDate;
        //     IsPinned   = c.IsPinned;
        //     PinnedAt   = c.PinnedAt;
        // }

         public int      Id          { get; set; }
        public int      EventId     { get; set; }
        public string   EventTitle  { get; set; } = "";
        public int      UserId      { get; set; }
        public string   UserName    { get; set; } = "";

        // ! убрали UserRole:string, добавили UserRoles:List<string>
        public List<string> UserRoles { get; set; } = new();

        [Required, MaxLength(200)]
        public string   Comment     { get; set; } = "";

        public DateTime PostDate    { get; set; }
        public bool     IsEdited    { get; set; }
        public DateTime? EditDate   { get; set; }
        public bool     IsPinned    { get; set; }
        public DateTime? PinnedAt   { get; set; }
    }

    public class CreateCommentDto
    {
        [Required, MaxLength(200)]
        public string Comment { get; set; } = null!;
    }

    public class UpdateCommentDto
    {
        [Required, MaxLength(200)]
        public string Comment { get; set; } = null!;
    }
}