namespace Breakout;

public static class AppSvg
{
    public static string GetFlag(string? lang) => (lang ?? string.Empty).ToLowerInvariant() switch
    {
        "de" => SvgFlagDe,
        "es" => SvgFlagEs,
        "fr" => SvgFlagFr,
        "it" => SvgFlagIt,
        "ru" => SvgFlagRu,
        "ja" => SvgFlagJa,
        "ko" => SvgFlagKo,
        "zh" => SvgFlagZh,
        _ => SvgFlagEn,
    };

    public const string SvgLeft = @"<svg width=""800px"" height=""800px"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
<path d=""M17.9999 12.0001V14.6701C17.9999 17.9801 15.6499 19.3401 12.7799 17.6801L10.4699 16.3401L8.15995 15.0001C5.28995 13.3401 5.28995 10.6301 8.15995 8.97005L10.4699 7.63005L12.7799 6.29005C15.6499 4.66005 17.9999 6.01005 17.9999 9.33005V12.0001Z"" stroke=""#292D32"" stroke-width=""1.5"" stroke-miterlimit=""10"" stroke-linecap=""round"" stroke-linejoin=""round""/>
</svg>";

    public const string SvgRight = @"<svg width=""800px"" height=""800px"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
<path d=""M6 11.9999V9.32992C6 6.01992 8.35 4.65992 11.22 6.31992L13.53 7.65992L15.84 8.99992C18.71 10.6599 18.71 13.3699 15.84 15.0299L13.53 16.3699L11.22 17.7099C8.35 19.3399 6 17.9899 6 14.6699V11.9999Z"" stroke=""#292D32"" stroke-width=""1.5"" stroke-miterlimit=""10"" stroke-linecap=""round"" stroke-linejoin=""round""/>
</svg>";

