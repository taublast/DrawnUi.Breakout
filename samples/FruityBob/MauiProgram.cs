﻿using Microsoft.Extensions.Logging;
using DrawnUi.Maui.Draw;

namespace FruityBob;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseDrawnUi()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddLogging(logging =>
		{
			logging.AddDebug();
		});
#endif

		return builder.Build();
	}
}
