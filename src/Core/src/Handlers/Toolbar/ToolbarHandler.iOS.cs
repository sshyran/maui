﻿using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		protected override UINavigationBar CreateNativeElement()
		{
			throw new System.NotImplementedException();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}
