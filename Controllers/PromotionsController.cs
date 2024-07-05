﻿using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using static System.Net.Mime.MediaTypeNames;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("Promotions")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public PromotionsController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [Route("add")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<bool>>> AddPromotion([FromForm] AddPromotionRequest request)
        {
            var response = new BaseResponse<bool>();
            using (var memoryStream = new MemoryStream())
            {
                await request.Image.CopyToAsync(memoryStream);
                var base64Image = Convert.ToBase64String(memoryStream.ToArray());

                var promotion = new Promotions
                {
                    PromotionName = request.PromotionName,
                    Discount = request.Discount,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Image = base64Image,
                };
                await _context.Promotions.AddRangeAsync(promotion);
                await _context.SaveChangesAsync();
            }
            
            response.Data = true;
            response.Success = true;
            return Ok(response);
        }
        [Route("all")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<List<PromotionResponse>>>> GetPromotionAll()
        {
            var response = new BaseResponse<List<PromotionResponse>>();
            var result = await _context.Promotions.Select(item=> new PromotionResponse(item)).ToListAsync();
            response.Data = result;
            response.Success = true;
            return Ok(response);
        }
        [Route("still-valid")]
        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<PromotionResponse>>>> GetPromotionStillValid()
        {
            var response = new BaseResponse<List<PromotionResponse>>();
            var currentDateTime = DateTime.Now;
            var result = await _context.Promotions
            .Where(p => p.StartDateTime.HasValue && p.EndDateTime.HasValue
                        && currentDateTime >= p.StartDateTime.Value
                        && currentDateTime <= p.EndDateTime.Value)
            .Select(item => new PromotionResponse(item))
            .ToListAsync();
            response.Data = result;
            response.Success = true;
            return Ok(response);
        }
    }
}
