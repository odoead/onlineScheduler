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

        private readonly IChatService _chatService;
        private readonly IGroupChatService _groupChatService;
        private readonly IUserConnectionService _userConnectionService;

        public ChatsController(IChatService chatService, IGroupChatService groupChatService, IUserConnectionService userConnectionService)
        {
            _chatService = chatService;
            _groupChatService = groupChatService;
            _userConnectionService = userConnectionService;
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = await _chatService.GetChatHistoryAsync(currentUserId, userId, limit, offset);
            return Ok(messages);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupChatHistory(string groupId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is in the group
            var group = await _groupChatService.GetGroupAsync(groupId);
            var members = await _groupChatService.GetGroupMemberIdsAsync(groupId);

            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            var messages = await _chatService.GetGroupChatHistoryAsync(groupId, limit, offset);
            return Ok(messages);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetUserGroups()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groups = await _groupChatService.GetUserGroupsAsync(currentUserId);
            return Ok(groups);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await _groupChatService.CreateGroupAsync(dto.Name, dto.Description, currentUserId);
            return Ok(group);
        }

        [HttpPost("groups/{groupId}/members")]
        public async Task<IActionResult> AddGroupMember(string groupId, [FromBody] AddMemberDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is admin of the group
            var group = await _groupChatService.GetGroupAsync(groupId);
            var members = await _groupChatService.GetGroupMemberIdsAsync(groupId);

            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            await _groupChatService.AddMemberToGroupAsync(groupId, dto.UserId, dto.IsAdmin);
            return Ok();
        }

        [HttpDelete("groups/{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveGroupMember(string groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is admin of the group
            var group = await _groupChatService.GetGroupAsync(groupId);
            var members = await _groupChatService.GetGroupMemberIdsAsync(groupId);

            if (!members.Contains(currentUserId))
            {
                return Forbid();
            }

            await _groupChatService.RemoveMemberFromGroupAsync(groupId, userId);
            return Ok();
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadCounts()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var counts = await _chatService.GetAllUnreadCountsAsync(currentUserId);
            return Ok(counts);
        }

        [HttpGet("online/{userId}")]
        public async Task<IActionResult> IsUserOnline(string userId)
        {
            var isOnline = await _userConnectionService.IsUserOnlineAsync(userId);
            return Ok(new { isOnline });
        }
    }




}

