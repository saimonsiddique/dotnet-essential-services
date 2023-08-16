﻿using BaseModule.Domain.Entities;
using System.Text.Json.Serialization;

namespace ProductDirectory.Domain.Entities
{
    public class Product : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ManPower { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public ProductCostType ProductCostType { get; set; }

        public string IconUrl { get; set; } = string.Empty;

        public string BannerUrl { get; set; } = string.Empty;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProductCostType
    {
        Low,
        Medium,
        High
    }

}
