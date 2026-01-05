using DataPlatform.Models;
using DataPlatform.Shared.DataAccess;
using DataPlatform.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataPlatform.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        [HttpGet("devices")]
        public IActionResult GetMyDevices()
        {
            var devices = new[]
            {
            new { id = "WS-1001", name = "采样器-市南" },
            new { id = "WS-1002", name = "采样器-市北" }
        };

            return Ok(devices);
        }
    }
}