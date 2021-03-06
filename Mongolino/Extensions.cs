﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ConfigurationSection = Mongolino.Configuration.ConfigurationSection;

namespace Mongolino
{
    public static class Extensions
    {
        internal static Dictionary<string, ConfigurationSection> TypesConfiguration { get; private set; } = new Dictionary<string, ConfigurationSection>();

        public static IConfiguration AddMongolino(this IConfiguration configuration)
        {
            var section = configuration.GetSection("Mongolino");

            if (section != null && section.Exists())
            {
                TypesConfiguration = section.GetChildren()
                           .Select(sub => new
                           {
                               Type = sub.Key,
                               Section = sub.Get<ConfigurationSection>()
                           })
                          .ToDictionary(x => x.Type, x => x.Section);
            }

            return configuration;
        }
    }
}
