﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mango.Services.OrderAPI.Models.DTO
{
    public class OrderDetailsDTO
    {

        public int OrderDetailsId { get; set; }

        public int OrderHeaderId { get; set; }


        public int ProductId { get; set; }


        public ProductDTO? Product { get; set; }

        public int Count { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }
    }
}
