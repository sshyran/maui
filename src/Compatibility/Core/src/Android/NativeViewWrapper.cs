using Android.Views;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class PlatformViewWrapper : View
	{
		public PlatformViewWrapper(global::Android.Views.View nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null, OnLayoutDelegate onLayoutDelegate = null,
								 OnMeasureDelegate onMeasureDelegate = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			PlatformView = nativeView;
			OnLayoutDelegate = onLayoutDelegate;
			OnMeasureDelegate = onMeasureDelegate;

			nativeView.TransferBindablePropertiesToWrapper(this);
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public global::Android.Views.View PlatformView { get; }

		public OnLayoutDelegate OnLayoutDelegate { get; }

		public OnMeasureDelegate OnMeasureDelegate { get; }

		protected override void OnBindingContextChanged()
		{
			PlatformView.SetBindingContext(BindingContext, (view) => (view as ViewGroup)?.GetChildrenOfType<global::Android.Views.View>());
			base.OnBindingContextChanged();
		}
	}
}