    public const string SvgSettings = @"<svg width=""800px"" height=""800px"" viewBox=""0 0 64 64"" id=""Layer_1"" version=""1.1"" xml:space=""preserve"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
<style type=""text/css"">
.st0{fill:url(#SVGID_1_);}.st1{fill:url(#SVGID_2_);}.st2{fill:url(#SVGID_3_);}.st3{fill:url(#SVGID_4_);}.st4{fill:url(#SVGID_5_);}.st5{fill:#FFFFFF;}.st6{opacity:0.6;fill:#FFFFFF;}.st7{fill:url(#SVGID_6_);}.st8{fill:url(#SVGID_7_);}.st9{fill:url(#SVGID_8_);}.st10{fill:url(#SVGID_9_);}.st11{fill:url(#SVGID_10_);}.st12{fill:url(#SVGID_11_);}.st13{fill:url(#SVGID_12_);}.st14{fill:url(#SVGID_13_);}.st15{fill:url(#SVGID_14_);}.st16{fill:url(#SVGID_15_);}.st17{fill:url(#SVGID_16_);}.st18{fill:url(#SVGID_17_);}.st19{fill:url(#SVGID_18_);}.st20{fill:url(#SVGID_19_);}.st21{fill:url(#SVGID_20_);}.st22{opacity:0.2;}.st23{fill:none;stroke:#FFFFFF;stroke-width:2;stroke-linecap:round;stroke-miterlimit:10;}.st24{fill:none;stroke:#FFFFFF;stroke-width:3;stroke-linecap:round;stroke-miterlimit:10;}.st25{opacity:0.5;}.st26{fill:none;stroke:#FFFFFF;stroke-width:2;stroke-linecap:round;stroke-miterlimit:10;stroke-dasharray:0.1,5;}.st27{opacity:0.6;fill:none;stroke:#FFFFFF;stroke-width:4;stroke-miterlimit:10;}.st28{opacity:0.3;}
</style>
<linearGradient gradientUnits=""userSpaceOnUse"" id=""SVGID_1_"" x1=""11.992"" x2=""52.2484"" y1=""11.9781"" y2=""52.2346"">
<stop offset=""0"" style=""stop-color:#F084C1""/><stop offset=""1"" style=""stop-color:#EB0099""/>
</linearGradient>
<path class=""st0"" d=""M57.2,60.5c-16.5,2-33.3,2-50.4,0c-1.7-0.2-3.1-1.6-3.3-3.3c-2-16.8-2-33.6,0-50.4c0.2-1.7,1.6-3.1,3.3-3.3c16.8-2,33.7-2,50.5,0c1.7,0.2,3,1.5,3.3,3.2c2,16.5,2,33.3,0,50.5C60.3,58.9,58.9,60.3,57.2,60.5z""/>
<path class=""st5"" d=""M38.5,25l2.7-2.1c0.3-0.3,0.5-0.7,0.3-1.1c-1.1-2.7-2.4-5.1-4.1-7.1c-0.3-0.3-0.7-0.4-1.1-0.3l-3.2,1.3l-1.7-1l-0.5-3.4c-0.1-0.4-0.4-0.7-0.8-0.8c-2.8-0.5-5.5-0.5-8.3,0c-0.4,0.1-0.7,0.4-0.8,0.8l-0.5,3.4l-1.7,1l-3.2-1.3c-0.4-0.1-0.8-0.1-1.1,0.2c-1.9,2-3.2,4.5-4.2,7.2c-0.1,0.4,0,0.8,0.3,1.1l2.7,2.2v1.9l-2.8,2.2c-0.3,0.2-0.5,0.7-0.3,1c0.8,2.7,2.2,5.1,4.2,7.2c0.3,0.3,0.7,0.4,1.1,0.2l3.2-1.3l1.7,1l0.5,3.5c0.1,0.4,0.3,0.7,0.7,0.8c2.8,0.7,5.6,0.7,8.4,0c0.4-0.1,0.7-0.4,0.7-0.8l0.5-3.5l1.7-1l3.2,1.3c0.4,0.1,0.8,0.1,1.1-0.2c1.8-1.9,3.2-4.4,4.1-7.2c0.1-0.4,0-0.8-0.3-1.1l-2.7-2.2V25z M26,33c-3.9,0-7.1-3.2-7.1-7.1s3.2-7.1,7.1-7.1s7.1,3.2,7.1,7.1S29.9,33,26,33z""/>
<circle class=""st6"" cx=""26"" cy=""25.9"" r=""4.1""/>
</svg>";

    public const string SvgUser = @"<svg width=""800px"" height=""800px"" viewBox=""0 0 64 64"" id=""Layer_1"" version=""1.1"" xml:space=""preserve"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
<style type=""text/css"">
.st0{fill:url(#SVGID_1_);}.st5{fill:#FFFFFF;}.st6{opacity:0.6;fill:#FFFFFF;}.st22{opacity:0.2;}
</style>
<linearGradient gradientUnits=""userSpaceOnUse"" id=""SVGID_1_"" x1=""11.992"" x2=""52.2484"" y1=""11.9781"" y2=""52.2346"">
<stop offset=""0"" style=""stop-color:#A38CF3""/><stop offset=""1"" style=""stop-color:#6B1AD6""/>
</linearGradient>
<path class=""st0"" d=""M57.2,60.5c-16.5,2-33.3,2-50.4,0c-1.7-0.2-3.1-1.6-3.3-3.3c-2-16.8-2-33.6,0-50.4c0.2-1.7,1.6-3.1,3.3-3.3c16.8-2,33.7-2,50.5,0c1.7,0.2,3,1.5,3.3,3.2c2,16.5,2,33.3,0,50.5C60.3,58.9,58.9,60.3,57.2,60.5z""/>
<path class=""st5"" d=""M32.5,37.9c-4.3-0.6-8.6-0.6-12.9,0c-2.4,0.4-4.4,2.1-5,4.5l-1.5,6.1c-0.3,1.3,0.6,2.5,1.9,2.5h22c1.3,0,2.3-1.2,1.9-2.5l-1.5-6.1C36.9,40.1,34.9,38.3,32.5,37.9z""/>
<path class=""st6"" d=""M32.3,26.3c0,3.5-2.8,8.1-6.3,8.1s-6.3-4.6-6.3-8.1S22.5,20,26,20S32.3,22.8,32.3,26.3z""/>
</svg>";

    public const string SvgStar = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 576 512"">
<path d=""M288.1 0l63.5 195.6H557.2L390.9 316.4 454.4 512 288.1 391.1 121.7 512l63.5-195.6L18.9 195.6H224.5L288.1 0z""/>
</svg>";

    public const string SvgDropdown = @"<svg viewBox=""0 0 320 512""><path d=""M320 240L160 384 0 240l0-48 320 0 0 48z""/></svg>";

    public const string SvgFlagRu = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg viewBox=""0 0 9 6"" width=""900"" height=""600""><rect fill=""#fff"" width=""9"" height=""3""/><rect fill=""#d52b1e"" y=""3"" width=""9"" height=""3""/><rect fill=""#0039a6"" y=""2"" width=""9"" height=""2""/></svg>";

    public const string SvgFlagEn = @"<?xml version=""1.0""?><svg viewBox=""0 0 60 30"" width=""1200"" height=""600""><clipPath id=""s""><path d=""M0,0 v30 h60 v-30 z""/></clipPath><clipPath id=""t""><path d=""M30,15 h30 v15 z v15 h-30 z h-30 v-15 z v-15 h30 z""/></clipPath><g clip-path=""url(#s)""><path d=""M0,0 v30 h60 v-30 z"" fill=""#012169""/><path d=""M0,0 L60,30 M60,0 L0,30"" stroke=""#fff"" stroke-width=""6""/><path d=""M0,0 L60,30 M60,0 L0,30"" clip-path=""url(#t)"" stroke=""#C8102E"" stroke-width=""4""/><path d=""M30,0 v30 M0,15 h60"" stroke=""#fff"" stroke-width=""10""/><path d=""M30,0 v30 M0,15 h60"" stroke=""#C8102E"" stroke-width=""6""/></g></svg>";

    public const string SvgFlagFr = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg width=""900"" height=""600""><rect width=""900"" height=""600"" fill=""#ED2939""/><rect width=""600"" height=""600"" fill=""#fff""/><rect width=""300"" height=""600"" fill=""#002395""/></svg>";

    public const string SvgFlagDe = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg width=""1000"" height=""600"" viewBox=""0 0 5 3""><rect width=""5"" height=""3"" y=""0"" x=""0"" fill=""#000""/><rect width=""5"" height=""2"" y=""1"" x=""0"" fill=""#D00""/><rect width=""5"" height=""1"" y=""2"" x=""0"" fill=""#FFCE00""/></svg>";

    public const string SvgFlagIt = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg width=""1500"" height=""1000"" viewBox=""0 0 3 2""><rect width=""3"" height=""2"" fill=""#009246""/><rect width=""2"" height=""2"" x=""1"" fill=""#fff""/><rect width=""1"" height=""2"" x=""2"" fill=""#ce2b37""/></svg>";

    public const string SvgFlagEs = @"<?xml version=""1.0"" encoding=""utf-8""?><svg width=""900"" height=""600""><rect width=""900"" height=""600"" fill=""#c60b1e""/><rect width=""900"" height=""300"" y=""150"" fill=""#ffc400""/></svg>";

    public const string SvgFlagJa = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg width=""900"" height=""600""><rect fill=""#fff"" height=""600"" width=""900""/><circle fill=""#bc002d"" cx=""450"" cy=""300"" r=""180""/></svg>";

    public const string SvgFlagKo = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""900"" height=""600"" viewBox=""-36 -24 72 48""><title>Flag of South Korea</title><rect fill=""#fff"" x=""-36"" y=""-24"" width=""72"" height=""48""/><g transform=""rotate(-56.3099325)""><g id=""b2""><path id=""b"" d=""M-6-25H6M-6-22H6M-6-19H6"" stroke=""#000"" stroke-width=""2""/><use xlink:href=""#b"" y=""44""/></g><path stroke=""#fff"" stroke-width=""1"" d=""M0,17v10""/><circle fill=""#cd2e3a"" r=""12""/><path fill=""#0047a0"" d=""M0-12A6,6 0 0 0 0,0A6,6 0 0 1 0,12A12,12 0 0,1 0-12Z""/></g><g transform=""rotate(-123.6900675)""><use xlink:href=""#b2""/><path stroke=""#fff"" stroke-width=""1"" d=""M0-23.5v3M0,17v3.5M0,23.5v3""/></g></svg>";

    public const string SvgFlagZh = @"<?xml version=""1.0"" encoding=""UTF-8""?><svg xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""900"" height=""600"" viewBox=""0 0 30 20""><defs><path id=""s"" d=""M0,-1 0.587785,0.809017 -0.951057,-0.309017H0.951057L-0.587785,0.809017z"" fill=""#FFFF00""/></defs><rect width=""30"" height=""20"" fill=""#EE1C25""/><use xlink:href=""#s"" transform=""translate(5,5) scale(3)""/><use xlink:href=""#s"" transform=""translate(10,2) rotate(23.036243)""/><use xlink:href=""#s"" transform=""translate(12,4) rotate(45.869898)""/><use xlink:href=""#s"" transform=""translate(12,7) rotate(69.945396)""/><use xlink:href=""#s"" transform=""translate(10,9) rotate(20.659808)""/></svg>";

    public const string SvgCircleClose = @"<svg viewBox=""0 0 512 512""><path d=""M256 512A256 256 0 1 0 256 0a256 256 0 1 0 0 512zM381.1 128L285.9 256l95.2 128-59.8 0L256 296.2 190.7 384l-59.8 0 95.2-128L130.9 128l59.8 0L256 215.8 321.3 128l59.8 0z""/></svg>";

    public const string SvgClose = @"<svg viewBox=""0 0 320 512""><path d=""M312.1 375c9.369 9.369 9.369 24.57 0 33.94s-24.57 9.369-33.94 0L160 289.9l-119 119c-9.369 9.369-24.57 9.369-33.94 0s-9.369-24.57 0-33.94L126.1 256L7.027 136.1c-9.369-9.369-9.369-24.57 0-33.94s24.57-9.369 33.94 0L160 222.1l119-119c9.369-9.369 24.57-9.369 33.94 0s9.369 24.57 0 33.94L193.9 256L312.1 375z""/></svg>";
}
