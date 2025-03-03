using System.Globalization;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class AppInfoImplementation
	{
		public string PackageName
			=> Application.Current.ApplicationInfo.PackageId;

		public string Name
			=> Application.Current.ApplicationInfo.Label;

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString
			=> Platform.CurrentPackage.Version;

		public string BuildString
			=> Version.Build.ToString(CultureInfo.InvariantCulture);

		public void PlatformShowSettingsUI()
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();
			AppControl.SendLaunchRequest(new AppControl() { Operation = AppControlOperations.Setting });
		}

		public AppTheme RequestedTheme
			=> AppTheme.Unspecified;
	}
}
