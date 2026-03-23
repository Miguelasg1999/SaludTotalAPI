using System;
using Microsoft.AspNetCore.Mvc;

namespace SaludTotalAPI.Constants;

public class CacheProfiles
{
    public const string Default60 = "Default60";

    public static readonly CacheProfile Profile60 = new()
    {
        Duration = 60
    };
}
