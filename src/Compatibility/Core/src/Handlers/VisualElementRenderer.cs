﻿#nullable enable
#if WINDOWS || ANDROID || IOS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif IOS
using PlatformView = UIKit.UIView;
#endif

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#if WINDOWS
	public abstract partial class VisualElementRenderer<TElement, TNativeElement> : IPlatformViewHandler
		where TElement : VisualElement
		where TNativeElement : PlatformView
#else
	public abstract partial class VisualElementRenderer<TElement> : IPlatformViewHandler
		where TElement : Element, IView
#endif
	{
		public static IPropertyMapper<TElement, IPlatformViewHandler> VisualElementRendererMapper = new PropertyMapper<TElement, IPlatformViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.Background)] = MapBackground,
			[nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapAutomationPropertiesIsInAccessibleTree,
#if WINDOWS
			[AutomationProperties.NameProperty.PropertyName] = MapAutomationPropertiesName,
			[AutomationProperties.HelpTextProperty.PropertyName] = MapAutomationPropertiesHelpText,
			[AutomationProperties.LabeledByProperty.PropertyName] = MapAutomationPropertiesLabeledBy,
#endif
		};

		public static CommandMapper<TElement, IPlatformViewHandler> VisualElementRendererCommandMapper = new CommandMapper<TElement, IPlatformViewHandler>(ViewHandler.ViewCommandMapper);

		TElement? _virtualView;
		IMauiContext? _mauiContext;
		protected IPropertyMapper _mapper;
		protected CommandMapper? _commandMapper;
		protected readonly IPropertyMapper _defaultMapper;
		protected IMauiContext MauiContext => _mauiContext ?? throw new InvalidOperationException("MauiContext not set");
		public TElement? Element => _virtualView;
		protected bool AutoPackage { get; set; } = true;

#if ANDROID
		public VisualElementRenderer(Android.Content.Context context) : this(context, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}
#else
		public VisualElementRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}
#endif


#if ANDROID
		internal VisualElementRenderer(Android.Content.Context context, IPropertyMapper mapper, CommandMapper? commandMapper = null)
#else
		internal VisualElementRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
#endif

#if ANDROID
			: base(context)
#elif IOS
			: base(CoreGraphics.CGRect.Empty)
#else
			: base()
#endif
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>>? ElementChanged;
		public event EventHandler<PropertyChangedEventArgs>? ElementPropertyChanged;

		public void SetElement(IView view)
		{
			((IPlatformViewHandler)this).SetVirtualView(view);
		}

		partial void ElementChangedPartial(ElementChangedEventArgs<TElement> e);
		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			ElementChanged?.Invoke(this, e);
			ElementChangedPartial(e);
		}

		partial void ElementPropertyChangedPartial(object sender, PropertyChangedEventArgs e);

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element != null && e.PropertyName != null)
				_mapper.UpdateProperty(this, Element, e.PropertyName);

			ElementPropertyChanged?.Invoke(sender, e);
			ElementPropertyChangedPartial(sender, e);
		}


		internal static Size GetDesiredSize(IPlatformViewHandler handler, double widthConstraint, double heightConstraint, Size? minimumSize)
		{
			var size = handler.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

			if (minimumSize != null)
			{
				var minSize = minimumSize.Value;

				if (size.Height < minSize.Height || size.Width < minSize.Width)
				{
					return new Size(
							size.Width < minSize.Width ? minSize.Width : size.Width,
							size.Height < minSize.Height ? minSize.Height : size.Height
						);
				}
			}

			return size;
		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			GetDesiredSize(this, widthConstraint, heightConstraint, MinimumSize());

		protected virtual Size MinimumSize()
		{
			return new Size();
		}


#if IOS
		protected virtual void SetBackgroundColor(Color? color)
#else
		protected virtual void UpdateBackgroundColor()
#endif
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}

#if IOS
		protected virtual void SetBackground(Brush brush)
#else
		protected virtual void UpdateBackground()
#endif
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}


		protected virtual void SetAutomationId(string id)
		{
			if (Element != null)
				ViewHandler.MapAutomationId(this, Element);
		}

#if WINDOWS
		protected virtual void SetAutomationPropertiesAccessibilityView()
#else
		protected virtual void SetImportantForAccessibility()
#endif
		{
			if (Element != null)
				VisualElement.MapAutomationPropertiesIsInAccessibleTree(this, Element);
		}


		bool IViewHandler.HasContainer { get => true; set { } }

		object? IViewHandler.ContainerView => this;

		IView? IViewHandler.VirtualView => Element;

		Maui.IElement? IElementHandler.VirtualView => Element;

		IMauiContext? IElementHandler.MauiContext => _mauiContext;

		PlatformView? IPlatformViewHandler.PlatformView => (Element?.Handler as IElementHandler)?.PlatformView as PlatformView;

		PlatformView? IPlatformViewHandler.ContainerView => this;

		void IViewHandler.NativeArrange(Rectangle rect) =>
			this.NativeArrangeHandler(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		internal static void SetVirtualView(
			Maui.IElement view,
			IPlatformViewHandler nativeViewHandler,
			Action<ElementChangedEventArgs<TElement>> onElementChanged,
			ref TElement? currentVirtualView,
			ref IPropertyMapper _mapper,
			IPropertyMapper _defaultMapper,
			bool autoPackage)
		{
			if (currentVirtualView == view)
				return;

			var oldElement = currentVirtualView;
			currentVirtualView = view as TElement;
			onElementChanged?.Invoke(new ElementChangedEventArgs<TElement>(oldElement, currentVirtualView));

			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (oldElement?.Handler != null)
				oldElement.Handler = null;

			currentVirtualView = (TElement)view;

			if (currentVirtualView.Handler != nativeViewHandler)
				currentVirtualView.Handler = nativeViewHandler;

			_mapper = _defaultMapper;

			if (currentVirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				if (map is not null)
				{
					map.Chained = new[] { _defaultMapper };
					_mapper = map;
				}
			}

			if (autoPackage)
			{
				ProcessAutoPackage(view);
			}

			_mapper.UpdateProperties(nativeViewHandler, currentVirtualView);
		}

		static partial void ProcessAutoPackage(Maui.IElement element);

		void IElementHandler.SetVirtualView(Maui.IElement view) =>
			SetVirtualView(view, this, OnElementChanged, ref _virtualView, ref _mapper, _defaultMapper, AutoPackage);

		void IElementHandler.UpdateValue(string property)
		{
			if (Element != null)
			{
				OnElementPropertyChanged(Element, new PropertyChangedEventArgs(property));
			}
		}

		void IElementHandler.Invoke(string command, object? args)
		{
			_commandMapper?.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			DisconnectHandlerCore();
			if (Element != null && Element.Handler == (IPlatformViewHandler)this)
				Element.Handler = null;

			_virtualView = null;
		}

		private protected virtual void DisconnectHandlerCore()
		{

		}

		public static void MapAutomationPropertiesIsInAccessibleTree(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
				ver.SetAutomationPropertiesAccessibilityView();
#else
			if (handler is VisualElementRenderer<TElement> ver)
				ver.SetImportantForAccessibility();
#endif
		}

		public static void MapAutomationId(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.SetAutomationId(view.AutomationId);
		}

		public static void MapBackgroundColor(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
#if IOS
				ver.SetBackgroundColor(view.Background?.ToColor());
#else
				ver.UpdateBackgroundColor();
#endif
		}

		public static void MapBackground(IPlatformViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
#if IOS
				ver.SetBackground(view.Background);
#else
				ver.UpdateBackground();
#endif
		}
	}
}
#endif