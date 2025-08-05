using ChatService.DTO;
using ChatService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatService.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatsController : Controller
    {

        private readonly IChatService chatService;
        private readonly IGroupChatService groupChatService;
        private readonly IUserConnectionService userConnectionService;

        public ChatsController(IChatService chatService, IGroupChatService groupChatService, IUserConnectionService userConnectionService)
        {
            this.chatService = chatService;
            this.groupChatService = groupChatService;
            this.userConnectionService = userConnectionService;
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = await chatService.GetChatHistoryAsync(currentUserId, userId, limit, offset);
            return Ok(messages);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupChatHistory(string groupId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is in the group
            var group = await groupChatService.GetGroupAsync(groupId);
            if (group == null)
            {
                return Forbid();
            }

            var members = await groupChatService.GetGroupMemberIdsAsync(groupId);
            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            var messages = await chatService.GetGroupChatHistoryAsync(groupId, limit, offset);
            return Ok(messages);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetUserGroups()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groups = await groupChatService.GetUserGroupsAsync(currentUserId);
            return Ok(groups);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await groupChatService.CreateGroupAsync(dto.Name, dto.Description, currentUserId);
            return Ok(group);
        }

        [HttpPost("groups/{groupId}/members")]
        public async Task<IActionResult> AddGroupMember(string groupId, [FromBody] AddMemberDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is admin of the group
            var group = await groupChatService.GetGroupAsync(groupId);
            if (group == null)
            {
                return Forbid();
            }

            var members = await groupChatService.GetGroupMemberIdsAsync(groupId);
            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            await groupChatService.AddMemberToGroupAsync(groupId, dto.UserId, dto.IsAdmin);
            return Ok();
        }

        [HttpDelete("groups/{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveGroupMember(string groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is admin of the group
            var group = await groupChatService.GetGroupAsync(groupId);
            if (group == null)
            {
                return Forbid();
            }

            var members = await groupChatService.GetGroupMemberIdsAsync(groupId);

            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            await groupChatService.RemoveMemberFromGroupAsync(groupId, userId);
            return Ok();
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadCounts()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var counts = await chatService.GetAllUnreadCountsAsync(currentUserId);
            return Ok(counts);
        }

        [HttpGet("online/{userId}")]
        public async Task<IActionResult> IsUserOnline(string userId)
        {
            var isOnline = await userConnectionService.IsUserOnlineAsync(userId);
            return Ok(new { isOnline });
        }
    }




}

