using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data;
using MinimalChatApplication.Models;

namespace MinimalChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly MinimalChatContext _context;

        public LogController(MinimalChatContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Logs>>> GetLogs()
        {
            var logs = _context.Log.Select(u => new
            {
                Id = u.Id,
                Ip = u.IP,
                Username = u.Username,
                RequestBody = u.RequestBody.Replace("\n", "").Replace("\"", "").Replace("\r", ""),
                TimeStamp = u.Timestamp,
            });

            if (logs == null)
            {
                return NotFound(new { message = "Logs not found" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            return Ok(logs);


        }


    }
}
