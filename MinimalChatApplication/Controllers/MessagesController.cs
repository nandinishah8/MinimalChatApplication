using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data;
using MinimalChatApplication.Models;

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
        [HttpGet("{id}")]
        public async Task<ActionResult> GetConversationHistory(int id)
        {
            Console.WriteLine("log"+id);
            var currentUser = HttpContext.User;

            var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Console.WriteLine("courrent"+currentUserId);
            if (currentUserId == id)
            {
                return BadRequest(new { error = "You cannot retrieve your own conversation history." });
            }

            var conversation = _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id) ||
                            (m.SenderId == id && m.ReceiverId == currentUserId));

            Console.WriteLine(conversation);
             
            // Check if the conversation exists
            if (!conversation.Any())
            {
                return NotFound(new { error = "Conversation not found" });
            }

            // Apply filters if provided
            //if (request.Before.HasValue)
            //{
            //    conversation = conversation.Where(m => m.Timestamp < request.Before);
            //}

            //// Apply sorting
            //if (request.Sort.ToLower() == "desc")
            //{
            //    conversation = conversation.OrderByDescending(m => m.Timestamp);
            //}
            //else
            //{
            //    conversation = conversation.OrderBy(m => m.Timestamp);
            //}

            // Limit the number of messages to be retrieved
            //conversation = conversation.Take(request.Count);

            // Select only the required properties for the response and map to the DTO
            var messages = conversation.Select(m => new ConversationResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                Timestamp = m.Timestamp
            });

            //.ToListAsync();

            return Ok(new ConversationHistoryResponseDto { Messages = messages });

        }

        //// GET: api/Messages/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Message>> GetMessage(int id)
        //{
        //    var message = await _context.Messages.FindAsync(id);

        //    if (message == null)
        //    {
        //        return NotFound();
        //    }

        //    return message;
        //}

        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            var userId = GetCurrentUserId();

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
        

        [HttpPost]
        public async Task<ActionResult<sendMessageResponse>> PostMessage(sendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "message sending failed due to validation errors." });
            }
            var senderId = GetCurrentUserId();

            // Create a new Message object based on the request data
            var message = new Message
            {
                SenderId = senderId,

                Content = request.Content,
                ReceiverId = request.ReceiverId,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();


            // Return a SendMessageResponse with the relevant message data
            var response = new sendMessageResponse
            {
                MessageId = message.Id,
                SenderId = senderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            };

            return Ok(response);
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
            int userId = GetCurrentUserId();
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

        private int GetCurrentUserId()
        {
            var currentUser = HttpContext.User;
            var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return currentUserId;
        }
    }
}
