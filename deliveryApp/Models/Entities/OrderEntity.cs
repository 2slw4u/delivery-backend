﻿using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace deliveryApp.Models.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        [Required]
        public DateTime DeliveryTime { get; set; }
        [Required]
        public DateTime OrderTime { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public UserEntity User { get; set; }
        [NotNull]
        public Guid AddresGuid { get; set; }
    }
}
