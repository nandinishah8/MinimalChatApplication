using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data;
using MinimalChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MinimalChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MinimalChatContext _context;

        public MessagesController(MinimalChatContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<ActionResult<ConversationHistoryResponseDto>> GetConversationHistory([FromQuery] ConversationRequest request)
        {
            int userId = request.UserId;

            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            // Retrieve the conversation history based on the provided parameters
            var conversationHistory = await _context.Messages
                .Where(m => (m.ReceiverId == userId && m.ReceiverId == request.UserId)
                            || (m.SenderId == request.UserId && m.SenderId == userId))
                .Where(m => request.Before == null || m.Timestamp < request.Before)
                .OrderByDescending(m => request.Sort == "desc" ? m.Timestamp : m.Timestamp)
                .Take(request.Count > 0 ? request.Count : 20)
                .Select(m => new ConversationResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                })
                .ToListAsync();

            if (conversationHistory.Count == 0)
            {
                return NotFound(new { message = "User or conversation not found" });
            }

            var responseDto = new ConversationHistoryResponseDto
            {
                Messages = conversationHistory
            };

            return Ok(responseDto);

            


        }

        // GET: api/Messages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            var userId = GetUserId(HttpContext);

            if (userId == -1)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "invalid request parameter." });
            }

            var existingMessage = await _context.Messages.FindAsync(id);

            Console.WriteLine(existingMessage);

            if (existingMessage == null)
            {
                return NotFound(new { error = "Message not found." });
            }

            if (message == null)
            {
                return NotFound(new { message = "User or conversation not found" });
            }

            existingMessage.Content = message.Content;
            await _context.SaveChangesAsync();

            return Ok(message);
        }



        // POST: api/Messages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "message sending failed due to validation errors." });
            }

            int userId = GetUserId(HttpContext);

            if (userId == -1)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            message.SenderId = userId;
            message.Timestamp = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, message);
        }


        // DELETE: api/Messages/5
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound(new { message = "Message not found" });
            }

            // Check if the user is authorized to delete the message (should be the sender)
            int userId = GetUserId(HttpContext);
            if (message.SenderId != userId)
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted successfully" });
        }
    

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }

        private int GetUserId(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            var token = authorizationHeader?.Replace("Bearer ", "");

            var user = _context.Users.FirstOrDefault(u => u.Token == token);

            return user?.Id ?? -1;
        } 

    }
}

