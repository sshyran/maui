﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentViewGroup GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.NativeView;

		Task ValidateHasColor(IBorderView border, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeBorder = GetNativeBorder(CreateHandler(border));
				action?.Invoke();
				nativeBorder.AssertContainsColor(color);
			});
		}
	}
}
